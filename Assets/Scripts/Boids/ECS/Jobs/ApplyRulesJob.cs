using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Entities.UniversalDelegates;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI;

namespace Boids
{
    [BurstCompile]
    partial struct ApplyRulesJob : IJobEntity
    {
        //[ReadOnly] public float deltaTime;
        [ReadOnly] public CMovementData movementData;
        [ReadOnly] public CBehaviourData behaviourData;
        [ReadOnly] public NativeArray<CPosition> positions;
        [ReadOnly] public NativeArray<CRotation> rotations;

        [ReadOnly] public NativeArray<int3> pivots;
        [ReadOnly] public NativeArray<int> hashTable;
        [ReadOnly] public NativeArray<int> cellIndices;
        [ReadOnly] public float conversionFactor;
        [ReadOnly] public int3 cellCountAxis;
        [ReadOnly] public int cellCountXY;
        
        [ReadOnly] public float3 boundsMin;
        [ReadOnly] public float3 boundsMax;
        [ReadOnly] public float3 convertedBoundsMin;
        [ReadOnly] public float3 convertedBoundsMax;

        [ReadOnly] public float3 forwardVector;

        [BurstCompile]
        public void Execute(BoidAspect boidAspect, [EntityIndexInQuery] int boidIndex)
        {
            int cellIndex = cellIndices[boidIndex];
            float3 position = positions[boidIndex].value;
            float3 convertedPosition = (position - boundsMin) * conversionFactor;
            int3 cell = new int3((int)convertedPosition.x, (int)convertedPosition.y, (int)convertedPosition.z);

            int3 pivot = pivots[cellIndex];

            float3 repulsionVector = float3.zero;
            int repulsionCounter = 0;

            float3 allignmentVector = float3.zero;
            int allignCohesCounter = 0;

            float3 cohesionVector = float3.zero;

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
                allignmentVector += rotations[boidIndexToCheck].value * forwardVector;
                allignCohesCounter++;
                
            }
        }

        [BurstCompile]
        private void ProcessBoid(in int boidIndex, in int3 pivot, in float3 position)
        {

        }
    }
}
