using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


namespace Boids
{
    public class CPositionAuthoring : MonoBehaviour
    {
        public float3 value;


        private class Baker : Baker<CPositionAuthoring>
        {
            public override void Bake(CPositionAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new CPosition
                {
                    value = authoring.value
                });
            }
        }
    }
}

