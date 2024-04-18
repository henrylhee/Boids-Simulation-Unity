using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.UIElements;

namespace Boids
{
    public partial struct BoidsEcsSystem : ISystem
    {
        SpatialHashGridBuilder hashGridBuilder;
        NativeArray<int> cellIndices;
        NativeArray<int> hashTable;

        SpawnDataBuilder spawnDataBuilder;

        CBoidsConfig config;

        RuleJobDataBuilder ruleJobDataBuilder;
        NativeArray<CPosition> positions;
        NativeArray<CRotation> rotations;
        NativeArray<CSpeed> speeds;

        NativeArray<Entity> boids;

        EntityQuery boidsQuery;

        JobHandle dataBuilderHandle;
        JobHandle initializeBoidsHandle;
        JobHandle applyRulesHandle;
        JobHandle moveBoidsHandle;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<CBoidsConfig>();
        }

        [BurstCompile]
        public void OnStartRunning(ref SystemState state)
        {
            config = SystemAPI.GetSingleton<CBoidsConfig>();

            Initialize(ref state);
            SpawnBoids(ref state);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            ruleJobDataBuilder.Gather(ref state, dataBuilderHandle, ref positions, ref rotations, ref speeds);

            NativeArray<int3> pivots; 
            SpatialHashGridBuilder.Build(in positions, config, out pivots, ref cellIndices, ref hashTable);

            applyRulesHandle = new ApplyRulesJob
            {
                behaviourData = config.behaviourData,

                positions = this.positions,
                velocities = this.velocities,

                boundsMin = hashGridBuilder.GetBoundsMin(),
                conversionFactor = hashGridBuilder.GetConversionFactor(),
                cellCountAxis = hashGridBuilder.GetCellCountAxis(),
                cellCountXY = hashGridBuilder.GetCellCountXY(),

                forwardVector = new float3(0f, 0f, 1f),

                pivots = this.pivots,
                hashTable = this.hashTable,
                cellIndices = this.cellIndices
            }.ScheduleParallel(boidsQuery, initializeBoidsHandle);

            moveBoidsHandle = new MoveBoidsJob
            {
                deltaTime = SystemAPI.Time.DeltaTime
            }.ScheduleParallel(boidsQuery, applyRulesHandle);

            moveBoidsHandle.Complete();

            pivots.Dispose();
        }

        private void Initialize(ref SystemState state)
        {
            boidsQuery = SystemAPI.QueryBuilder().WithAspect<BoidAspect>().Build();

            boids = new NativeArray<Entity>(new Entity[config.spawnData.boidCount], Allocator.Persistent);
            cellIndices = new NativeArray<int>(config.spawnData.boidCount, Allocator.Persistent);
            hashTable = new NativeArray<int>(config.spawnData.boidCount, Allocator.Persistent);

            SpawnDataBuilder.GenerateCubeSpawnData(config.spawnData, out positions, out rotations, Allocator.Persistent);

            ruleJobDataBuilder = new RuleJobDataBuilder(ref state, boidsQuery);
        }

        private void SpawnBoids(ref SystemState state)
        {
            state.EntityManager.Instantiate(config.boidPrefabEntity, boids);

            initializeBoidsHandle = new InitializeBoids()
            {
                startSpeed = config.movementData.startSpeed,
                positions = this.positions,
                rotations = this.rotations,
            }.ScheduleParallel(boidsQuery, new JobHandle());
        }
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

