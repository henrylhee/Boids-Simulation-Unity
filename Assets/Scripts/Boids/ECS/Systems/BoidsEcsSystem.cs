using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using Unity.Mathematics;
using Unity.Physics;
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

        int swarmTargetIndex;
        NativeArray<float3> swarmTargetPositions;

        NativeArray<int> boidTargetIndices;
        NativeArray<float3> boidTargetPositions;

        NativeArray<LocalTransform> enemyTransforms;
        float boidVisionRadius;
        float enemyScale;
        Timer enemyChaseTimer;

        float3 swarmCenter;
        float maxDistanceBoidToCenter;

        NativeArray<BoidData> boidData;
        NativeArray<ObstacleData> boidObstacleData;

        NativeArray<Unity.Mathematics.Random> randoms;

        EntityQuery boidsQuery;
        EntityQuery boidsEnemyQuery;
        EntityQuery swarmTargetsQuery;
        EntityQuery boidObstacleQuery;


        [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CBoidsConfig>();
            boidsQuery = SystemAPI.QueryBuilder().WithAspect<BoidAspect>().Build();
            boidsEnemyQuery = SystemAPI.QueryBuilder().WithAspect<BoidEnemyAspect>().Build();
            swarmTargetsQuery = SystemAPI.QueryBuilder().WithAll<CSwarmTarget>().WithAll<LocalToWorld>().Build();
            boidObstacleQuery = SystemAPI.QueryBuilder().WithAll<CBoidObstacleTag>().Build();
        }

        [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
        public void OnStartRunning(ref SystemState state)
        {
            Initialize(ref state);
            InitializeBoids(ref state);
            SpawnBoids(ref state);
            InitializeEnemies(ref state);
        }

        [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
        public void OnUpdate(ref SystemState state)
        {
            behaviourData = SystemAPI.GetSingleton<CBoidsConfig>().behaviourData.Value;
            movementData = SystemAPI.GetSingleton<CBoidsConfig>().movementData.Value;

            new GatherBoidDataJob
            {
                boidData = boidData,
            }
            .ScheduleParallel(boidsQuery, state.Dependency)
            .Complete();

            hashGridBuilder.SetUp(in behaviourData, in boidData);
            NativeArray<int3> pivots = new NativeArray<int3>(hashGridBuilder.cellCountXYZ, Allocator.TempJob);
            hashGridBuilder.Build(in boidData, ref pivots, ref cellIndices, ref hashTable);

            UpdateBoidObstacleCollision(ref state, in pivots);
            UpdateBoids(in pivots, ref state);
            UpdateEnemies(ref state);
            pivots.Dispose();
        }

        [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
        private void Initialize(ref SystemState state)
        {
            boidPrefab = SystemAPI.GetSingleton<CBoidsConfig>().boidPrefabEntity;
            behaviourData = SystemAPI.GetSingleton<CBoidsConfig>().behaviourData.Value;
            movementData = SystemAPI.GetSingleton<CBoidsConfig>().movementData.Value;
            spawnData = SystemAPI.GetSingleton<CBoidsConfig>().spawnData.Value;
            enemyConfig = SystemAPI.GetSingleton<CBoidEnemyConfig>().Config;

            cellIndices = new NativeArray<int>(spawnData.boidCount, Allocator.Persistent);
            hashTable = new NativeArray<int>(spawnData.boidCount, Allocator.Persistent);
            boidData = new NativeArray<BoidData>(spawnData.boidCount, Allocator.Persistent);
            boidObstacleData = new NativeArray<ObstacleData>(spawnData.boidCount, Allocator.Persistent);

            boidTargetIndices = new NativeArray<int>(boidsEnemyQuery.CalculateEntityCount(), Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
            boidTargetPositions = new NativeArray<float3>(boidsEnemyQuery.CalculateEntityCount(), Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            randoms = new NativeArray<Unity.Mathematics.Random>(JobsUtility.MaxJobThreadCount, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);

            spawnDataBuilder = new SpawnDataBuilder();
            hashGridBuilder = new SpatialHashGridBuilder();

            enemyChaseTimer = new Timer();
            enemyChaseTimer.Initialize(enemyConfig.chaseTime);
        }

        [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
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
            }
            .ScheduleParallel(boidsQuery, state.Dependency)
            .Complete();

            positions.Dispose();
            rotations.Dispose();
        }

        [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
        private void InitializeEnemies(ref SystemState state)
        {
            enemyTransforms = new NativeArray<LocalTransform>(boidTargetIndices.Length, Allocator.Persistent);
            enemyScale = boidsEnemyQuery.ToComponentDataArray<LocalTransform>(Allocator.Temp)[0].Scale;
            SetRandomEnemyTargets();

            new InitializeEnemiesJob
            {
                speed = enemyConfig.speed,
                angularSpeed = enemyConfig.angularSpeed,
                enemyTransforms = this.enemyTransforms
            }
            .ScheduleParallel(boidsEnemyQuery, state.Dependency)
            .Complete();
        }

        [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
        private void InitializeBoids(ref SystemState state)
        {
            boidVisionRadius = behaviourData.enemyVisionRadius;

            swarmTargetIndex = 0;
            swarmTargetPositions = new NativeArray<float3>(swarmTargetsQuery.CalculateEntityCount(), Allocator.Persistent);
            swarmCenter = new float3();
            maxDistanceBoidToCenter = 0;

            new InitializeSwarmTargetPositionsJob()
            {
                swarmTargetPositions = this.swarmTargetPositions,
            }
            .Schedule(swarmTargetsQuery);
        }

        [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
        private void SetRandomEnemyTargets()
        {
            for (int i = 0; i < boidTargetIndices.Length; i++)
            {
                boidTargetIndices[i] = UnityEngine.Random.Range(0, spawnData.boidCount);
            }
        }

        [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
        private void UpdateBoids(in NativeArray<int3> pivots, ref SystemState state)
        {
            GetSwarmCenter();
            float deltaTime = SystemAPI.Time.DeltaTime;

            new ApplyRulesJob
            {
                behaviourData = this.behaviourData,
                speedTowardsObjective = movementData.speedTowardsObjective * deltaTime,

                boidData = this.boidData,

                boundsMin = hashGridBuilder.boundsMin,
                conversionFactor = hashGridBuilder.conversionFactor,
                cellCountAxis = hashGridBuilder.cellCountAxis,
                cellCountXY = hashGridBuilder.cellCountXY,

                swarmCenter = swarmCenter,
                maxDistanceCenter = maxDistanceBoidToCenter,
                swarmObjective = swarmTargetPositions[swarmTargetIndex],

                pivots = pivots,
                hashTable = this.hashTable,
                cellIndices = this.cellIndices,
            }
            .ScheduleParallel(boidsQuery, state.Dependency)
            .Complete();

            for (int i = 0; i < randoms.Length; i++)
            {
                randoms[i] = new Unity.Mathematics.Random((uint)UnityEngine.Random.Range(int.MinValue, int.MaxValue));
            }

            float speedFactor = deltaTime * movementData.speedMul;
            new MoveBoidsJob
            {
                speedFactor = speedFactor,
                maxSpeed = movementData.maxSpeed * speedFactor,
                maxSpeedFleeing = movementData.maxSpeedFleeing * speedFactor,
                minSpeed = movementData.minSpeed * speedFactor,
                acceleration = movementData.acceleration * deltaTime,
                accelerationFleeing = movementData.accelerationFleeing * deltaTime,
                maxRadiansRandom = (behaviourData.DirectionRandomness / 360f) * math.PI * 2f,
                boidVisionRadius = this.boidVisionRadius,
                boidEnemyMaxDistance = this.boidVisionRadius + enemyScale,

                randoms = this.randoms,
                enemyTransforms = this.enemyTransforms,
            }
            .ScheduleParallel(boidsQuery, state.Dependency)
            .Complete();
        }

        private void UpdateEnemies(ref SystemState state)
        {
            enemyChaseTimer.Update(SystemAPI.Time.DeltaTime);
            if (enemyChaseTimer.isFinished)
            {
                enemyChaseTimer.Restart();
                SetRandomEnemyTargets();
            }

            for (int i = 0; i < boidTargetIndices.Length; i++)
            {
                boidTargetPositions[i] = boidData[boidTargetIndices[i]].position;
            }
            
            new MoveEnemiesJob
            {
                targetPositions = boidTargetPositions,
                enemyTransforms = this.enemyTransforms,
                deltaTime = SystemAPI.Time.DeltaTime,
                speed = enemyConfig.speed,
                angularSpeed = enemyConfig.angularSpeed
            }
            .Schedule(boidsEnemyQuery, state.Dependency)
            .Complete();
        }

        [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
        private void GetSwarmCenter()
        {
            swarmCenter = new float3();
            maxDistanceBoidToCenter = 0;

            for(int i = 0; i < boidData.Length; i++)
            {
                swarmCenter += boidData[i].position;
            }
            swarmCenter /= boidData.Length;

            for (int i = 0; i < boidData.Length; i++)
            {
                float distance = math.distance(swarmCenter, boidData[i].position);
                if(distance > maxDistanceBoidToCenter) { maxDistanceBoidToCenter = distance; }
            }
        }

        [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
        private void UpdateSwarmTargetPosition(float3 swarmCenter)
        {
            float3 offsetPosition = new float3(behaviourData.swarmTargetRadius, 
                                               behaviourData.swarmTargetRadius, 
                                               behaviourData.swarmTargetRadius);
            float3 minPosition = swarmCenter - offsetPosition;
            float3 maxPosition = swarmCenter + offsetPosition;
            float3 swarmTargetPosition = swarmTargetPositions[swarmTargetIndex];

            if(swarmTargetPosition.x < minPosition.x || swarmTargetPosition.x > maxPosition.x) { return; }
            if (swarmTargetPosition.y < minPosition.y || swarmTargetPosition.y > maxPosition.y) { return; }
            if (swarmTargetPosition.z < minPosition.z || swarmTargetPosition.z > maxPosition.z) { return; }

            if(swarmTargetIndex < swarmTargetsQuery.CalculateEntityCount() - 1)
            {
                swarmTargetIndex++;
            }
            else
            {
                swarmTargetIndex = 0;
            }
        }

        [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
        private void UpdateBoidObstacleCollision(ref SystemState state, in NativeArray<int3>  pivots)
        {
            int obstacleCount = boidObstacleQuery.CalculateEntityCount();
            NativeArray<bool> hasOverlapObstacles = new NativeArray<bool>(obstacleCount, Allocator.TempJob);
            NativeArray<MinMaxAABB> localObstacleAABBsExtended = new NativeArray<MinMaxAABB>(obstacleCount, Allocator.TempJob);
            MinMaxAABB localHashMapAABBs = new MinMaxAABB
            {
                Min = new float3(0,0,0),
                Max = hashGridBuilder.boundsMax - hashGridBuilder.boundsMin
            };

            new FindObstacleHashMapOverlapsJob
            {
                localHashMapAABBs = localHashMapAABBs,
                localObstacleAABBsExtended = localObstacleAABBsExtended,
                hasOverlapObstacles = hasOverlapObstacles,
                hashMapMin = hashGridBuilder.boundsMin,
                obstacleInteractionRadius = behaviourData.obstacleInteractionRadius
            }
            .ScheduleParallel(boidObstacleQuery, new JobHandle())
            .Complete();

            float conversionFactor = 1 / behaviourData.visionRange;
            for (int i = 0; i < obstacleCount; i++)
            {
                if (hasOverlapObstacles[i])
                {
                    int3 areaCellMin;
                    int3 areaCellMax;
                    MinMaxAABB localObstacleAABBExtended = localObstacleAABBsExtended[i];
                    CollisionExtension.GetAABBHashMapOverlapData(in conversionFactor, in localHashMapAABBs, 
                                                                 in localObstacleAABBExtended, out areaCellMin, out areaCellMax);

                    int3 areaCellCount = areaCellMax - areaCellMin + 1;
                    new GetBoidObstacleCollisionDataJob
                    {
                        cellCountX = hashGridBuilder.cellCountAxis.x,
                        cellCountXY = hashGridBuilder.cellCountXY,
                        areaCellCountX = areaCellCount.x,
                        areaCellCountXY = areaCellCount.x * areaCellCount.y,
                        areaStartCell = areaCellMin,
                        areaCellOffset = hashGridBuilder.cellCountAxis - areaCellCount,

                        hashPivots = pivots,
                        hashTable = hashTable,
                        boidData = boidData,

                        col,

                        obstacleData = boidObstacleData,
                    }.Schedule()
                    .Complete();
                }
            }
        }

        public void OnStopRunning(ref SystemState state) { }


        public void OnDestroy(ref SystemState state)
        {
            cellIndices.Dispose();
            hashTable.Dispose();
            boidData.Dispose();
            boidObstacleData.Dispose();
            randoms.Dispose();
            boidTargetIndices.Dispose();
            boidTargetPositions.Dispose();
            enemyTransforms.Dispose();
            swarmTargetPositions.Dispose();
        }
    }

    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    partial struct InitializeBoidsJob : IJobEntity
    {
        [ReadOnly] public float startSpeed;
        [ReadOnly] public float startAngularSpeed;
        [ReadOnly] public NativeArray<float3> positions;
        [ReadOnly] public NativeArray<quaternion> rotations;


        [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
        public void Execute([EntityIndexInQuery] int boidIndex, ref LocalTransform transform, ref CSpeed speed, ref CAngularSpeed angularSpeed)
        {
            speed.value = startSpeed;
            angularSpeed.value = startAngularSpeed;
            transform = LocalTransform.FromPositionRotationScale(positions[boidIndex], rotations[boidIndex], transform.Scale);
        }
    }

    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    partial struct InitializeEnemiesJob : IJobEntity
    {
        [ReadOnly] public float speed;
        [ReadOnly] public float angularSpeed;
        [NativeDisableContainerSafetyRestriction] public NativeArray<LocalTransform> enemyTransforms;

        [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
        public void Execute([EntityIndexInQuery] int enemyIndex, ref CSpeed speed, ref CAngularSpeed angularSpeed, in LocalTransform transform)
        {
            speed.value = this.speed;
            angularSpeed.value = this.angularSpeed;
            enemyTransforms[enemyIndex] = transform;
        }
    }

    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    partial struct InitializeSwarmTargetPositionsJob : IJobEntity
    {
        public NativeArray<float3> swarmTargetPositions;

        [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
        public void Execute(in CSwarmTarget swarmTarget, in LocalToWorld localToWorld)
        {
            swarmTargetPositions[swarmTarget.index] = localToWorld.Position;
        }
    }

    
}

