using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

namespace Boids
{
    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    partial struct GetBoidObstacleCollisionDataJob : IJobParallelFor
    {
        [ReadOnly] public int3 cellCount;
        [ReadOnly] public int3 areaPivot;

        [ReadOnly] public NativeArray<int3> pivots;
        [ReadOnly] public NativeArray<int> boidIndices;

        public NativeArray<ObstacleData> obstacleData;


        [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
        public void Execute([EntityIndexInQuery] int cellIndex)
        {
            int3 areaCellPosition = ;
            int3 cellPosition = areaPivot + areaCellPosition;
        }
    }
}
