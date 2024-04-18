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
        public static void GenerateCubeSpawnData(CSpawnData settings, out NativeArray<CPosition> positions, out NativeArray<CRotation> rotations, Allocator allocator)
        {
            float3[] tempPositions = GenerateCubePositions(settings);
            quaternion[] tempRotations = GenerateCubeRotations(settings);

            positions = new NativeArray<CPosition>(tempPositions.Length, allocator);
            rotations = new NativeArray<CRotation>(tempRotations.Length, allocator);
        }

        [BurstCompile]
        private static float3[] GenerateCubePositions(CSpawnData settings)
        {
            float3[] positions = new float3[settings.boidCount];

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
        private static quaternion[] GenerateCubeRotations(CSpawnData settings)
        {
            quaternion[] rotationsResult = new quaternion[settings.boidCount];
            float3 randomDirection = new float3(Random.Range(-1f, 1f), Random.Range(-1f, 1f), Random.Range(-1f, 1f);
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
