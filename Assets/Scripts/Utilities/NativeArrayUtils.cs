using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using Unity.Mathematics;

[BurstCompile(OptimizeFor = OptimizeFor.Performance)]
public unsafe static class NativeArrayUtils
{
    //[BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    //public static void Add(ref NativeArray<int3> array, int index, int3 addValue)
    //{
    //    int3 value = UnsafeUtility.ReadArrayElement<int3>(array.GetUnsafePtr<int3>(), index);
    //    value += addValue;
    //    UnsafeUtility.WriteArrayElement(array.GetUnsafePtr<int3>(), index, value);
    //}

    //[BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    //public static int3 GetElement(ref NativeArray<int3> array, int index)
    //{
    //    return UnsafeUtility.ReadArrayElement<int3>(array.GetUnsafePtr<int3>(), index);
    //}
}

