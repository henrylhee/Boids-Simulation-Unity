using Unity.Entities;
using Unity.Mathematics;

namespace Boids
{
    public struct CPosition : IComponentData
    {
        public float3 value;
    }
}
