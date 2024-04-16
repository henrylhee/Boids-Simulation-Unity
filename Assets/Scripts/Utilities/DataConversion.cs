using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public class DataConversion
{
    [BurstCompile]
    public static int FloorToInt(float value)
    {
        return (int)value;
    }

    [BurstCompile]
    public static int3 FloorToInt(float3 value)
    {
        return new int3((int)value.x, (int)value.y, (int)value.z);
    }

    [BurstCompile]
    public static int CeilToInt(float value)
    {
        return (int)math.ceil(value);
    }

    [BurstCompile]
    public static int3 CeilToInt(float3 value)
    {
        return new int3((int)math.ceil(value).x, (int)math.ceil(value).y, (int)math.ceil(value).z);
    }
}
