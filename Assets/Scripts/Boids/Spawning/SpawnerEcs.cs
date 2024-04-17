using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Boids
{
    public class SpawnerEcs
    {
        private float3[] positions;
        private float3[] velocities;

        public float3 boundsMin { get; private set; }
        public float3 boundsMax { get; private set; }


        public void Generate(CSpawnData settings)
        {
            positions = new float3[settings.boidCount];
            velocities = new float3[settings.boidCount];

            switch (settings.shape)
            {
                case GenerationShape.SPHERE:
                    //GenerateSpherePositions(settings);
                    break;
                case GenerationShape.CUBE:
                    GenerateCubeSpawn(settings);
                    break;
            }
        }

        public void GetPositions(ref NativeArray<CPosition> positions, Allocator allocator)
        {
            new NativeArray<float3>(this.positions, allocator).Reinterpret<CPosition>();
        }

        public void GetVelocities(ref NativeArray<CVelocity> velocities, Allocator allocator)
        {
            new NativeArray<float3>(this.velocities, allocator).Reinterpret<CVelocity>();
        }

        private void GenerateSpherePositions(CSpawnData settings)
        {
            //float3 center = settings.center;
            //positions[0] = center;

            //for (int i = 1; i < positions.Length; i++)
            //{
            //    float offset = settings.spawnDistance;
            //}

            throw new NotImplementedException();
        }

        private void GenerateCubeSpawn(CSpawnData settings)
        {
            GenerateCubePositions(settings);
            GenerateCubeVelocities();
        }

        private void GenerateCubePositions(CSpawnData settings)
        {
            float3 center = settings.center;
            int edgeCount = 2;
            float offset = settings.spawnDistance;
            int index = 0;
            int halfSize = 0;


            while (index < settings.boidCount)
            {
                halfSize = edgeCount / 2;
                float3 start = center - new float3(offset, offset, offset) * halfSize;

                for (int x = 0; x <= edgeCount; x++)
                {
                    for (int y = 0; y <= edgeCount; y++)
                    {
                        for (int z = 0; z <= edgeCount; z++)
                        {
                            if (index < settings.boidCount)
                            {
                                positions[index] = start + new float3(x * offset, y * offset, z * offset);

                                index++;
                            }
                            else
                            {
                                boundsMin = center - new float3(offset, offset, offset) * halfSize;
                                boundsMax = center + new float3(offset, offset, offset) * halfSize;
                                Debug.Log("boundsMin: " + boundsMin);
                                Debug.Log("boundsMax: " + boundsMax);
                                return;
                            }
                        }
                    }
                }

                edgeCount += 2;
            }
        }

        private void GenerateCubeVelocities()
        {
            float3 randomDirection = new float3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));

            for(int i = 0; i < velocities.Length; i++)
            {
                velocities[i] = randomDirection;
            }
        }
    }
}
