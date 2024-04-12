using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Boids
{
    public partial class BoidsEcsSystem : SystemBase
    {
        SpatialHashGridBuilder hashGridBuilder;
        SpawnerEcs spawner;

        CBoidsConfig config;

        NativeArray<float3> positions;
        NativeArray<quaternion> directions;
        NativeArray<Entity> boids;

        EntityQuery boidsQuery;


        protected override void OnCreate()
        {
            RequireForUpdate<CBoidsConfig>();
            config = SystemAPI.GetSingleton<CBoidsConfig>();

            Initialize();
            SpawnBoids();
        }

        private void Initialize()
        {
            hashGridBuilder = new SpatialHashGridBuilder();

            spawner = new SpawnerEcs();
            spawner.Generate(config.spawnData);
            positions = spawner.GetPositions(Allocator.TempJob);
            directions = spawner.GetDirections(Allocator.TempJob);

            boidsQuery = SystemAPI.QueryBuilder().WithAspect<BoidAspect>().Build();
        }

        protected override void OnUpdate()
        {

        }

        [BurstCompile]
        private void SpawnBoids()
        {
            for(int i = 0; i < positions.Length; i++)
            {
                EntityManager.Instantiate(config.boidPrefabEntity, boids);
            }

            boidsQuery.CopyFromComponentDataArray<CPosition>(positions.Reinterpret<CPosition>());

            NativeArray<LocalToWorld> localToWorld = boidsQuery.ToComponentDataArray<LocalToWorld>(Allocator.TempJob);
            EntityManager.SetComponentData(boidsQuery, );

            boidsQuery.CopyFromComponentDataArray<LocalToWorld>(localToWorld);
        }

        private partial struct SpawnBoidsJob : IJobEntity
        {
            public void Execute()
            {

            }
        }
    }

    //Useentitiesforeach https://forum.unity.com/threads/entity-query-vs-entities-for-each.851968/
    public struct SetBoidsData : IJobEntityBatch
    {
        public ComponentTypeHandle<CPosition> velocityTypeHandle;
        public ComponentTypeHandle<CDirection> translationTypeHandle;
        public ComponentTypeHandle<LocalToWorld> localToWorldTypeHandle;
        public float DeltaTime;

        [BurstCompile]
        public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
        {
            NativeArray<VelocityVector> velocityVectors =
                batchInChunk.GetNativeArray(velocityTypeHandle);
            NativeArray<Translation> translations =
                batchInChunk.GetNativeArray(translationTypeHandle);

            for (int i = 0; i < batchInChunk.Count; i++)
            {
                float3 translation = translations[i].Value;
                float3 velocity = velocityVectors[i].Value;
                float3 newTranslation = translation + velocity * DeltaTime;

                translations[i] = new Translation() { Value = newTranslation };
            }
        }
    }
}

