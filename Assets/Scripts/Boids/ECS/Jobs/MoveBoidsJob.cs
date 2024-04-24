using System.Diagnostics;
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
        [ReadOnly] public float maxSpeed;

        [BurstCompile]
        public void Execute([EntityIndexInQuery] int boidIndex,in CTargetVector targetVector, ref LocalTransform transform, ref CSpeed speed)
        {
            float length = math.length(targetVector.value);
            speed.value = math.clamp(length * deltaTime, 0, maxSpeed);

            if(length > 0.000000001f) 
            {
                transform = transform.Translate(math.normalize(targetVector.value) * speed.value);
                transform.Rotation = quaternion.LookRotation(targetVector.value, new float3(0f, 1f, 0f));
            }
        }
    }
}
