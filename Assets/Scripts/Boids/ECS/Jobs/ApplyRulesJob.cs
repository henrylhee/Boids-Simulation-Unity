using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Boids
{
    [BurstCompile]
    partial struct ApplyRulesJob : IJobEntity
    {
        //[ReadOnly] public float deltaTime;
        [ReadOnly] public BehaviourData behaviourData;
        [ReadOnly] public NativeArray<CPosition> positions;
        [ReadOnly] public NativeArray<CRotation> rotations;
        [ReadOnly] public NativeArray<CSpeed> speeds;

        [ReadOnly] public NativeArray<int3> pivots;
        [ReadOnly] public NativeArray<int> hashTable;
        [ReadOnly] public NativeArray<int> cellIndices;
        [ReadOnly] public float conversionFactor;
        [ReadOnly] public int3 cellCountAxis;
        [ReadOnly] public int cellCountXY;
        
        [ReadOnly] public float3 boundsMin;


        [BurstCompile]
        public void Execute([EntityIndexInQuery] int boidIndex, in LocalTransform transform, ref CTargetVector targetVector)
        {
            float3 position = transform.Position;
            int cellIndex = cellIndices[boidIndex];
            float3 convertedPosition = (position - boundsMin) * conversionFactor;
            int3 cell = new int3((int)convertedPosition.x, (int)convertedPosition.y, (int)convertedPosition.z);
            int3 pivot = pivots[cellIndex];

            float3 repulsionVector = float3.zero;
            int repulsionCounter = 0;

            float3 allignmentVector = float3.zero;
            int allignCohesCounter = 0;

            float3 cohesionVector = float3.zero;

            float cohesionDistance = behaviourData.CohesionDistance;
            float repulsionDistance = behaviourData.RepulsionDistance;


            // iterate over all boids in the cell of the currently processed boid
            for (int i = pivot.y; i < pivot.z; i++)
            {
                int boidIndexToCheck = hashTable[i];
                float3 positionToCheck = positions[boidIndexToCheck].value;
                float3 distVector = position - positionToCheck;

                if (distVector.x == 0 && distVector.y == 0 && distVector.z == 0)
                {
                    continue;
                }

                ProcessBoid(positionToCheck, distVector, rotations[boidIndexToCheck].value, speeds[boidIndexToCheck].value);
            }

            // get the cells surrounding the cell of the currently processed boid and iterate over all boids inside
            for(int x = -1 + cell.x; x <= 1 + cell.x;  x++)
            {
                if(x < 0 || x >= cellCountAxis.x) { continue; }

                for(int y = -1 + cell.y; y <= 1 + cell.y; y++)
                {
                    if (y < 0 || y >= cellCountAxis.y) { continue; }

                    for (int z = -1 + cell.z; z <= 1 + cell.z; z++)
                    {
                        if (z < 0 || z >= cellCountAxis.z) { continue; }
                        if (x == cell.x && y == cell.y && z == cell.z) { continue; }

                        int cellIndexToCheck = x + y * cellCountAxis.x + z * cellCountXY;
                        int3 pivotToCheck = pivots[cellIndexToCheck];
                        if(pivotToCheck.x == 0) { continue; }

                        for (int i = pivot.y; i < pivot.z; i++)
                        {
                            int boidIndexToCheck = hashTable[i];
                            float3 positionToCheck = positions[boidIndexToCheck].value;
                            float3 distVector = position - positionToCheck;

                            ProcessBoid(positionToCheck, distVector, rotations[boidIndexToCheck].value, speeds[boidIndexToCheck].value);
                        }
                    }
                }
            }

            cohesionVector = (cohesionVector / allignCohesCounter) * behaviourData.CohesionStrength;
            allignmentVector = (allignmentVector / allignCohesCounter) * behaviourData.AllignmentStrength;
            repulsionVector = -1 * (repulsionVector / repulsionCounter) * behaviourData.RepulsionStrength;

            targetVector.value = cohesionVector + allignmentVector + repulsionVector;


            void ProcessBoid(float3 positionToCheck, float3 distVector, quaternion rotation, float speed)
            {
                float distVectorLength = math.length(distVector);

                if (distVectorLength > cohesionDistance) { return; }
                cohesionVector += distVector;
                allignmentVector += math.mul(rotation, new float3(0f,0f,1f) * speed);
                allignCohesCounter++;

                if (distVectorLength > repulsionDistance) { return; }
                repulsionVector += (repulsionDistance - distVectorLength) * math.normalize(distVector);
                repulsionCounter++;
            } 
        }
    }
}
