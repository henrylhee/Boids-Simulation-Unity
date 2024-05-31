using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Boids
{
    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    partial struct InitializeBoidsJob : IJobEntity
    {
        [ReadOnly] public float startSpeed;
        [ReadOnly] public float startAngularSpeed;
        [ReadOnly] public NativeArray<float3> positions;
        [ReadOnly] public NativeArray<quaternion> rotations;


        [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
        public void Execute([EntityIndexInQuery] int boidIndex, ref LocalTransform transform, ref CSpeed speed, ref CAngularSpeed angularSpeed)
        {
            speed.value = startSpeed;
            angularSpeed.value = startAngularSpeed;
            transform = LocalTransform.FromPositionRotationScale(positions[boidIndex], rotations[boidIndex], transform.Scale);
        }
    }

    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    partial struct InitializeEnemiesJob : IJobEntity
    {
        [ReadOnly] public float speed;
        [ReadOnly] public float angularSpeed;
        [NativeDisableContainerSafetyRestriction] public NativeArray<LocalTransform> enemyTransforms;

        [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
        public void Execute([EntityIndexInQuery] int enemyIndex, ref CSpeed speed, ref CAngularSpeed angularSpeed, in LocalTransform transform)
        {
            speed.value = this.speed;
            angularSpeed.value = this.angularSpeed;
            enemyTransforms[enemyIndex] = transform;
        }
    }

    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    partial struct InitializeSwarmTargetPositionsJob : IJobEntity
    {
        public NativeArray<float3> swarmTargetPositions;

        [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
        public void Execute(in CSwarmTarget swarmTarget, in LocalToWorld localToWorld)
        {
            swarmTargetPositions[swarmTarget.index] = localToWorld.Position;
        }
    }

    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    partial struct InitializeBoidObstacleDataJob : IJobParallelFor
    {
        public NativeArray<ObstacleData> boidObstacleData;

        [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
        public void Execute(int index)
        {
            //if(index == 500)
            //{
            //    UnityEngine.Debug.Log("index: " + index);
            //}
            boidObstacleData[index] = new ObstacleData
            {
                distance = -1
            };
            //if (index == 500)
            //{
            //    UnityEngine.Debug.Log("boidObstacleData[index].distance: " + boidObstacleData[index].distance);
            //}
        }
    }
}

