using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEditor.Search;
using UnityEngine;

namespace Boids
{
    [BurstCompile]
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

        EntityQuery boidsQuery;

        JobHandle rulesDataBuilderHandle;
        JobHandle initializeBoidsHandle;
        JobHandle applyRulesHandle;
        JobHandle moveBoidsHandle;


        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CBoidsConfig>();
            boidsQuery = SystemAPI.QueryBuilder().WithAspect<BoidAspect>().Build();
        }

        [BurstCompile]
        public void OnStartRunning(ref SystemState state)
        {
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
            rulesDataBuilderHandle = ruleJobDataBuilder.Gather(ref state, initializeBoidsHandle, ref positions, ref rotations, ref speeds);
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

                forwardVector = new float3(0f, 0f, 1f),

                pivots = pivots,
                hashTable = this.hashTable,
                cellIndices = this.cellIndices,

                deltaTime = SystemAPI.Time.DeltaTime,
                maxSpeed = movementData.maxSpeed
            }.ScheduleParallel(boidsQuery, rulesDataBuilderHandle);

            moveBoidsHandle = new MoveBoidsJob {}
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
        }

        private void Initialize(ref SystemState state)
        {
            cellIndices = new NativeArray<int>(spawnData.boidCount, Allocator.Persistent);
            hashTable = new NativeArray<int>(spawnData.boidCount, Allocator.Persistent);
            positions = new NativeArray<CPosition>(spawnData.boidCount, Allocator.Persistent);
            rotations = new NativeArray<CRotation>(spawnData.boidCount, Allocator.Persistent);
            speeds = new NativeArray<CSpeed>(spawnData.boidCount, Allocator.Persistent);

            SpawnData data = spawnData;
            spawnDataBuilder = new SpawnDataBuilder();
            spawnDataBuilder.GenerateCubeSpawnData(in data, out positions, out rotations, Allocator.Persistent);

            hashGridBuilder = new SpatialHashGridBuilder();

            ruleJobDataBuilder = new RuleJobDataBuilder(ref state, boidsQuery);
        }

        private void SpawnBoids(ref SystemState state)
        {
            state.EntityManager.Instantiate(boidPrefab, spawnData.boidCount, Allocator.Persistent);

            initializeBoidsHandle = new InitializeBoids()
            {
                startSpeed = movementData.startSpeed,
                positions = this.positions,
                rotations = this.rotations,
            }.ScheduleParallel(boidsQuery, new JobHandle());

            initializeBoidsHandle.Complete();
        }

        public void OnStopRunning(ref SystemState state) { }
    }

    [BurstCompile]
    partial struct InitializeBoids : IJobEntity
    {
        [ReadOnly] public float startSpeed;
        [ReadOnly] public NativeArray<CPosition> positions;
        [ReadOnly] public NativeArray<CRotation> rotations;

        [BurstCompile]
        public void Execute([EntityIndexInQuery] int boidIndex, ref LocalTransform transform, ref CSpeed speed)
        {
            speed.value = startSpeed;
            transform.Position = positions[boidIndex].value;
            transform.Rotation = rotations[boidIndex].value;
        }
    }
}

