namespace Boids
{
    using Unity.Burst;
    using Unity.Burst.Intrinsics;
    using Unity.Collections;
    using Unity.Collections.LowLevel.Unsafe;
    using Unity.Entities;
    using Unity.Jobs;
    using Unity.Mathematics;
    using Unity.Transforms;

    public struct PositionBuilder
    {
        private EntityQuery query;
        private ComponentTypeHandle<LocalTransform> transformHandle;


        public PositionBuilder(ref SystemState state, EntityQuery query)
        {
            this.query = query;

            this.transformHandle = state.GetComponentTypeHandle<LocalTransform>(true);
        }

        public JobHandle Gather(ref SystemState state, JobHandle dependency, ref NativeArray<CPosition> positions)
        {
            this.transformHandle.Update(ref state);

            var firstEntityIndices = this.query.CalculateBaseEntityIndexArrayAsync(state.WorldUpdateAllocator, dependency, out var dependency1);
            dependency = JobHandle.CombineDependencies(dependency, dependency1);

            dependency = new GatherPositionsJob
            {
                TransformHandle = this.transformHandle,
                Positions = positions,
                FirstEntityIndices = firstEntityIndices,
            }
                .ScheduleParallel(this.query, dependency);

            return dependency;
        }

        [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
        private unsafe struct GatherPositionsJob : IJobChunk
        {
            [ReadOnly]
            public ComponentTypeHandle<LocalTransform> TransformHandle;

            [NativeDisableParallelForRestriction]
            public NativeArray<CPosition> Positions;

            [ReadOnly]
            public NativeArray<int> FirstEntityIndices;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                var ptr = (float3*)this.Positions.GetUnsafePtr();
                var dst = ptr + this.FirstEntityIndices[unfilteredChunkIndex];

                var size = UnsafeUtility.SizeOf<float3>();
                var positions = chunk.GetNativeArray(ref this.TransformHandle).Slice().SliceWithStride<float3>(0);

                UnsafeUtility.MemCpyStride(dst, size, positions.GetUnsafeReadOnlyPtr(), positions.Stride, size, positions.Length);
            }
        }
    }
}

