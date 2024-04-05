using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Boids
{
    public class CVelocityAuthoring : MonoBehaviour
    {
        public float3 value;


        private class Baker : Baker<CVelocityAuthoring>
        {
            public override void Bake(CVelocityAuthoring authoring) 
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new CVelocity
                {
                    value = authoring.value
                });
            }
        }
    }
}

