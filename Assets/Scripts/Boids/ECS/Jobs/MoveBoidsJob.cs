using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Boids
{
    [BurstCompile]
    partial struct MoveBoidsJob : IJobEntity
    {


        [BurstCompile]
        public void Execute(in CTargetRotation targetRotation, in CTargetSpeed targetSpeed, ref LocalTransform transform, ref CSpeed speed)
        {
            speed.value = targetSpeed.value;
            float3 newPos = transform.Position + math.mul(targetRotation.value, new float3(0f, 0f, 1f) * targetSpeed.value);
            transform = LocalTransform.FromPositionRotationScale(newPos, targetRotation.value, 0.01f);
        }
    }
}
