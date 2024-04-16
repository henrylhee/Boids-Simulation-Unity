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
        private quaternion[] directions;

        public float3 boundsMin { get; private set; }
        public float3 boundsMax { get; private set; }


        public void Generate(CSpawnData settings)
        {
            positions = new float3[settings.boidCount];
            directions = new quaternion[settings.boidCount];

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

        public NativeArray<float3> GetPositions(Allocator allocator)
        {
            return new NativeArray<float3>(positions, allocator);
        }

        public NativeArray<quaternion> GetDirections(Allocator allocator)
        {
            return new NativeArray<quaternion>(directions, allocator);
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
            GenerateCubeDirections();
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
                                UnityEngine.Debug.Log("boundsMin: " + boundsMin);
                                UnityEngine.Debug.Log("boundsMax: " + boundsMax);
                                return;
                            }
                        }
                    }
                }

                edgeCount += 2;
            }
        }

        private void GenerateCubeDirections()
        {
            Quaternion randomDirection = Quaternion.Euler(Random.Range(0, 360f), Random.Range(0, 360f), Random.Range(0, 360f));

            for(int i = 0; i < directions.Length; i++)
            {
                directions[i] = randomDirection;
            }
        }
    }
}
