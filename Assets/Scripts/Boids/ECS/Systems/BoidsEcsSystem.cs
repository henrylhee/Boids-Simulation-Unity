using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Boids
{
    public partial class BoidsEcsSystem : SystemBase
    {
        SpatialHashGridBuilder hashGridBuilder;
        SpawnerEcs spawner;

        CBoidsConfig config;

        NativeArray<float3> boidsPositions;
        NativeArray<Entity> boids;

        EntityQuery boidsQuery;


        protected override void OnCreate()
        {
            RequireForUpdate<CBoidsConfig>();
            config = SystemAPI.GetSingleton<CBoidsConfig>();

            Initialize();

            SpawnBoids(boidsPositions);
        }

        private void Initialize()
        {
            hashGridBuilder = new SpatialHashGridBuilder();
            spawner = new SpawnerEcs();
            boidsPositions = new NativeArray<float3>(spawner.GenerateBoidPositions(config.spawnData), Allocator.TempJob);

            boidsQuery = SystemAPI.QueryBuilder().WithAspect<BoidAspect>().Build();
        }

        protected override void OnUpdate()
        {

        }

        [BurstCompile]
        private void SpawnBoids(NativeArray<float3> boidsPositions)
        {
            for(int i = 0; i < boidsPositions.Length; i++)
            {
                EntityManager.Instantiate(config.boidPrefabEntity, boids);
            }

            boidsQuery.ToComponentDataArray<>

            for (int i = 0;i < boidsPositions.Length; i++)
            {
                //SystemAPI.SetComponent(boids, new LocalTransform
                //{

                //});
            }
        }

        private partial struct SpawnBoidsJob : IJobEntity
        {
            public void Execute()
            {

            }
        }
    }
}

