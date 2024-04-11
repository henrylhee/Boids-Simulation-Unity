using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class DataConversion
{
    public static int FloorFloatToInt(float value)
    {
        return (int)value;
    }

    public static int3 FloorFloatToInt(float3 value)
    {
        return new int3((int)value.x, (int)value.y, (int)value.z);
    }
}
