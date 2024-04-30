using System.Diagnostics;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
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
        [ReadOnly] public float minSpeed;
        [ReadOnly] public float maxRadiansRandom;
        [ReadOnly] public float3 swarmObjective;
        [ReadOnly] public float3 swarmCenter;
        [ReadOnly] public float objectiveCenterRatio;
        [ReadOnly] public float speedMulRules;
        [NativeDisableContainerSafetyRestriction] public NativeArray<Random> randoms;
        [NativeSetThreadIndex] [ReadOnly] int threadId;

        [BurstCompile]
        public void Execute([EntityIndexInQuery] int boidIndex, in CTargetVector targetVector, ref LocalTransform transform, ref CSpeed speed, ref CAngularSpeed angularSpeed)
        {
            float3 velocity = targetVector.value * deltaTime * speedMulRules;
            float length = math.length(velocity);
            if(length < minSpeed)
            {
                float3 position = transform.Position;
                float3 objectiveVector = math.normalize(swarmObjective - position) * objectiveCenterRatio;
                float3 centerVector = math.normalize(swarmCenter - position) * (1 - objectiveCenterRatio);
                velocity = math.normalize(objectiveVector + centerVector) * (minSpeed - length) + velocity;
            }

            //if (boidIndex == 500)
            //{
            //    UnityEngine.Debug.Log("- Boid 500 -: " + "----> length: " + length + ". adjusted length: " + math.length(velocity));
            //}
            //if (boidIndex == 999)
            //{
            //    UnityEngine.Debug.Log("- Boid 999 -: " + "----> length: " + length + ". adjusted length: " + math.length(velocity));
            //                }
            //if (boidIndex == 1)
            //{
            //    UnityEngine.Debug.Log("- Boid 1 -: " + "----> length: " + length + ". adjusted length: " + math.length(velocity));
            //}

            length = math.length(velocity);
            speed.value = math.clamp(length, 0, maxSpeed);

            if (length > 0.000000001f) 
            {
                float3 normedVelocity = math.normalize(velocity);

                var random = randoms[threadId];
                float3 randomVector = random.NextFloat3(-maxRadiansRandom, maxRadiansRandom);
                float3 adjustedVelocity = math.mul(quaternion.EulerZXY(randomVector), normedVelocity);
                randoms[threadId] = random;

                quaternion targetRotation = quaternion.LookRotation(adjustedVelocity, new float3(0f, 1f, 0f));
                quaternion smoothRotation;
                MathExtensions.RotateTowards(in transform.Rotation, in targetRotation, out smoothRotation, angularSpeed.value * deltaTime);
                transform.Rotation = smoothRotation;

                transform = transform.Translate(math.mul(smoothRotation, new float3(0f,0f,1f)) * length);


                
            }
        }
    }
}
