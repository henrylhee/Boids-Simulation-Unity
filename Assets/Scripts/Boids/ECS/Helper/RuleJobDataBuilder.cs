namespace Boids
{
    using System.Diagnostics;
    using Unity.Burst;
    using Unity.Burst.Intrinsics;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using Unity.Entities;
    using Unity.Jobs;
    using Unity.Mathematics;
    using Unity.Transforms;

    [BurstCompile]
    public struct RuleJobDataBuilder
    {
        private EntityQuery query;


        public RuleJobDataBuilder(EntityQuery query)
        {
            this.query = query;
        }

        public JobHandle Gather(ref SystemState state, ComponentTypeHandle<LocalTransform> transformHandle, ComponentTypeHandle<CSpeed> speedHandle, 
                                ref NativeArray<RuleData> ruleData, JobHandle dependency)
        {
            NativeArray<int> firstEntityIndices = this.query.CalculateBaseEntityIndexArrayAsync(state.WorldUpdateAllocator, dependency, out var dependency1);
            dependency = JobHandle.CombineDependencies(dependency, dependency1);

            dependency = new GatherPositionsJob
            {
                TransformHandle = transformHandle,
                SpeedHandle = speedHandle,

                RuleDataArray = ruleData,

                FirstEntityIndices = firstEntityIndices
            }.ScheduleParallel(query, dependency);

            return dependency;
        }

        [BurstCompile]
        private unsafe struct GatherPositionsJob : IJobChunk
        {
            [ReadOnly] public ComponentTypeHandle<LocalTransform> TransformHandle;
            [ReadOnly] public ComponentTypeHandle<CSpeed> SpeedHandle;

            [NativeDisableContainerSafetyRestriction] public NativeArray<RuleData> RuleDataArray;

            [ReadOnly] public NativeArray<int> FirstEntityIndices;

            [BurstCompile]
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                var ruleDataPtr = (RuleData*)this.RuleDataArray.GetUnsafePtr();

                var transformSize = UnsafeUtility.SizeOf<LocalTransform>();
                var posRotSize = UnsafeUtility.SizeOf<PosRot>();
                var speedSize = UnsafeUtility.SizeOf<float>();
                var ruleDataSize = UnsafeUtility.SizeOf<RuleData>();

                var ruleDataDst = ruleDataPtr + this.FirstEntityIndices[unfilteredChunkIndex];
                var speedOffset = posRotSize;

                var posRots = chunk.GetNativeArray(ref this.TransformHandle).Slice().SliceWithStride<PosRot>(0);
                var speeds = chunk.GetNativeArray(ref this.SpeedHandle).Slice();

                UnsafeUtility.MemCpyStride(ruleDataDst, ruleDataSize, posRots.GetUnsafeReadOnlyPtr(), posRots.Stride, posRotSize, speeds.Length);
                UnsafeUtility.MemCpyStride(ruleDataDst + speedOffset, ruleDataSize, speeds.GetUnsafeReadOnlyPtr(), speeds.Stride, speedSize, speeds.Length);
            }
        }

        struct PosRot
        {
            public float3 position;
            public quaternion rotation;
        }
    }
}

