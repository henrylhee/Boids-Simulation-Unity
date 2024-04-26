using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;

[BurstCompile]
public static class MathExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining), BurstCompile]
    public static ref quaternion RotateTowards(
            quaternion from,
            quaternion to,
            float maxRadiansDelta)
    {
        var dot = math.dot(from, to);
        float num = !(dot > 0.999998986721039) ? (float)(math.acos(math.min(math.abs(dot), 1f)) * 2.0) : 0.0f;

        quaternion result = to;
        if(num < float.Epsilon) { return ref result; }
        else { return ref math.slerp(from, to, math.min(1f, maxRadiansDelta / num)); }
        return num < float.Epsilon ? to : math.slerp(from, to, math.min(1f, maxRadiansDelta / num));
    }
}
