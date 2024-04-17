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
        NativeArray<int3> pivots;
        NativeArray<int> cellIndices;
        NativeArray<int> hashTable;

        SpawnerEcs spawner;

        CBoidsConfig config;

        NativeArray<CPosition> positions;
        NativeArray<CVelocity> velocities;
        NativeArray<Entity> boids;

        EntityQuery boidsQuery;
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

            Initialize();
            SpawnBoids();
        }

        public void OnUpdate(ref SystemState state)
        {
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

            positions = boidsQuery.ToComponentDataArray<CPosition>(Allocator.Persistent);
            velocities = boidsQuery.ToComponentDataArray<CVelocity>(Allocator.Persistent);

            hashGridBuilder.Build(positions);
            hashGridBuilder.GetPivots(ref pivots);
            hashGridBuilder.GetHashTable(ref hashTable);
            hashGridBuilder.GetCellIndices(ref cellIndices);
        }

        [BurstCompile]
        private void Initialize()
        {
            boids = new NativeArray<Entity>(new Entity[config.spawnData.boidCount], Allocator.Persistent);

            spawner = new SpawnerEcs();
            spawner.Generate(config.spawnData);

            spawner.GetPositions(ref positions, Allocator.Persistent);
            spawner.GetVelocities(ref velocities, Allocator.Persistent);

            boidsQuery = SystemAPI.QueryBuilder().WithAspect<BoidAspect>().Build();

            hashGridBuilder = new SpatialHashGridBuilder();
            hashGridBuilder.Inititalize(config.behaviourData.CohesionDistance, config.spawnData.boidCount);
            hashGridBuilder.Build(in positions);
        }

        private void SpawnBoids()
        {
            EntityManager.Instantiate(config.boidPrefabEntity, boids);

            boidsQuery.CopyFromComponentDataArray<CPosition>(positions.Reinterpret<CPosition>());
            boidsQuery.CopyFromComponentDataArray<CVelocity>(velocities.Reinterpret<CVelocity>());

            initializeBoidsHandle = new InitializeBoids()
            {
                startSpeed = config.movementData.startSpeed,
            }.ScheduleParallel(boidsQuery, new JobHandle());
        }
    }

    

    [BurstCompile]
    partial struct InitializeBoids : IJobEntity
    {
        public float startSpeed;


        [BurstCompile]
        public void Execute(ref LocalTransform transform, in CPosition position, in CVelocity velocity)
        {
            transform.Position = position.value;
            transform.Rotation = TransformHelpers.LookAtRotation(new float3(0f, 0f, 1f), 
                                                                 velocity.value,
                                                                 new float3(0f, 1f, 0f));
        }
    }
}

