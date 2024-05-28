using System.Collections;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace Boids
{
    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    public readonly partial struct BoidEnemyAspect : IAspect
    {
        private readonly RefRO<CBoidEnemyTag> boidEnemyTag;

        private readonly RefRW<LocalTransform> transform;
        private readonly RefRW<CSpeed> speed;
        private readonly RefRW<CAngularSpeed> angularSpeed;
    }
}
