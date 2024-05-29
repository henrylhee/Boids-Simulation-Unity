using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Mathematics;

[BurstCompile(OptimizeFor = OptimizeFor.Performance)]
public static class MathExtensions
{
    // Code from Unity forum user "eizenhorn". https://forum.unity.com/threads/rotate-towards-c-jobs.836356/#post-5530615
    [MethodImpl(MethodImplOptions.AggressiveInlining), BurstCompile]
    public static void RotateTowards(
            in quaternion from,
            in quaternion to,
            out quaternion result,
            float maxRadiansDelta)
    {
        var dot = math.dot(from, to);
        float num = !(dot > 0.999998986721039) ? (float)(math.acos(math.min(math.abs(dot), 1f)) * 2.0) : 0.0f;

        if(num < float.Epsilon) { result = to; }
        else { result = math.slerp(from, to, math.min(1f, maxRadiansDelta / num)); }
    }
}
