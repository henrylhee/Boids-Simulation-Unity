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

            float3[] tempPositions = GenerateCubePositions(boidCount, center, spawnDistance);
            quaternion[] tempRotations = GenerateCubeRotations(boidCount);

            positions = new NativeArray<CPosition>(tempPositions.Length, allocator);
            rotations = new NativeArray<CRotation>(tempRotations.Length, allocator);
        }

        [BurstCompile]
        public float3[] GenerateCubePositions(int boidCount, in float3 center, float spawnDistance)
        {

            float3[] positions = new float3[boidCount];
            int edgeCount = 2;
            float offset = spawnDistance;
            int index = 0;
            int halfSize = 0;

            while (index < boidCount)
            {
                halfSize = edgeCount / 2;
                float3 start = center - new float3(offset, offset, offset) * halfSize;

                for (int x = 0; x <= edgeCount; x++)
                {
                    for (int y = 0; y <= edgeCount; y++)
                    {
                        for (int z = 0; z <= edgeCount; z++)
                        {
                            if (index < boidCount)
                            {
                                positions[index] = start + new float3(x * offset, y * offset, z * offset);

                                index++;
                            }
                            else
                            {
                                return positions;
                            }
                        }
                    }
                }

                edgeCount += 2;
            }
            return positions;
        }

        [BurstCompile]
        public quaternion[] GenerateCubeRotations(int boidCount)
        {
            quaternion[] rotationsResult = new quaternion[boidCount];
            float3 randomDirection = new float3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f));
            float3 eyeWorldDirection = new float3(0f,0f,1f);
            float3 upWorldDirection = new float3(0f, 1f, 0f);
            quaternion randomQuaternion = TransformHelpers.LookAtRotation(eyeWorldDirection, randomDirection, upWorldDirection);

            for(int i = 0; i < rotationsResult.Length; i++)
            {
                rotationsResult[i] = randomQuaternion;
            }
            return rotationsResult;
        }
    }
}
