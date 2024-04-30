using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

namespace Boids
{
    [UpdateAfter(typeof(ExportPhysicsWorld))]
    public partial struct BoidsEcsSystem : ISystem, ISystemStartStop
    {
        SpatialHashGridBuilder hashGridBuilder;
        NativeArray<int> cellIndices;
        NativeArray<int> hashTable;

        SpawnDataBuilder spawnDataBuilder;

        Entity boidPrefab;
        SpawnData spawnData;
        BehaviourData behaviourData;
        MovementData movementData;

        RuleJobDataBuilder ruleJobDataBuilder;
        NativeArray<CPosition> positions;
        NativeArray<CRotation> rotations;
        NativeArray<CSpeed> speeds;

        NativeArray<Random> randoms;

        EntityQuery boidsQuery;

        ComponentTypeHandle<LocalTransform> transformHandle;
        ComponentTypeHandle<CSpeed> speedHandle;

        JobHandle rulesDataBuilderHandle;
        JobHandle initializeBoidsHandle;
        JobHandle applyRulesHandle;
        JobHandle moveBoidsHandle;
        JobHandle systemDependency;

        BuildPhysicsWorld buildPhysicsWorld;


        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CBoidsConfig>();
            boidsQuery = SystemAPI.QueryBuilder().WithAspect<BoidAspect>().Build();
            //buildPhysicsWorld = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<BuildPhysicsWorld>();
        }

        [BurstCompile]
        public void OnStartRunning(ref SystemState state)
        {
            //this.RegisterPhysicsRuntimeSystemReadWrite();

            boidPrefab = SystemAPI.GetSingleton<CBoidsConfig>().boidPrefabEntity;
            spawnData = SystemAPI.GetSingleton<CBoidsConfig>().spawnData.Value;
            behaviourData = SystemAPI.GetSingleton<CBoidsConfig>().behaviourData.Value;
            movementData = SystemAPI.GetSingleton<CBoidsConfig>().movementData.Value;

            Initialize(ref state);
            SpawnBoids(ref state);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency.Complete();

            transformHandle.Update(ref state);
            speedHandle.Update(ref state);
            rulesDataBuilderHandle = ruleJobDataBuilder.Gather(ref state, transformHandle, speedHandle, new JobHandle(), ref positions, ref rotations, ref speeds);
            rulesDataBuilderHandle.Complete();

            NativeArray<int3> pivots;
            hashGridBuilder.Build(in positions, behaviourData, out pivots, ref cellIndices, ref hashTable);

            applyRulesHandle = new ApplyRulesJob
            {
                behaviourData = this.behaviourData,

                positions = this.positions,
                rotations = this.rotations,
                speeds = this.speeds,

                boundsMin = hashGridBuilder.boundsMin,
                conversionFactor = hashGridBuilder.conversionFactor,
                cellCountAxis = hashGridBuilder.cellCountAxis,
                cellCountXY = hashGridBuilder.cellCountXY,

                pivots = pivots,
                hashTable = this.hashTable,
                cellIndices = this.cellIndices
            }.ScheduleParallel(boidsQuery, state.Dependency);

            float3 swarmObjective = new float3();
            moveBoidsHandle = new MoveBoidsJob
            {
                deltaTime = SystemAPI.Time.DeltaTime,
                maxSpeed = movementData.maxSpeed,
                minSpeed = movementData.minSpeed,
                swarmCenter = GetSwarmCenter(),
                swarmObjective = swarmObjective,
                objectiveCenterRatio = behaviourData.objectiveCenterRatio,
                speedMulRules = movementData.speedMulRules,
                maxRadiansRandom = (behaviourData.DirectionRandomness/360f) * math.PI * 2f,
                randoms = this.randoms
            }
            .ScheduleParallel(boidsQuery, applyRulesHandle);

            moveBoidsHandle.Complete();
            pivots.Dispose();
        }

        public void OnDestroy(ref SystemState state)
        {
            cellIndices.Dispose();
            hashTable.Dispose();
            positions.Dispose();
            rotations.Dispose();
            speeds.Dispose();
            randoms.Dispose();
        }

        private void Initialize(ref SystemState state)
        {
            cellIndices = new NativeArray<int>(spawnData.boidCount, Allocator.Persistent);
            hashTable = new NativeArray<int>(spawnData.boidCount, Allocator.Persistent);
            positions = new NativeArray<CPosition>(spawnData.boidCount, Allocator.Persistent);
            rotations = new NativeArray<CRotation>(spawnData.boidCount, Allocator.Persistent);
            speeds = new NativeArray<CSpeed>(spawnData.boidCount, Allocator.Persistent);

            randoms = new NativeArray<Random>(JobsUtility.MaxJobThreadCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            for (int i = 0; i < JobsUtility.MaxJobThreadCount; i++)
            {
                randoms[i] = new Random((uint)UnityEngine.Random.Range(int.MinValue, int.MaxValue));
            }

            spawnDataBuilder = new SpawnDataBuilder();
            spawnDataBuilder.GenerateCubeSpawnData(in spawnData, out positions, out rotations, Allocator.Persistent);

            hashGridBuilder = new SpatialHashGridBuilder();

            transformHandle = state.GetComponentTypeHandle<LocalTransform>(true);
            speedHandle = state.GetComponentTypeHandle<CSpeed>(true);
            ruleJobDataBuilder = new RuleJobDataBuilder(boidsQuery);
        }

        private void SpawnBoids(ref SystemState state)
        {
            state.EntityManager.Instantiate(boidPrefab, spawnData.boidCount, Allocator.Persistent);

            initializeBoidsHandle = new InitializeBoids()
            {
                startSpeed = movementData.startSpeed,
                startAngularSpeed = (movementData.startAngularSpeed/360f) * 2f * math.PI,
                positions = this.positions,
                rotations = this.rotations,

            }.ScheduleParallel(boidsQuery, new JobHandle());

            initializeBoidsHandle.Complete();
        }

        private float3 GetSwarmCenter()
        {
            float3 result = new float3();

            for(int i = 0; i < positions.Length; i++)
            {
                result += positions[i].value;
            }
            return result / positions.Length;
        }

        public void OnStopRunning(ref SystemState state) { }
    }

    [BurstCompile]
    partial struct InitializeBoids : IJobEntity
    {
        [ReadOnly] public float startSpeed;
        [ReadOnly] public float startAngularSpeed;
        [ReadOnly] public NativeArray<CPosition> positions;
        [ReadOnly] public NativeArray<CRotation> rotations;

        [BurstCompile]
        public void Execute([EntityIndexInQuery] int boidIndex, ref LocalTransform transform, ref CSpeed speed, ref CAngularSpeed angularSpeed)
        {
            speed.value = startSpeed;
            angularSpeed.value = startAngularSpeed;
            transform = LocalTransform.FromPositionRotationScale(positions[boidIndex].value, rotations[boidIndex].value, 0.01f);
        }
    }
}

