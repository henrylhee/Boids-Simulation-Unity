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
        SpawnerEcs spawner;

        CBoidsConfig config;


        NativeArray<LocalToWorld> localToWorld;
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
            hashGridBuilder = new SpatialHashGridBuilder();

            boids = new NativeArray<Entity>(new Entity[config.spawnData.boidCount], Allocator.Persistent);

            spawner = new SpawnerEcs();
            spawner.Generate(config.spawnData);
            positions = spawner.GetPositions(Allocator.TempJob);
            directions = spawner.GetDirections(Allocator.TempJob);
        }

        protected override void OnUpdate()
        {
        }

        [BurstCompile]
        private void SpawnBoids()
        {
            boidsQuery = SystemAPI.QueryBuilder().WithAspect<BoidAspect>().Build();

            EntityManager.Instantiate(config.boidPrefabEntity, boids);

            boidsQuery.CopyFromComponentDataArray<CPosition>(positions.Reinterpret<CPosition>());
            boidsQuery.CopyFromComponentDataArray<CRotation>(directions.Reinterpret<CRotation>());

            new SetBoidsData()
            {
                startSpeed = config.movementData.Speed,
                scale = new float3(1f,1f,1f),
            }.ScheduleParallel(boidsQuery);
        }
    }

    [BurstCompile]
    public partial struct SetBoidsData : IJobEntity
    {
        public float startSpeed;
        public float3 scale;


        [BurstCompile]
        public void Execute(ref CPosition position, ref CRotation rotation, ref CSpeed speed, ref LocalTransform transform)
        {

            speed.value = startSpeed;
            localToWorld.Value = float4x4.TRS(position.value, direction.value, scale);
        }
    }
}

