using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Transforms;

namespace Boids
{
    //[UpdateAfter(typeof(ExportPhysicsWorld))]
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

        BoidEnemyConfig enemyConfig;

        NativeArray<Entity> boids;
        NativeArray<Entity> enemyTargets;

        RuleJobDataBuilder ruleJobDataBuilder;
        NativeArray<RuleData> ruleData;

        NativeArray<Random> randoms;

        EntityQuery boidsQuery;
        EntityQuery boidsEnemyQuery;

        ComponentTypeHandle<LocalTransform> transformHandle;
        ComponentTypeHandle<CSpeed> speedHandle;

        JobHandle rulesDataBuilderHandle;
        JobHandle initializeBoidsHandle;
        JobHandle applyRulesHandle;
        JobHandle moveBoidsHandle;


        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CBoidsConfig>();
            boidsQuery = SystemAPI.QueryBuilder().WithAspect<BoidAspect>().Build();
            boidsEnemyQuery = SystemAPI.QueryBuilder().WithAspect<BoidEnemyAspect>().Build();
        }

        [BurstCompile]
        public void OnStartRunning(ref SystemState state)
        {
            boidPrefab = SystemAPI.GetSingleton<CBoidsConfig>().boidPrefabEntity;
            spawnData = SystemAPI.GetSingleton<CBoidsConfig>().spawnData.Value;
            behaviourData = SystemAPI.GetSingleton<CBoidsConfig>().behaviourData.Value;
            movementData = SystemAPI.GetSingleton<CBoidsConfig>().movementData.Value;

            enemyConfig = SystemAPI.GetSingleton<CBoidEnemyConfig>().Config;

            Initialize(ref state);
            SpawnBoids(ref state);
            //InitializeEnemies(ref state);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency.Complete();

            transformHandle.Update(ref state);
            speedHandle.Update(ref state);
            rulesDataBuilderHandle = ruleJobDataBuilder.Gather(ref state, transformHandle, speedHandle, ref ruleData, state.Dependency);
            rulesDataBuilderHandle.Complete();

            NativeArray<int3> pivots;
            hashGridBuilder.Build(in ruleData, behaviourData, out pivots, ref cellIndices, ref hashTable);

            //UpdateBoids(in pivots, ref state);
            applyRulesHandle = new ApplyRulesJob
            {
                behaviourData = this.behaviourData,

                ruleData = this.ruleData,

                boundsMin = hashGridBuilder.boundsMin,
                conversionFactor = hashGridBuilder.conversionFactor,
                cellCountAxis = hashGridBuilder.cellCountAxis,
                cellCountXY = hashGridBuilder.cellCountXY,

                pivots = pivots,
                hashTable = this.hashTable,
                cellIndices = this.cellIndices
            }.ScheduleParallel(boidsQuery, rulesDataBuilderHandle);

            float3 swarmObjective = new float3(0, 0, 0);
            moveBoidsHandle = new MoveBoidsJob
            {
                deltaTime = SystemAPI.Time.DeltaTime,
                maxSpeed = movementData.maxSpeed,
                minSpeed = movementData.minSpeed,
                swarmCenter = GetSwarmCenter(),
                swarmObjective = swarmObjective,
                objectiveCenterRatio = behaviourData.objectiveCenterRatio,
                speedMulRules = movementData.speedMulRules,
                maxRadiansRandom = (behaviourData.DirectionRandomness / 360f) * math.PI * 2f,
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
            ruleData.Dispose();
            randoms.Dispose();
            enemyTargets.Dispose();
        }

        [BurstCompile]
        private void Initialize(ref SystemState state)
        {
            cellIndices = new NativeArray<int>(spawnData.boidCount, Allocator.Persistent);
            hashTable = new NativeArray<int>(spawnData.boidCount, Allocator.Persistent);
            ruleData = new NativeArray<RuleData>(spawnData.boidCount, Allocator.Persistent);

            boids = new NativeArray<Entity>(spawnData.boidCount, Allocator.Persistent);
            //enemyTargets = new NativeArray<Entity>(boidsEnemyQuery.CalculateEntityCount(), Allocator.Persistent);

            randoms = new NativeArray<Random>(JobsUtility.MaxJobThreadCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            for (int i = 0; i < JobsUtility.MaxJobThreadCount; i++)
            {
                randoms[i] = new Random((uint)UnityEngine.Random.Range(int.MinValue, int.MaxValue));
            }

            spawnDataBuilder = new SpawnDataBuilder();
            spawnDataBuilder.GenerateCubeSpawnData(in spawnData, out ruleData, Allocator.Persistent);

            hashGridBuilder = new SpatialHashGridBuilder();

            transformHandle = state.GetComponentTypeHandle<LocalTransform>(true);
            speedHandle = state.GetComponentTypeHandle<CSpeed>(true);
            ruleJobDataBuilder = new RuleJobDataBuilder(boidsQuery);
        }

        [BurstCompile]
        private void SpawnBoids(ref SystemState state)
        {
            state.EntityManager.Instantiate(boidPrefab, boids);

            initializeBoidsHandle = new InitializeBoidsJob()
            {
                startSpeed = movementData.startSpeed,
                startAngularSpeed = (movementData.startAngularSpeed/360f) * 2f * math.PI,
                RuleData = this.ruleData
            }.ScheduleParallel(boidsQuery, state.Dependency);

            initializeBoidsHandle.Complete();
        }

        [BurstCompile]
        private void InitializeEnemies(ref SystemState state)
        {
            for (int i = 0; i < enemyTargets.Length; i++)
            {
                enemyTargets[i] = boids[UnityEngine.Random.Range(0, boids.Length)];
            }

            new InitializeEnemiesJob
            {
                speed = enemyConfig.speed,
                angularSpeed = enemyConfig.angularSpeed,
            }.ScheduleParallel(boidsEnemyQuery, new JobHandle());
        }

        [BurstCompile]
        private void UpdateBoids(in NativeArray<int3> pivots, ref SystemState state)
        {
            applyRulesHandle = new ApplyRulesJob
            {
                behaviourData = this.behaviourData,

                ruleData = this.ruleData,

                boundsMin = hashGridBuilder.boundsMin,
                conversionFactor = hashGridBuilder.conversionFactor,
                cellCountAxis = hashGridBuilder.cellCountAxis,
                cellCountXY = hashGridBuilder.cellCountXY,

                pivots = pivots,
                hashTable = this.hashTable,
                cellIndices = this.cellIndices
            }.ScheduleParallel(boidsQuery, rulesDataBuilderHandle);

            float3 swarmObjective = new float3(0, 0, 0);
            moveBoidsHandle = new MoveBoidsJob
            {
                deltaTime = SystemAPI.Time.DeltaTime,
                maxSpeed = movementData.maxSpeed,
                minSpeed = movementData.minSpeed,
                swarmCenter = GetSwarmCenter(),
                swarmObjective = swarmObjective,
                objectiveCenterRatio = behaviourData.objectiveCenterRatio,
                speedMulRules = movementData.speedMulRules,
                maxRadiansRandom = (behaviourData.DirectionRandomness / 360f) * math.PI * 2f,
                randoms = this.randoms
            }
            .ScheduleParallel(boidsQuery, applyRulesHandle);

            moveBoidsHandle.Complete();
        }

        private void UpdateEnemies(ref SystemState state)
        {
            NativeArray<float3> targetPositions = new NativeArray<float3>(enemyTargets.Length, Allocator.TempJob);

            for (int i = 0; i < enemyTargets.Length; i++)
            {
                targetPositions[i] = state.EntityManager.GetComponentData<LocalTransform>(enemyTargets[i]).Position;
            }

            JobHandle dependency = new MoveEnemiesJob
            {
                targetPositions = targetPositions,
                deltaTime = SystemAPI.Time.DeltaTime,
                speed = enemyConfig.speed,
                angularSpeed = enemyConfig.angularSpeed
            }.Schedule(state.Dependency);

            dependency.Complete();
            targetPositions.Dispose();
        }

        [BurstCompile]
        private float3 GetSwarmCenter()
        {
            float3 result = new float3();

            for(int i = 0; i < ruleData.Length; i++)
            {
                result += ruleData[i].position;
            }
            return result / ruleData.Length;
        }

        public void OnStopRunning(ref SystemState state) { }
    }

    [BurstCompile]
    partial struct InitializeBoidsJob : IJobEntity
    {
        [ReadOnly] public float startSpeed;
        [ReadOnly] public float startAngularSpeed;
        [ReadOnly] public NativeArray<RuleData> RuleData;

        [BurstCompile]
        public void Execute([EntityIndexInQuery] int boidIndex, ref LocalTransform transform, ref CSpeed speed, ref CAngularSpeed angularSpeed)
        {
            speed.value = startSpeed;
            angularSpeed.value = startAngularSpeed;
            transform = LocalTransform.FromPositionRotationScale(RuleData[boidIndex].position, RuleData[boidIndex].rotation, 0.01f);
        }
    }

    [BurstCompile]
    partial struct InitializeEnemiesJob : IJobEntity
    {
        [ReadOnly] public float speed;
        [ReadOnly] public float angularSpeed;

        [BurstCompile]
        public void Execute([EntityIndexInQuery] int enemyIndex, ref CSpeed speed, ref CAngularSpeed angularSpeed)
        {
            speed.value = this.speed;
            angularSpeed.value = this.angularSpeed;
        }
    }
}

