using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Physics.Systems;
using UnityEngine;

namespace Boids
{
    [BurstCompile]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(PhysicsInitializeGroup))]
    [UpdateBefore(typeof(PhysicsSimulationGroup))]
    public partial struct BoidEnemySystem : ISystem, ISystemStartStop
    {
        NativeArray<Entity> targets;
        RefRW<PhysicsWorldSingleton> worldSingleton;
        SystemHandle boidsSystemHandle;

        public void OnStart(ref SystemState state)
        {
            worldSingleton = SystemAPI.GetSingletonRW<PhysicsWorldSingleton>();
        }

        public void OnStartRunning(ref SystemState state)
        {
            boidsSystemHandle = state.World.GetExistingSystem<BoidsEcsSystem>();
        }

        public void OnStopRunning(ref SystemState state)
        {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            ref BoidsEcsSystem boidsSystem = ref state.WorldUnmanaged.GetUnsafeSystemRef<BoidsEcsSystem>(boidsSystemHandle);


            state.Dependency = new ApplyVelocityJob
            {
                physicsWorld = worldSingleton.ValueRW.PhysicsWorld
            }.Schedule(state.Dependency);
        }

        [BurstCompile]
        public partial struct ApplyVelocityJob : IJobEntity
        {
            public PhysicsWorld physicsWorld;
            

            public void Execute()
            {
                // values randomly selected
                physicsWorld.ApplyImpulse(3, new float3(1, 0, 0), new float3(1, 1, 1));
            }
        }
    }
}
