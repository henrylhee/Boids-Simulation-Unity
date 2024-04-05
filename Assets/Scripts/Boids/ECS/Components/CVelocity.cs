using Unity.Entities;
using Unity.Mathematics;

namespace Boids
{
    public struct CVelocity : IComponentData
    {
        public float3 value;
    }
}
