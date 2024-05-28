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
    partial struct DetectObstacleBoundsCollisionJob : IJobEntity
    {
        [ReadOnly] public MinMaxAABB hashMapBounds;
        public NativeArray<bool> collidesWithHashMap;

        [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
        public void Execute([EntityIndexInQuery] int index, in RenderBounds bounds)
        {
            MinMaxAABB minMaxBounds = new MinMaxAABB
            {
                Min = new float3(),
                Max = new float3(),
            };
            collidesWithHashMap[index] = CollisionExtension.CheckAABBCollision(hashMapBounds, );
        }
    }
}

