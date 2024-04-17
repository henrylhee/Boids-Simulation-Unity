using System.ComponentModel;
using Unity.Burst;
using Unity.Entities;

namespace Boids
{
    [BurstCompile(DisableSafetyChecks = true)]
    partial struct MoveBoidsJob : IJobEntity
    {
        [ReadOnly] public float deltaTime;

        [BurstCompile(DisableSafetyChecks = true)]
        public void Execute(BoidAspect boidAspect)
        {
            boidAspect.ApplyAimedVelocity();
            boidAspect.Move(deltaTime);
        }
    }
}
