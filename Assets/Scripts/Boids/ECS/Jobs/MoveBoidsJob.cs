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
        [ReadOnly] public float deltaTime;


        [BurstCompile]
        public void Execute(in CTargetRotation targetRotation, in CTargetSpeed targetSpeed, ref LocalTransform transform, ref CSpeed speed)
        {
            transform.Rotation = targetRotation.value;
            speed.value = targetSpeed.value;

            transform.Position = transform.Position + math.mul(targetRotation.value, new float3(0f,0f,1f) * targetSpeed.value);
        }
    }
}
