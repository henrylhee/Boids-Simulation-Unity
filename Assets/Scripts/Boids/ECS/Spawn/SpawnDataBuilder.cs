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
        public void GenerateCubeSpawnData(in SpawnData settings, out NativeArray<CPosition> positions, out NativeArray<CRotation> rotations, Allocator allocator)
        {
            int boidCount = settings.boidCount;
            float3 center = settings.center;
            float spawnDistance = settings.spawnDistance;

            positions = GenerateCubePositions(boidCount, center, spawnDistance, allocator);
            rotations = GenerateCubeRotations(boidCount, allocator);
        }

        [BurstCompile]
        public NativeArray<CPosition> GenerateCubePositions(int boidCount, in float3 center, float spawnDistance, Allocator allocator)
        {
            NativeArray<CPosition> positions = new NativeArray<CPosition>(boidCount, allocator);
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
                            positions[index] = new CPosition { value = center + randomVector + new float3(xMax * offset, y * offset, z * offset) };
                            randomVector = new float3(Random.Range(-randomness, randomness), Random.Range(-randomness, randomness), Random.Range(-randomness, randomness));
                            positions[index+1] = new CPosition { value = center + randomVector + new float3(-xMax * offset, y * offset, z * offset) };
                            index += 2;
                        }
                        else if (index < boidCount)
                        {
                            randomVector = new float3(Random.Range(-randomness, randomness), Random.Range(-randomness, randomness), Random.Range(-randomness, randomness));
                            positions[index] = new CPosition { value = center + randomVector + new float3(xMax * offset, y * offset, z * offset) };
                            index++;
                            return positions;
                        }
                        else
                        {
                            return positions;
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

                            positions[index] = new CPosition { value = center + randomVector + new float3(x * offset, yMax * offset, z * offset) };
                            randomVector = new float3(Random.Range(-randomness, randomness), Random.Range(-randomness, randomness), Random.Range(-randomness, randomness));

                            positions[index+1] = new CPosition { value = center + randomVector + new float3(x * offset, -yMax * offset, z * offset) };
                            index += 2;
                        }
                        else if(index < boidCount)
                        {
                            randomVector = new float3(Random.Range(-randomness, randomness), Random.Range(-randomness, randomness), Random.Range(-randomness, randomness));

                            positions[index] = new CPosition { value = center + randomVector + new float3(x * offset, yMax * offset, z * offset) };
                            index++;
                            return positions;
                        }
                        else
                        {
                            return positions;
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

                            positions[index] = new CPosition { value = center + randomVector + new float3(x * offset, y * offset, zMax * offset) };
                            randomVector = new float3(Random.Range(-randomness, randomness), Random.Range(-randomness, randomness), Random.Range(-randomness, randomness));

                            positions[index+1] = new CPosition { value = center + randomVector + new float3(x * offset, y * offset, -zMax * offset) };
                            index += 2;
                        }
                        else if (index < boidCount)
                        {
                            randomVector = new float3(Random.Range(-randomness, randomness), Random.Range(-randomness, randomness), Random.Range(-randomness, randomness));

                            positions[index] = new CPosition { value = center + randomVector + new float3(x * offset, y * offset, zMax * offset) };
                            index++;
                            return positions;
                        }
                        else
                        { 
                            return positions;
                        }
                    }
                }

                edgeCount += 2;
            }
            return positions;
        }

        [BurstCompile]
        public NativeArray<CRotation> GenerateCubeRotations(int boidCount, Allocator allocator)
        {
            NativeArray<CRotation> rotationsResult = new NativeArray<CRotation>(boidCount, allocator);
            quaternion randomQuaternion = Random.rotation;

            for(int i = 0; i < rotationsResult.Length; i++)
            {
                rotationsResult[i] = new CRotation { value = randomQuaternion };
            }
            return rotationsResult;
        }
    }
}
