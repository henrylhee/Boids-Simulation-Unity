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

    public struct RotationBuilder
    {
        private EntityQuery query;
        private ComponentTypeHandle<LocalTransform> transformHandle;


        public RotationBuilder(ref SystemState state, EntityQuery query)
        {
            this.query = query;

            this.transformHandle = state.GetComponentTypeHandle<LocalTransform>(true);
        }

        public JobHandle Gather(ref SystemState state, JobHandle dependency, ref NativeArray<CRotation> positions)
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
            public NativeArray<CRotation> Positions;

            [ReadOnly]
            public NativeArray<int> FirstEntityIndices;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                var ptr = (quaternion*)this.Positions.GetUnsafePtr();
                var dst = ptr + this.FirstEntityIndices[unfilteredChunkIndex];

                var size = UnsafeUtility.SizeOf<quaternion>();
                var positions = chunk.GetNativeArray(ref this.TransformHandle).Slice().SliceWithStride<quaternion>(0);

                UnsafeUtility.MemCpyStride(dst, size, positions.GetUnsafeReadOnlyPtr(), positions.Stride, size, positions.Length);
            }
        }
    }
}

