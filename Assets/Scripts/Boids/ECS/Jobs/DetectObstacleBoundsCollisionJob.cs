using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Rendering;
using UnityEngine;

namespace Boids
{
    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    partial struct FindObstacleHashMapOverlapsJob : IJobEntity
    {
        [ReadOnly] public MinMaxAABB hashMapBoundsExtended;
        public NativeArray<bool> HasOverlapObstacles;
        public NativeArray<MinMaxAABB> obstacleAABBs;

        [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
        public void Execute([EntityIndexInQuery] int index, in RenderBounds bounds)
        {
            MinMaxAABB obstacleBounds = new MinMaxAABB
            {
                Min = bounds.Value.Min,
                Max = bounds.Value.Max,
            };
            HasOverlapObstacles[index] = CollisionExtension.CheckAABBOverlap(in hashMapBoundsExtended, in obstacleBounds);
            obstacleAABBs[index] = obstacleBounds;
        }
    }
}

