using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Boids
{
    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    partial struct ApplyRulesJob : IJobEntity
    {
        //[ReadOnly] public float deltaTime;
        [ReadOnly] public BehaviourData behaviourData;
        [ReadOnly] public float speedTowardsObjective;
        [ReadOnly] public float maxDistanceCenter;
        [ReadOnly] public NativeArray<BoidData> sortedBoidData;

        [ReadOnly] public NativeArray<int3> pivots;
        [ReadOnly] public NativeArray<int> cellIndices;
        [ReadOnly] public float conversionFactor;
        [ReadOnly] public int3 cellCountAxis;
        [ReadOnly] public int cellCountXY;

        [ReadOnly] public float3 swarmCenter;
        [ReadOnly] public float3 swarmObjective;
        [ReadOnly] public float3 boundsMin;


        [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
        public void Execute([EntityIndexInQuery] int boidIndex, ref CRuleVector ruleVector, in LocalTransform transform)
        {
            float3 position = transform.Position;
            int cellIndex = cellIndices[boidIndex];
            float3 convertedPosition = (position - boundsMin) * conversionFactor;
            int3 cell = new int3((int)convertedPosition.x, (int)convertedPosition.y, (int)convertedPosition.z);
            int3 pivot = pivots[cellIndex];

            float3 separationVector = float3.zero;

            float3 alignmentVector = float3.zero;
            float3 cohesionVector = float3.zero;
            float3 addedPositions = float3.zero;
            int boidCount = 0;

            float3 globalCohesionVector = float3.zero;
            float3 objectiveVector = float3.zero;

            float visionRangeSq = behaviourData.visionRange * behaviourData.visionRange;

            // iterate over all boids in the cell of the currently processed boid
            for (int i = pivot.y; i < pivot.z; i++)
            {
                BoidData boidData = sortedBoidData[i];

                if (math.distancesq(boidData.position, transform.Position) < 0.000001)
                {
                    continue;
                }
                
                ProcessBoid(boidData);
            }

            // get the cells surrounding the cell of the currently processed boid and iterate over all boids inside
            for (int x = cell.x - 1; x <= cell.x + 1; x++)
            {
                if (x < 0 || x >= cellCountAxis.x) { continue; }

                for (int y = cell.y - 1; y <= cell.y + 1; y++)
                {
                    if (y < 0 || y >= cellCountAxis.y) { continue; }

                    for (int z = cell.z - 1; z <= cell.z + 1; z++)
                    {
                        if (z < 0 || z >= cellCountAxis.z) { continue; }
                        if (x == cell.x && y == cell.y && z == cell.z) { continue; }

                        int cellIndexToCheck = x + y * cellCountAxis.x + z * cellCountXY;
                        int3 pivotToCheck = pivots[cellIndexToCheck];
                        if (pivotToCheck.x == 0) { continue; }

                        for (int i = pivotToCheck.y; i < pivotToCheck.z; i++)
                        {
                            ProcessBoid(sortedBoidData[i]);
                        }
                    }
                }
            }

            if (boidCount > 0)
            {
                alignmentVector = (alignmentVector / boidCount) * behaviourData.alignmentStrength;
                float3 towardsMassCenter = cohesionVector / boidCount - position;
                float3 towardsMassCenterNormed = math.normalize(towardsMassCenter);
                float separateUrgency = 1 - math.lengthsq(towardsMassCenter) / visionRangeSq;

                cohesionVector = towardsMassCenterNormed * behaviourData.cohesionStrength;
                separationVector = -1 * towardsMassCenterNormed * separateUrgency * behaviourData.separationStrength;
            }

            globalCohesionVector = ((swarmCenter - position)/maxDistanceCenter) * behaviourData.globalCohesionStrength;
            objectiveVector = math.normalize(swarmObjective - position) * behaviourData.speedTowardsObjective * behaviourData.objectiveStrength;

            ruleVector.value = alignmentVector + separationVector + cohesionVector + globalCohesionVector + objectiveVector;

            void ProcessBoid(BoidData boidToCheck)
            {
                float3 distVector = boidToCheck.position - position;
                float distVectorLengthSq = math.lengthsq(distVector);

                if (distVectorLengthSq > visionRangeSq) { return; }
                alignmentVector += math.mul(boidToCheck.rotation, new float3(0f,0f,1f)) * boidToCheck.speed;
                cohesionVector += boidToCheck.position;
                boidCount++;
            } 
        }
    }
}
