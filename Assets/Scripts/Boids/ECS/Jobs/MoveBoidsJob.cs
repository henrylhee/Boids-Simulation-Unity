using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Boids
{
    [BurstCompile(DisableSafetyChecks = true)]
    partial struct MoveBoidsJob : IJobEntity
    {
        [ReadOnly] public float deltaTime;


        [BurstCompile(DisableSafetyChecks = true)]
        public void Execute(ref CPosition position, ref CVelocity velocity, in CAimedVelocity aimedVelocity , ref LocalTransform transform)
        {
            velocity.value = aimedVelocity.value;
            position.value = position.value + velocity.value * deltaTime;

            transform.Position = position.value;
            transform.Rotation = TransformHelpers.LookAtRotation(new float3(0f, 0f, 1f),
                                                                         velocity.value,
                                                                         new float3(0f, 1f, 0f));
        }
    }
}
