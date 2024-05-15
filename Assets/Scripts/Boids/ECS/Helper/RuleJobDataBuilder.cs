using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace Boids
{
    [BurstCompile]
    partial struct GatherRuleDataJob : IJobEntity
    {
        [NativeDisableContainerSafetyRestriction] public NativeArray<RuleData> RuleDataArray;

        [BurstCompile]
        public void Execute([EntityIndexInQuery] int boidIndex, in LocalTransform transform, in CSpeed speed)
        {
            RuleDataArray[boidIndex] = new RuleData
            {
                position = transform.Position,
                rotation = transform.Rotation,
                speed = speed.value
            };
        }
    }
}

