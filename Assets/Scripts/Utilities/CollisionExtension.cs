using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile(OptimizeFor = OptimizeFor.Performance)]
public static class CollisionExtension
{
    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    public static bool CheckAABBCollision(MinMaxAABB aabb1, MinMaxAABB aabb2)
    {
        if(aabb1.Min.x > aabb2.Max.x || aabb1.Max.x < aabb2.Min.x)
        {
            return false;
        }
        else if(aabb1.Min.y > aabb2.Max.y || aabb1.Max.y < aabb2.Min.y)
        {
            return false;
        }
        else if(aabb1.Min.z > aabb2.Max.z || aabb1.Max.z < aabb2.Min.z)
        {
            return false;
        }
        else 
        { 
            return true;
        }
    }
}
