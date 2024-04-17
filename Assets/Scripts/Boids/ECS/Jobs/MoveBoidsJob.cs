using Unity.Burst;
using Unity.Entities;

namespace Boids
{
    [BurstCompile(DisableSafetyChecks = true)]
    partial struct MoveBoidsJob : IJobEntity
    {
        public float deltaTime;

        [BurstCompile(DisableSafetyChecks = true)]
        public void Execute(BoidAspect boidAspect)
        {
            boidAspect.ApplyAimedVelocity();
            boidAspect.Move(deltaTime);
        }
    }
}
