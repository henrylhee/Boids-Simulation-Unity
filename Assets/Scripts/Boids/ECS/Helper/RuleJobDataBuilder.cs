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
                                JobHandle dependency, ref NativeArray<CPosition> positions, ref NativeArray<CRotation> rotations, ref NativeArray<CSpeed> speeds)
        {
            
            NativeArray<int> firstEntityIndices = this.query.CalculateBaseEntityIndexArrayAsync(state.WorldUpdateAllocator, dependency, out var dependency1);
            dependency = JobHandle.CombineDependencies(dependency, dependency1);

            dependency = new GatherPositionsJob
            {
                TransformHandle = transformHandle,
                SpeedHandle = speedHandle,

                Positions = positions,
                Rotations = rotations,
                Speeds = speeds,

                FirstEntityIndices = firstEntityIndices,
            }.ScheduleParallel(query, dependency);

            return dependency;
        }

        [BurstCompile]
        private unsafe struct GatherPositionsJob : IJobChunk
        {
            [ReadOnly] public ComponentTypeHandle<LocalTransform> TransformHandle;
            [ReadOnly] public ComponentTypeHandle<CSpeed> SpeedHandle;

            [NativeDisableContainerSafetyRestriction] public NativeArray<CPosition> Positions;
            [NativeDisableContainerSafetyRestriction] public NativeArray<CRotation> Rotations;
            [NativeDisableContainerSafetyRestriction] public NativeArray<CSpeed> Speeds;

            [ReadOnly] public NativeArray<int> FirstEntityIndices;

            [BurstCompile]
            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask, in v128 chunkEnabledMask)
            {
                var positionPtr = (float3*)this.Positions.GetUnsafePtr();
                var rotationPtr = (quaternion*)this.Rotations.GetUnsafePtr();
                var speedPtr = (float*)this.Speeds.GetUnsafePtr();

                var positionDst = positionPtr + this.FirstEntityIndices[unfilteredChunkIndex];
                var rotationDst = rotationPtr + this.FirstEntityIndices[unfilteredChunkIndex];
                var speedDst = speedPtr + this.FirstEntityIndices[unfilteredChunkIndex];

                var positionSize = UnsafeUtility.SizeOf<float3>();
                var rotationSize = UnsafeUtility.SizeOf<quaternion>();
                var speedSize = UnsafeUtility.SizeOf<float>();

                var positions = chunk.GetNativeArray(ref this.TransformHandle).Slice().SliceWithStride<float3>(0);
                var rotations = chunk.GetNativeArray(ref this.TransformHandle).Slice().SliceWithStride<quaternion>(12);
                var speeds = chunk.GetNativeArray(ref this.SpeedHandle).Slice().SliceWithStride<float>(0);

                UnsafeUtility.MemCpyStride(positionDst, positionSize, positions.GetUnsafeReadOnlyPtr(), positions.Stride, positionSize, positions.Length);
                UnsafeUtility.MemCpyStride(rotationDst, rotationSize, rotations.GetUnsafeReadOnlyPtr(), rotations.Stride, rotationSize, rotations.Length);
                UnsafeUtility.MemCpyStride(speedDst, speedSize, speeds.GetUnsafeReadOnlyPtr(), speeds.Stride, speedSize, speeds.Length);
            }
        }
    }
}

