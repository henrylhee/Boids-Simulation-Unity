using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

namespace Boids
{
    public class SpawnerEcs
    {
        public float3[] GenerateBoidPositions(CSpawnData settings)
        {
            float3[] positions = new float3[settings.boidCount];

            switch (settings.shape)
            {
                case GenerationShape.SPHERE:
                    GenerateSpherePositions(ref positions, settings);
                    break;
                case GenerationShape.CUBE:
                    GenerateCubePositions(ref positions, settings);
                    break;
            }

            return positions;
        }

        private void GenerateSpherePositions(ref float3[] positions, CSpawnData settings)
        {
            float3 center = settings.center;
            positions[0] = center;

            for (int i = 1; i < positions.Length; i++)
            {
                float offset = settings.spawnDistance;
            }
        }

        private void GenerateCubePositions(ref float3[] positions, CSpawnData settings)
        {
            float3 center = settings.center;
            int edgeCount = 2;
            float offset = settings.spawnDistance;
            int index = 0;

            while (index < settings.boidCount)
            {
                int halfSize = edgeCount / 2;
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
                                return;
                            }
                        }
                    }
                }

                edgeCount += 2;
            }
        }
    }
}
