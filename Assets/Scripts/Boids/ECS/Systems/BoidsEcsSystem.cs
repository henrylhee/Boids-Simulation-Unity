using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
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

        NativeArray<float3> positions;
        NativeArray<quaternion> directions;
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
            positions = spawner.GetPositions(Allocator.TempJob);
            directions = spawner.GetDirections(Allocator.TempJob);
            boundsMin = spawner.boundsMin;
            boundsMax = spawner.boundsMax;

            boidsQuery = SystemAPI.QueryBuilder().WithAspect<BoidAspect>().Build();

            hashGridBuilder = new SpatialHashGridBuilder();
            hashGridBuilder.Inititalize(config.behaviourData.CohesionDistance, boundsMin, boundsMax);
            hashGridBuilder.Build(positions);
        }

        protected override void OnUpdate()
        {
        }

        [BurstCompile]
        private void SpawnBoids()
        {
            EntityManager.Instantiate(config.boidPrefabEntity, boids);

            boidsQuery.CopyFromComponentDataArray<CPosition>(positions.Reinterpret<CPosition>());
            boidsQuery.CopyFromComponentDataArray<CRotation>(directions.Reinterpret<CRotation>());

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

