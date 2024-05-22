using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Boids
{
    [BurstCompile]
    partial struct MoveBoidsJob : IJobEntity
    {
        [ReadOnly] public float speedFactor;
        [ReadOnly] public float maxSpeed;
        [ReadOnly] public float maxSpeedFleeing;
        [ReadOnly] public float minSpeed;
        [ReadOnly] public float acceleration;
        [ReadOnly] public float accelerationFleeing;
        [ReadOnly] public float maxRadiansRandom;
        [ReadOnly] public float objectiveCenterRatio;
        [ReadOnly] public float cohesionStrength;
        [NativeDisableContainerSafetyRestriction] public NativeArray<Unity.Mathematics.Random> randoms;
        [NativeDisableContainerSafetyRestriction] public NativeArray<float3> boidPositions;
        [NativeSetThreadIndex] [ReadOnly] int threadId;

        [ReadOnly] public float boidVisionRadius;
        [ReadOnly] public float boidEnemyMaxDistance;
        [ReadOnly] public NativeArray<LocalTransform> enemyTransforms;

        [BurstCompile]
        public void Execute([EntityIndexInQuery] int boidIndex, in CRuleVector ruleVector, ref LocalTransform transform, ref CSpeed speed, ref CAngularSpeed angularSpeed)
        {
            float3 normedRuleVector = math.normalizesafe(ruleVector.value);
            float ruleVectorLength = math.length(ruleVector.value);

            // For the boid enemy behaviour the boids are a single point and the enemies have a simple sphere in this example.
            // Therefore its faster to check the distance between boids and enemies directly for each boid. Only performant for small number of enemies. 
            float3 fleeVector = new float3();
            int interactionsCount = 0;
            float fleeRuleRatio = 0;
            for (int i = 0; i < enemyTransforms.Length; i++)
            {
                float3 enemyPosition = enemyTransforms[i].Position;
                float3 distVector = transform.Position - enemyPosition;
                float3 distVectorNormed = math.normalizesafe(distVector);
                float distVectorLength = math.length(distVector);
                if (distVectorLength > boidEnemyMaxDistance) { continue; }
                fleeRuleRatio += boidVisionRadius - distVectorLength;
                float3 enemyDirection = math.mul(transform.Rotation, new float3(0, 0, 1));
                float beta = math.PI * 0.5f - math.dot(distVectorNormed, enemyDirection);
                float3 perpendicularVector = distVector - (enemyDirection * (distVector * math.sin(beta)));
                //float3 orhtoVector = enemyPosition + math.dot()
                fleeVector += math.normalizesafe((distVectorNormed + math.normalizesafe(perpendicularVector))) * fleeRuleRatio;
                interactionsCount++;
            }

            if(interactionsCount > 0)
            {
                fleeRuleRatio = fleeRuleRatio / (interactionsCount + boidVisionRadius);
                fleeVector = math.normalizesafe(fleeVector) * fleeRuleRatio;
                normedRuleVector = math.normalizesafe(fleeVector + normedRuleVector * (1 - fleeRuleRatio));
                ruleVectorLength = maxSpeedFleeing * fleeRuleRatio + ruleVectorLength * (1 - fleeRuleRatio);

                if (speed.value < ruleVectorLength)
                {
                    speed.value = math.clamp(speed.value + accelerationFleeing, speed.value, ruleVectorLength);
                }
                else if (speed.value > ruleVectorLength)
                {
                    speed.value = math.clamp(speed.value - accelerationFleeing, ruleVectorLength, speed.value);
                }
                speed.value = math.clamp(speed.value, minSpeed, maxSpeedFleeing);
            }
            else
            {
                if (speed.value < ruleVectorLength)
                {
                    speed.value = math.clamp(speed.value + acceleration, speed.value, ruleVectorLength);
                }
                else if (speed.value > ruleVectorLength)
                {
                    speed.value = math.clamp(speed.value - acceleration, ruleVectorLength, speed.value);
                }
                speed.value = math.clamp(speed.value, minSpeed, maxSpeed);
            }

            var random = randoms[threadId];
            float3 randomVector = random.NextFloat3(-maxRadiansRandom, maxRadiansRandom);
            float3 adjustedVelocity = math.mul(quaternion.EulerZXY(randomVector), normedRuleVector);
            randoms[threadId] = random;

            quaternion targetRotation = quaternion.LookRotationSafe(adjustedVelocity, new float3(0f, 1f, 0f));
            quaternion smoothRotation;
            MathExtensions.RotateTowards(in transform.Rotation, in targetRotation, out smoothRotation, angularSpeed.value * speedFactor);
            transform.Rotation = smoothRotation;

            transform = transform.Translate(math.mul(smoothRotation, new float3(0f, 0f, 1f)) * speed.value);

            //if (boidIndex == 500)
            //{
            //    UnityEngine.Debug.Log("minSpeed * deltaTime * speedMulRules: " + minSpeed * deltaTime * speedMulRules);
            //    UnityEngine.Debug.Log("maxSpeed * deltaTime * speedMulRules: " + maxSpeed * deltaTime * speedMulRules);
            //    UnityEngine.Debug.Log("- Boid 500 -: " + "----> length: " + length + ". adjusted length: " + math.length(velocity));
            //}
            //if (boidIndex == 999)
            //{
            //    UnityEngine.Debug.Log("- Boid 999 -: " + "----> length: " + length + ". adjusted length: " + math.length(velocity));
            //}
            //if (boidIndex == 1)
            //{
            //    UnityEngine.Debug.Log("- Boid 1 -: " + "----> length: " + length + ". adjusted length: " + math.length(velocity));
            //}
        }
    }
}
