using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Boids
{
    [BurstCompile]
    partial struct ApplyRulesJob : IJobEntity
    {
        //[ReadOnly] public float deltaTime;
        [ReadOnly] public BehaviourData behaviourData;
        [ReadOnly] public float speedTowardsObjective;
        [ReadOnly] public float maxDistanceCenter;
        [ReadOnly] public NativeArray<RuleData> ruleData;

        [ReadOnly] public NativeArray<int3> pivots;
        [ReadOnly] public NativeArray<int> hashTable;
        [ReadOnly] public NativeArray<int> cellIndices;
        [ReadOnly] public float conversionFactor;
        [ReadOnly] public int3 cellCountAxis;
        [ReadOnly] public int cellCountXY;

        [ReadOnly] public float3 swarmCenter;
        [ReadOnly] public float3 swarmObjective;
        [ReadOnly] public float3 boundsMin;


        [BurstCompile]
        public void Execute([EntityIndexInQuery] int boidIndex, ref CRuleVector ruleVector)
        {
            float3 position = ruleData[boidIndex].position;
            int cellIndex = cellIndices[boidIndex];
            float3 convertedPosition = (position - boundsMin) * conversionFactor;
            int3 cell = new int3((int)convertedPosition.x, (int)convertedPosition.y, (int)convertedPosition.z);
            int3 pivot = pivots[cellIndex];

            float3 repulsionVector = float3.zero;
            int repulsionCounter = 0;

            float3 allignmentVector = float3.zero;
            int allignCounter = 0;

            float3 cohesionVector = float3.zero;
            float3 objectiveVector = float3.zero;

            float visionRange = behaviourData.visionRange;
            float repulsionDistance = behaviourData.repulsionDistance;

            // iterate over all boids in the cell of the currently processed boid
            for (int i = pivot.y; i < pivot.z; i++)
            {
                int boidIndexToCheck = hashTable[i];

                if (boidIndexToCheck == boidIndex)
                {
                    continue;
                }
                
                ProcessBoid(ruleData[boidIndexToCheck]);
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
                            ProcessBoid(ruleData[boidIndexToCheck]);
                        }
                    }
                }
            }

            if(allignCounter > 0)
            {
                allignmentVector = (allignmentVector / allignCounter) * behaviourData.allignmentStrength;
            }
            if (repulsionCounter > 0)
            {
                repulsionVector = -1 * repulsionVector * behaviourData.repulsionStrength;
            }

            cohesionVector = ((swarmCenter - position)/maxDistanceCenter) * behaviourData.cohesionStrength * behaviourData.maxSpeedCohesion;
            objectiveVector = math.normalizesafe(swarmObjective - position) * speedTowardsObjective;
            ruleVector.value = allignmentVector + repulsionVector + cohesionVector + objectiveVector;

            if (boidIndex == 500)
            {
                UnityEngine.Debug.Log("- Boid 500 - : cohesionVector: " + math.length(cohesionVector) + ". allignmentVector: " + math.length(allignmentVector) + ". repulsionVector: " + math.length(repulsionVector) + ". objectiveVector: " + math.length(objectiveVector));
            }
            if (boidIndex == 999)
            {
                UnityEngine.Debug.Log("- Boid 999 - : cohesionVector: " + math.length(cohesionVector) + ". allignmentVector: " + math.length(allignmentVector) + ". repulsionVector: " + math.length(repulsionVector) + ". objectiveVector: " + math.length(objectiveVector));
            }
            if (boidIndex == 1)
            {
                UnityEngine.Debug.Log("- Boid 1 - : cohesionVector: " + math.length(cohesionVector) + ". allignmentVector: " + math.length(allignmentVector) + ". repulsionVector: " + math.length(repulsionVector) + ". objectiveVector: " + math.length(objectiveVector));
            }

            void ProcessBoid(RuleData boidToCheck)
            {
                float3 distVector = boidToCheck.position - position;
                float distVectorLength = math.length(distVector);

                if (distVectorLength > visionRange) { return; }
                allignmentVector += math.mul(boidToCheck.rotation, new float3(0f,0f,1f)) * boidToCheck.speed;
                allignCounter++;

                if (distVectorLength > repulsionDistance) { return; }
                repulsionVector += (repulsionDistance - distVectorLength) * math.normalizesafe(distVector);
                repulsionCounter++;
            } 
        }
    }
}
