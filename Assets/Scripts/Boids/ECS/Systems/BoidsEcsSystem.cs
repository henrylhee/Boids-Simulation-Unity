using System.Collections;
using System.Diagnostics;
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

        NativeArray<int> enemyTargetIndices;
        NativeArray<float3> enemyTargetPositions;

        RuleJobDataBuilder ruleJobDataBuilder;
        NativeArray<RuleData> ruleData;

        NativeArray<Random> randoms;

        EntityQuery boidsQuery;
        EntityQuery boidsEnemyQuery;

        ComponentTypeHandle<LocalTransform> transformHandle;
        ComponentTypeHandle<CSpeed> speedHandle;


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
            InitializeEnemies(ref state);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            transformHandle.Update(ref state);
            speedHandle.Update(ref state);
            ruleJobDataBuilder.Gather(ref state, transformHandle, speedHandle, ref ruleData, state.Dependency)
            .Complete();

            //UnityEngine.Profiling.Profiler.BeginSample("hashgridbuilder");
            hashGridBuilder.SetUp(in behaviourData, in ruleData);
            NativeArray<int3> pivots = new NativeArray<int3>(hashGridBuilder.cellCountXYZ, Allocator.TempJob);
            hashGridBuilder.Build(in ruleData, ref pivots, ref cellIndices, ref hashTable);
            //UnityEngine.Profiling.Profiler.EndSample();
            //UpdateBoids(in pivots, ref state);
            new ApplyRulesJob
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
            }.ScheduleParallel(boidsQuery, state.Dependency)
            .Complete();
            pivots.Dispose();

            float3 swarmObjective = new float3(0, 0, 0);
            for (int i = 0; i < randoms.Length; i++)
            {
                randoms[i] = new Random((uint)UnityEngine.Random.Range(int.MinValue, int.MaxValue));
            }
            new MoveBoidsJob
            {
                deltaTime = SystemAPI.Time.DeltaTime,
                maxSpeed = movementData.maxSpeed,
                minSpeed = movementData.minSpeed,
                swarmCenter = GetSwarmCenter(),
                swarmObjective = swarmObjective,
                objectiveCenterRatio = behaviourData.objectiveCenterRatio,
                speedMulRules = movementData.speedMulRules,
                maxRadiansRandom = (behaviourData.DirectionRandomness / 360f) * math.PI * 2f,
                randoms = this.randoms,
            }
            .ScheduleParallel(boidsQuery, state.Dependency)
            .Complete();

            UpdateEnemies(ref state);
        }

        [BurstCompile]
        private void Initialize(ref SystemState state)
        {
            cellIndices = new NativeArray<int>(spawnData.boidCount, Allocator.Persistent);
            hashTable = new NativeArray<int>(spawnData.boidCount, Allocator.Persistent);
            ruleData = new NativeArray<RuleData>(spawnData.boidCount, Allocator.Persistent);

            enemyTargetIndices = new NativeArray<int>(boidsEnemyQuery.CalculateEntityCount(), Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            SetRandomEnemyTargets();
            enemyTargetPositions = new NativeArray<float3>(boidsEnemyQuery.CalculateEntityCount(), Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            randoms = new NativeArray<Random>(JobsUtility.MaxJobThreadCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            spawnDataBuilder = new SpawnDataBuilder();

            hashGridBuilder = new SpatialHashGridBuilder();

            transformHandle = state.GetComponentTypeHandle<LocalTransform>(true);
            speedHandle = state.GetComponentTypeHandle<CSpeed>(true);
            ruleJobDataBuilder = new RuleJobDataBuilder(boidsQuery);
        }

        [BurstCompile]
        private void SpawnBoids(ref SystemState state)
        {
            for(int i = 0; i < spawnData.boidCount; i++)
            {
                state.EntityManager.Instantiate(boidPrefab);
            }
            NativeArray<float3> positions = new NativeArray<float3>(spawnData.boidCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);
            NativeArray<quaternion> rotations = new NativeArray<quaternion>(spawnData.boidCount, Allocator.TempJob, NativeArrayOptions.UninitializedMemory);

            spawnDataBuilder.GenerateCubeSpawnData(in spawnData, ref positions, ref rotations);

            new InitializeBoidsJob()
            {
                startSpeed = movementData.startSpeed,
                startAngularSpeed = (movementData.startAngularSpeed/360f) * 2f * math.PI,
                positions = positions,
                rotations = rotations
            }.ScheduleParallel(boidsQuery, state.Dependency)
            .Complete();

            positions.Dispose();
            rotations.Dispose();
        }

        [BurstCompile]
        private void InitializeEnemies(ref SystemState state)
        {
            new InitializeEnemiesJob
            {
                speed = enemyConfig.speed,
                angularSpeed = enemyConfig.angularSpeed
            }.ScheduleParallel(boidsEnemyQuery, state.Dependency)
            .Complete();
        }

        [BurstCompile]
        private void SetRandomEnemyTargets()
        {
            for (int i = 0; i < enemyTargetIndices.Length; i++)
            {
                enemyTargetIndices[i] = UnityEngine.Random.Range(0, spawnData.boidCount);
            }
        }

        [BurstCompile]
        private void UpdateBoids(in NativeArray<int3> pivots, ref SystemState state)
        {
            new ApplyRulesJob
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
            }.ScheduleParallel(boidsQuery, state.Dependency)
            .Complete();

            float3 swarmObjective = new float3(0, 0, 0);
            new MoveBoidsJob
            {
                deltaTime = SystemAPI.Time.DeltaTime,
                maxSpeed = movementData.maxSpeed,
                minSpeed = movementData.minSpeed,
                swarmCenter = GetSwarmCenter(),
                swarmObjective = swarmObjective,
                objectiveCenterRatio = behaviourData.objectiveCenterRatio,
                speedMulRules = movementData.speedMulRules,
                maxRadiansRandom = (behaviourData.DirectionRandomness / 360f) * math.PI * 2f,
                randoms = this.randoms,
                enemyTargetPositions = this.enemyTargetPositions                
            }
            .ScheduleParallel(boidsQuery, state.Dependency)
            .Complete();
        }

        private void UpdateEnemies(ref SystemState state)
        {
            for (int i = 0; i < enemyTargetIndices.Length; i++)
            {
                enemyTargetPositions[i] = ruleData[enemyTargetIndices[i]].position;
            }

            new MoveEnemiesJob
            {
                targetPositions = enemyTargetPositions,
                deltaTime = SystemAPI.Time.DeltaTime,
                speed = enemyConfig.speed,
                angularSpeed = enemyConfig.angularSpeed
            }.Schedule(boidsEnemyQuery, state.Dependency)
            .Complete();
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


        public void OnDestroy(ref SystemState state)
        {
            cellIndices.Dispose();
            hashTable.Dispose();
            ruleData.Dispose();
            randoms.Dispose();
            enemyTargetIndices.Dispose();
            enemyTargetPositions.Dispose();
        }
    }


    [BurstCompile]
    partial struct InitializeBoidsJob : IJobEntity
    {
        [ReadOnly] public float startSpeed;
        [ReadOnly] public float startAngularSpeed;
        [ReadOnly] public NativeArray<float3> positions;
        [ReadOnly] public NativeArray<quaternion> rotations;


        [BurstCompile]
        public void Execute([EntityIndexInQuery] int boidIndex, ref LocalTransform transform, ref CSpeed speed, ref CAngularSpeed angularSpeed)
        {
            speed.value = startSpeed;
            angularSpeed.value = startAngularSpeed;
            transform = LocalTransform.FromPositionRotationScale(positions[boidIndex], rotations[boidIndex], transform.Scale);
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

