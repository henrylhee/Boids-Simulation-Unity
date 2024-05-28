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
    public static void GetAABBHashMapOverlapArea(in float conversionFactor, in MinMaxAABB hashMapAABB, in MinMaxAABB obstacleAABB, out MinMaxAABB resultAABB)
    {
        resultAABB = new MinMaxAABB();

        if(obstacleAABB.Min.x < hashMapAABB.Min.x)
        {
            resultAABB.Min.x = hashMapAABB.Min.x;
        }
        else
        {
            resultAABB.Min.x = ((int)(obstacleAABB.Min.x * conversionFactor)) / conversionFactor;
        }

        if (obstacleAABB.Min.y < hashMapAABB.Min.y)
        {
            resultAABB.Min.y = hashMapAABB.Min.y;
        }
        else
        {
            resultAABB.Min.y = ((int)(obstacleAABB.Min.y * conversionFactor)) / conversionFactor;
        }

        if (obstacleAABB.Min.z < hashMapAABB.Min.z)
        {
            resultAABB.Min.z = hashMapAABB.Min.z;
        }
        else
        {
            resultAABB.Min.z = ((int)(obstacleAABB.Min.z * conversionFactor)) / conversionFactor;
        }


        if (obstacleAABB.Max.x > hashMapAABB.Max.x)
        {
            resultAABB.Max.x = hashMapAABB.Max.x;
        }
        else
        {
            resultAABB.Max.x = ((int)(obstacleAABB.Max.x * conversionFactor) + 1) / conversionFactor;
        }

        if (obstacleAABB.Max.y < hashMapAABB.Max.y)
        {
            resultAABB.Max.y = hashMapAABB.Max.y;
        }
        else
        {
            resultAABB.Max.y = ((int)(obstacleAABB.Max.y * conversionFactor) + 1) / conversionFactor;
        }

        if (obstacleAABB.Max.z < hashMapAABB.Max.z)
        {
            resultAABB.Max.z = hashMapAABB.Max.z;
        }
        else
        {
            resultAABB.Max.z = ((int)(obstacleAABB.Max.z * conversionFactor) + 1) / conversionFactor;
        }
    }
}
