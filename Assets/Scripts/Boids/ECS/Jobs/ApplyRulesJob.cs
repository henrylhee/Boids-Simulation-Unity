using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Boids
{
    [BurstCompile(DisableSafetyChecks = true)]
    partial struct ApplyRulesJob : IJobEntity
    {
        //[ReadOnly] public float deltaTime;
        [ReadOnly] public CBehaviourData behaviourData;
        [ReadOnly] public NativeArray<CPosition> positions;
        [ReadOnly] public NativeArray<CRotation> rotations;
        [ReadOnly] public NativeArray<CSpeed> speeds;

        [ReadOnly] public NativeArray<int3> pivots;
        [ReadOnly] public NativeArray<int> hashTable;
        [ReadOnly] public NativeArray<int> cellIndices; // put into boidAspect only need current entities value
        [ReadOnly] public float conversionFactor;
        [ReadOnly] public int3 cellCountAxis;
        [ReadOnly] public int cellCountXY;
        
        [ReadOnly] public float3 boundsMin;

        [ReadOnly] public float3 forwardVector;

        [BurstCompile(DisableSafetyChecks = true)]
        public void Execute([EntityIndexInQuery] int boidIndex, in LocalTransform transform, ref CAimedVelocity aimedVelocity)
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

                if(math.distance(position, positionToCheck) > behaviourData.CohesionDistance) { continue; }
                cohesionVector += distVector;
                allignmentVector += rotations[boidIndexToCheck].value * (forwardVector * speeds[]);
                allignCohesCounter++;

                if (math.distance(position, positionToCheck) > behaviourData.RepulsionDistance) { continue; }
                repulsionVector += distVector;
                repulsionCounter++;
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
                        if (x == 0 && y == 0 && z == 0) { continue; }

                        int cellIndexToCheck = x + y * cellCountAxis.x + z * cellCountXY;
                        int3 pivotToCheck = pivots[cellIndexToCheck];
                        if(pivotToCheck.x == 0) { continue; }

                        for (int i = pivot.y; i < pivot.z; i++)
                        {
                            int boidIndexToCheck = hashTable[i];
                            float3 positionToCheck = positions[boidIndexToCheck].value;
                            float3 distVector = position - positionToCheck;

                            if (math.distance(position, positionToCheck) > behaviourData.CohesionDistance) { continue; }
                            cohesionVector += distVector;
                            allignmentVector += velocities[boidIndexToCheck].value;
                            allignCohesCounter++;

                            if (math.distance(position, positionToCheck) > behaviourData.RepulsionDistance) { continue; }
                            repulsionVector += distVector;
                            repulsionCounter++;
                        }
                    }
                }
            }

            cohesionVector = (cohesionVector / allignCohesCounter) * behaviourData.CohesionStrength;
            allignmentVector = (allignmentVector / allignCohesCounter) * behaviourData.AllignmentStrength;
            repulsionVector = repulsionVector / repulsionCounter;

            aimedVelocity.value = cohesionVector + allignmentVector + repulsionVector;
        }
    }
}
