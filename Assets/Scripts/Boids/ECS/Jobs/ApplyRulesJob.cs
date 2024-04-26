using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.UIElements;

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
                float3 distVector = positionToCheck - position;

                if (distVector.x == 0 && distVector.y == 0 && distVector.z == 0)
                {
                    continue;
                }
                
                ProcessBoid(distVector, rotations[boidIndexToCheck].value, speeds[boidIndexToCheck].value);
            }

            // get the cells surrounding the cell of the currently processed boid and iterate over all boids inside
            for(int x = cell.x - 1; x <= cell.x + 1;  x++)
            {
                if(x < 0 || x >= cellCountAxis.x) { continue; }

                for(int y = cell.y - 1; y <= cell.y + 1; y++)
                {
                    if (y < 0 || y >= cellCountAxis.y) { continue; }

                    for (int z = cell.z - 1; z <= cell.z + 1; z++)
                    {
                        if (z < 0 || z >= cellCountAxis.z) { continue; }
                        if (x == cell.x && y == cell.y && z == cell.z) { continue; }

                        int cellIndexToCheck = x + y * cellCountAxis.x + z * cellCountXY;
                        int3 pivotToCheck = pivots[cellIndexToCheck];
                        if(pivotToCheck.x == 0) { continue; }

                        for (int i = pivotToCheck.y; i < pivotToCheck.z; i++)
                        {

                            int boidIndexToCheck = hashTable[i];
                            float3 positionToCheck = positions[boidIndexToCheck].value;
                            float3 distVector = positionToCheck - position;

                            ProcessBoid(distVector, rotations[boidIndexToCheck].value, speeds[boidIndexToCheck].value);
                        }
                    }
                }
            }
            
            if (repulsionCounter > 0)
            {
                cohesionVector = (cohesionVector / allignCohesCounter) * behaviourData.CohesionStrength;
                allignmentVector = (allignmentVector / allignCohesCounter) * behaviourData.AllignmentStrength;
                repulsionVector = -1 * repulsionVector * behaviourData.RepulsionStrength;

                targetVector.value = cohesionVector + allignmentVector + repulsionVector;
            }
            else if(allignCohesCounter > 0)
            {
                cohesionVector = (cohesionVector / allignCohesCounter) * behaviourData.CohesionStrength;
                allignmentVector = (allignmentVector / allignCohesCounter) * behaviourData.AllignmentStrength;

                targetVector.value = cohesionVector + allignmentVector;
            }
            else
            {
                targetVector.value = new float3(0f,0f,0f);
            }

            //if (boidIndex == 50)
            //{
            //    UnityEngine.Debug.Log("----> 50: ");

            //    UnityEngine.Debug.Log("allignCohesCounter: " + allignCohesCounter);
            //    UnityEngine.Debug.Log("cohesionVector: " + math.length(cohesionVector));
            //    UnityEngine.Debug.Log("allignmentVector: " + math.length(allignmentVector));
            //    UnityEngine.Debug.Log("repulsionVector: " + math.length(repulsionVector));
            //}
            //if (boidIndex == 200)
            //{
            //    UnityEngine.Debug.Log("----> 200: ");

            //    UnityEngine.Debug.Log("cohesionVector: " + math.length(cohesionVector));
            //    UnityEngine.Debug.Log("allignmentVector: " + math.length(allignmentVector));
            //    UnityEngine.Debug.Log("repulsionVector: " + math.length(repulsionVector));
            //    UnityEngine.Debug.Log("targetVector.value: " + math.length(targetVector.value));
            //}

            void ProcessBoid(float3 distVector, quaternion rotation, float speed)
            {
                float distVectorLength = math.length(distVector);

                if (distVectorLength > cohesionDistance) { return; }
                cohesionVector += distVector;
                allignmentVector += math.mul(rotation, new float3(0f,0f,1f)) * speed;
                allignCohesCounter++;

                if (distVectorLength > repulsionDistance) { return; }
                repulsionVector += (repulsionDistance - distVectorLength) * math.normalize(distVector);
                repulsionCounter++;
            } 
        }
    }
}
