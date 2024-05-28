using System.Threading;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Boids
{
    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    partial struct MoveEnemiesJob : IJobEntity
    {
        [ReadOnly] public NativeArray<float3> targetPositions;
        [ReadOnly] public float deltaTime;
        [ReadOnly] public float speed;
        [ReadOnly] public float angularSpeed;

        [NativeDisableContainerSafetyRestriction] public NativeArray<LocalTransform> enemyTransforms;

        [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
        public void Execute([EntityIndexInQuery] int enemyIndex, ref LocalTransform transform)
        {
            float3 towardsTargetdirection = targetPositions[enemyIndex] - transform.Position;
            quaternion targetRotation = quaternion.LookRotation(towardsTargetdirection, new float3(0f, 1f, 0f));
            quaternion smoothRotation;
            MathExtensions.RotateTowards(in transform.Rotation, in targetRotation, out smoothRotation, angularSpeed * deltaTime);
            transform.Rotation = smoothRotation;

            transform = transform.Translate(math.mul(smoothRotation, new float3(0f, 0f, 1f)) * speed * deltaTime);
            enemyTransforms[enemyIndex] = transform;
        }
    }
}

