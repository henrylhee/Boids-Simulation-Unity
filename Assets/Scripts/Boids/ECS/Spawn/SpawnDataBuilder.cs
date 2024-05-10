using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;
using Random = UnityEngine.Random;

namespace Boids
{
    [BurstCompile]
    public struct SpawnDataBuilder
    {
        [BurstCompile]
        public void GenerateCubeSpawnData(in SpawnData settings, ref NativeArray<float3> positions, ref NativeArray<quaternion> rotations)
        {
            GenerateCubePositions(settings.boidCount, settings.center, settings.spawnDistance, ref positions);
            GenerateCubeRotations(settings.boidCount, ref rotations);
        }

        [BurstCompile]
        public void GenerateCubePositions(int boidCount, in float3 center, float spawnDistance, ref NativeArray<float3> positions)
        {
            int edgeCount = 2;
            float offset = spawnDistance;
            int index = 0;
            int halfSize;
            float randomness = 0.005f;
            float3 randomVector;

            while (index < boidCount)
            {
                halfSize = edgeCount / 2;

                int xMax = halfSize;
                for (int y = -halfSize; y <= halfSize; y++)
                {
                    for (int z = -halfSize; z <= halfSize; z++)
                    {
                        if (index < boidCount-1)
                        {
                            randomVector = new float3(Random.Range(-randomness, randomness), Random.Range(-randomness, randomness), Random.Range(-randomness, randomness));
                            positions[index] = center + randomVector + new float3(xMax * offset, y * offset, z * offset);
                            randomVector = new float3(Random.Range(-randomness, randomness), Random.Range(-randomness, randomness), Random.Range(-randomness, randomness));
                            positions[index + 1] = center + randomVector + new float3(-xMax * offset, y * offset, z * offset);
                            index += 2;
                        }
                        else if (index < boidCount)
                        {
                            randomVector = new float3(Random.Range(-randomness, randomness), Random.Range(-randomness, randomness), Random.Range(-randomness, randomness));
                            positions[index] = center + randomVector + new float3(xMax * offset, y * offset, z * offset);
                            index++;
                            return;
                        }
                        else
                        {
                            return;
                        }
                    }
                }

                int yMax = halfSize;
                for (int x = -halfSize; x <= halfSize; x++)
                {
                    for (int z = -halfSize; z <= halfSize; z++)
                    {
                        if (index < boidCount-1)
                        {
                            randomVector = new float3(Random.Range(-randomness, randomness), Random.Range(-randomness, randomness), Random.Range(-randomness, randomness));

                            positions[index] = center + randomVector + new float3(x * offset, yMax * offset, z * offset);
                            randomVector = new float3(Random.Range(-randomness, randomness), Random.Range(-randomness, randomness), Random.Range(-randomness, randomness));

                            positions[index+1] = center + randomVector + new float3(x * offset, -yMax * offset, z * offset);
                            index += 2;
                        }
                        else if(index < boidCount)
                        {
                            randomVector = new float3(Random.Range(-randomness, randomness), Random.Range(-randomness, randomness), Random.Range(-randomness, randomness));

                            positions[index] = center + randomVector + new float3(x * offset, yMax * offset, z * offset);
                            index++;
                            return;
                        }
                        else
                        {
                            return;
                        }
                    }
                }

                int zMax = halfSize;
                for (int x = -halfSize; x <= halfSize; x++)
                {
                    for (int y = -halfSize; y <= halfSize; y++)
                    {
                        if (index < boidCount - 1)
                        {
                            randomVector = new float3(Random.Range(-randomness, randomness), Random.Range(-randomness, randomness), Random.Range(-randomness, randomness));

                            positions[index] = center + randomVector + new float3(x * offset, y * offset, zMax * offset);
                            randomVector = new float3(Random.Range(-randomness, randomness), Random.Range(-randomness, randomness), Random.Range(-randomness, randomness));

                            positions[index+1] = center + randomVector + new float3(x * offset, y * offset, -zMax * offset);
                            index += 2;
                        }
                        else if (index < boidCount)
                        {
                            randomVector = new float3(Random.Range(-randomness, randomness), Random.Range(-randomness, randomness), Random.Range(-randomness, randomness));

                            positions[index] = center + randomVector + new float3(x * offset, y * offset, zMax * offset);
                            index++;
                            return;
                        }
                        else
                        { 
                            return;
                        }
                    }
                }

                edgeCount += 2;
            }
            return;
        }

        [BurstCompile]
        public void GenerateCubeRotations(int boidCount, ref NativeArray<quaternion> rotations)
        {
            quaternion randomRotation = Random.rotation;

            for (int i = 0; i < boidCount; i++)
            {
                rotations[i] = randomRotation;
            }
        }
    }
}
