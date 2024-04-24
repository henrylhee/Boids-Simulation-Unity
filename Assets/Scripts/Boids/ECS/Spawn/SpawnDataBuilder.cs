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
            int halfSize = 0;

            while (index < boidCount)
            {
                halfSize = edgeCount / 2;

                int xMax = halfSize;
                for (int y = -halfSize; y <= halfSize; y++)
                {
                    for (int z = -halfSize; z <= halfSize; z++)
                    {
                        if (index < boidCount)
                        {
                            positions[index] = new CPosition { value = center + new float3(xMax * offset, y * offset, z * offset) };
                            positions[index] = new CPosition { value = center + new float3(-xMax * offset, y * offset, z * offset) };

                            index++;
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
                        if (index < boidCount)
                        {
                            positions[index] = new CPosition { value = center + new float3(x * offset, yMax * offset, z * offset) };
                            positions[index] = new CPosition { value = center + new float3(x * offset, -yMax * offset, z * offset) };

                            index++;
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
                        if (index < boidCount)
                        {
                            positions[index] = new CPosition { value = center + new float3(x * offset, y * offset, zMax * offset) };
                            positions[index] = new CPosition { value = center + new float3(x * offset, y * offset, -zMax * offset) };

                            index++;
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
            float3 randomDirection = new float3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            float3 eyeWorldDirection = new float3(0f,0f,1f);
            float3 upWorldDirection = new float3(0f, 1f, 0f);
            quaternion randomQuaternion = TransformHelpers.LookAtRotation(eyeWorldDirection, randomDirection, upWorldDirection);

            for(int i = 0; i < rotationsResult.Length; i++)
            {
                rotationsResult[i] = new CRotation { value = randomQuaternion };
            }
            return rotationsResult;
        }
    }
}
