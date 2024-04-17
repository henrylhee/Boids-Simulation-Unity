
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine.Analytics;

[BurstCompile]
public class DataConversion
{
    [BurstCompile]
    public static void FloorToInt(in float input, ref int result)
    {
        result = (int)input;
    }

    [BurstCompile]
    public static void FloorToInt3(in float3 input, ref int3 result)
    {
        result = new int3((int)input.x, (int)input.y, (int)input.z);
    }

    [BurstCompile]
    public static void CeilToInt(in float input, ref int result)
    {
        result = (int)math.ceil(input);
    }

    [BurstCompile]
    public static void CeilToInt3(in float3 input, ref int3 result)
    {
        result = new int3((int)math.ceil(input).x, (int)math.ceil(input).y, (int)math.ceil(input).z);
    }
}
