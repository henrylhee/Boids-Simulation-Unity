using System.Collections;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace Boids
{
    [BurstCompile]
    public readonly partial struct BoidEnemyAspect : IAspect
    {
        private readonly RefRO<CBoidTag> boidEnemyTag;

        private readonly RefRW<LocalTransform> transform;
    }
}
