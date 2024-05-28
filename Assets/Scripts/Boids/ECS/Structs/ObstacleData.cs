using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public struct ObstacleData
{
    public bool isInRange;
    public float3 nearestPoint;
    public float3 avoidDirection;
}
