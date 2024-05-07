using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace Boids
{
    [BurstCompile]
    public readonly partial struct BoidAspect : IAspect
    {
        private readonly RefRO<CBoidTag> boidTag;

        private readonly RefRW<LocalTransform> transform;
        private readonly RefRW<CSpeed> speed;
        private readonly RefRW<CAngularSpeed> angularSpeed;

        private readonly RefRW<CRuleVector> ruleVector;
    }
}

