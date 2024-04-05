using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Boids
{
    public class SpawnShapeGenerator
    {
        private BoidsSpawnConfig settings;


        public Vector3[] GenerateBoidPositions(BoidsSpawnConfig settings)
        {
            this.settings = settings;

            Vector3[] positions = new Vector3[settings.boidCount];
            Debug.Log(settings.shape);

            switch (settings.shape)
            {
                case GenerationShape.SPHERE:
                    GenerateSpherePositions(ref positions);
                    break;
                case GenerationShape.CUBE:
                    Debug.Log("düu");
                    GenerateCubePositions(ref positions);
                    break;
            }

            return positions;
        }

        private void GenerateSpherePositions(ref Vector3[] positions)
        {
            Vector3 center = settings.center;
            positions[0] = center;

            for (int i = 1; i < positions.Length; i++)
            {
                float offset = settings.spawnDistance;
            }
        }

        private void GenerateCubePositions(ref Vector3[] positions)
        {
            Vector3 center = settings.center;
            int edgeCount = 2;
            float offset = settings.spawnDistance;
            int index = 0;

            while(index < settings.boidCount)
            {
                Debug.Log("Generate cubeshell; startIndex: " + index);
                int halfSize = edgeCount / 2;
                Vector3 start = center - new Vector3(offset, offset, offset) * halfSize;
                Debug.Log("settings.boidCount: " + settings.boidCount);
                Debug.Log("halfSize: " + halfSize);
                Debug.Log("start: " + start);


                for (int x = 0; x <= edgeCount; x++)
                {
                    for(int y = 0; y <= edgeCount; y++)
                    {
                        for(int z = 0; z <= edgeCount; z++)
                        {
                            if (index < settings.boidCount)
                            {
                                positions[index] = start + new Vector3(x * offset, y * offset, z * offset);
                                Debug.Log(positions[index]);

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

