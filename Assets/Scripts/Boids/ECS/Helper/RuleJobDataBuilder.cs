using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Unity.Jobs;

namespace Boids
{
    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    partial struct GatherBoidDataJob : IJobEntity
    {
        [NativeDisableContainerSafetyRestriction] public NativeArray<BoidData> boidData;

        [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
        public void Execute([EntityIndexInQuery] int boidIndex, in LocalTransform transform, in CSpeed speed)
        {
            boidData[boidIndex] = new BoidData
            {
                position = transform.Position,
                rotation = transform.Rotation,
                speed = speed.value
            };
        }
    }

    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    partial struct SortBoidDataJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<int> hashTable;
        [ReadOnly] public NativeArray<BoidData> boidData;
        public NativeArray<BoidData> sortedBoidData;

        [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
        public void Execute(int index)
        {
            sortedBoidData[index] = boidData[hashTable[index]];
        }
    }
}

