using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Boids
{
    [BurstCompile]
    public partial class BoidsEcsSystem : SystemBase
    {
        SpatialHashGridBuilder hashGridBuilder;
        float3 boundsMin;
        float3 boundsMax;

        SpawnerEcs spawner;

        CBoidsConfig config;

        NativeArray<CPosition> positions;
        NativeArray<CVelocity> velocities;
        NativeArray<Entity> boids;

        EntityQuery boidsQuery;


        protected override void OnCreate()
        {
            RequireForUpdate<CBoidsConfig>();
        }

        protected override void OnStartRunning()
        {
            config = SystemAPI.GetSingleton<CBoidsConfig>();

            Initialize();
            SpawnBoids();
        }

        private void Initialize()
        {
            boids = new NativeArray<Entity>(new Entity[config.spawnData.boidCount], Allocator.Persistent);

            spawner = new SpawnerEcs();
            spawner.Generate(config.spawnData);
            positions = spawner.GetPositions(Allocator.Persistent).Reinterpret<CPosition>();
            velocities = spawner.GetVelocities(Allocator.Persistent).Reinterpret<CVelocity>();
            boundsMin = spawner.boundsMin;
            boundsMax = spawner.boundsMax;

            boidsQuery = SystemAPI.QueryBuilder().WithAspect<BoidAspect>().Build();

            hashGridBuilder = new SpatialHashGridBuilder();
            hashGridBuilder.Inititalize(config.behaviourData.CohesionDistance, boundsMin, boundsMax);
            hashGridBuilder.Build(spawner.GetPositions(Allocator.Persistent));
        }

        protected override void OnUpdate()
        {
            hashGridBuilder.Build(spawner.GetPositions(Allocator.Persistent));

            JobHandle applyRulesJob = new ApplyRulesJob
            {
                behaviourData = config.behaviourData,

                positions = this.positions,
                velocities = this.velocities,

                boundsMin = this.boundsMin,
                conversionFactor = hashGridBuilder.GetConversionFactor(),
                cellCountAxis = hashGridBuilder.GetCellCountAxis(),
                cellCountXY = hashGridBuilder.GetCellCountXY(),

                forwardVector = new float3(0f,0f,1f),

                pivots = hashGridBuilder.GetPivots(),
                hashTable = hashGridBuilder.GetHashTable(),
                cellIndices = hashGridBuilder.GetCellIndices()
            }.ScheduleParallel(boidsQuery, new JobHandle());

            new MoveBoidsJob
            {
                deltaTime = SystemAPI.Time.DeltaTime
            }.ScheduleParallel(boidsQuery, applyRulesJob);


        }

        [BurstCompile]
        private void SpawnBoids()
        {
            EntityManager.Instantiate(config.boidPrefabEntity, boids);

            boidsQuery.CopyFromComponentDataArray<CPosition>(positions.Reinterpret<CPosition>());
            boidsQuery.CopyFromComponentDataArray<CVelocity>(velocities.Reinterpret<CVelocity>());

            new InitializeBoids()
            {
                startSpeed = config.movementData.startSpeed,
            }.ScheduleParallel(boidsQuery);
        }
    }

    

    [BurstCompile]
    partial struct InitializeBoids : IJobEntity
    {
        public float startSpeed;


        [BurstCompile]
        public void Execute(BoidAspect boidAspect)
        {
            boidAspect.Speed = startSpeed;
            boidAspect.Initialize();
        }
    }
}

