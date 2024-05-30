using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
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
        [ReadOnly] public MinMaxAABB localHashMapAABBs;
        [ReadOnly] public float3 hashMapMin;
        [ReadOnly] public float obstacleInteractionRadius;
        [NativeDisableContainerSafetyRestriction] public NativeArray<bool> hasOverlapObstacles;
        [NativeDisableContainerSafetyRestriction] public NativeArray<MinMaxAABB> localObstacleAABBsExtended;
        [NativeDisableContainerSafetyRestriction] public NativeArray<PhysicsCollider> obstacleColliders;

        [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
        public void Execute([EntityIndexInQuery] int index, in RenderBounds bounds, in PhysicsCollider col)
        {
            MinMaxAABB localObstacleAABBExtended = new MinMaxAABB
            {
                Min = bounds.Value.Min - hashMapMin - obstacleInteractionRadius,
                Max = bounds.Value.Max - hashMapMin + obstacleInteractionRadius,
            };
            hasOverlapObstacles[index] = CollisionExtension.CheckAABBOverlap(in localHashMapAABBs, in localObstacleAABBExtended);
            localObstacleAABBsExtended[index] = localObstacleAABBExtended;
            obstacleColliders[index] = col;
        }
    }
}

