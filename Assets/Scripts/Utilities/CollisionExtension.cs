using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile(OptimizeFor = OptimizeFor.Performance)]
public static class CollisionExtension
{
    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    public static bool CheckAABBOverlap(in MinMaxAABB aabb1, in MinMaxAABB aabb2)
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

    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    public static void GetAABBHashMapOverlapData(in float conversionFactor, in MinMaxAABB hashMapAABB, 
                                                 in MinMaxAABB obstacleAABB, out int3 cellMin, out int3 cellMax)
    {
        cellMin = new int3(0, 0, 0);
        cellMax = new int3((int)(hashMapAABB.Max.x * conversionFactor),
                                (int)(hashMapAABB.Max.y * conversionFactor),
                                (int)(hashMapAABB.Max.z * conversionFactor));

        if (obstacleAABB.Min.x > hashMapAABB.Min.x)
        {
            cellMin.x = (int)(obstacleAABB.Min.x * conversionFactor);
        }
        if (obstacleAABB.Min.y > hashMapAABB.Min.y)
        {
            cellMin.y = (int)(obstacleAABB.Min.y * conversionFactor);
        }
        if (obstacleAABB.Min.z > hashMapAABB.Min.z)
        {
            cellMin.z = (int)(obstacleAABB.Min.z * conversionFactor);
        }

        if (obstacleAABB.Max.x < hashMapAABB.Max.x)
        {
            cellMax.x = (int)(obstacleAABB.Max.x * conversionFactor);
        }
        if (obstacleAABB.Max.y < hashMapAABB.Max.y)
        {
            cellMax.y = (int)(obstacleAABB.Max.y * conversionFactor);
        }
        if (obstacleAABB.Max.z < hashMapAABB.Max.z)
        {
            cellMax.z = (int)(obstacleAABB.Max.z * conversionFactor);
        }
    }
}
