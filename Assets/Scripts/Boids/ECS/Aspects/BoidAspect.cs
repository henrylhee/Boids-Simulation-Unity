using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using static UnityEngine.Rendering.DebugUI;

namespace Boids
{
    [BurstCompile]
    public readonly partial struct BoidAspect : IAspect
    {
        private readonly RefRO<CBoidTag> boidTag;

        private readonly RefRW<LocalTransform> transform;
        private readonly RefRW<CSpeed> speed;

        private readonly RefRW<CTargetRotation> targetRotation;
        private readonly RefRW<CTargetSpeed> targetSpeed;
        private readonly RefRW<CAngularSpeed> angularSpeed;
    }
}

