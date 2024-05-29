using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

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
}

