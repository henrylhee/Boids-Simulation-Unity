using Unity.Entities;
using UnityEngine;

namespace Boids
{
    public class BoidTargetAuthoring : MonoBehaviour
    {
        private class Baker : Baker<BoidTargetAuthoring>
        {
            public override void Bake(BoidTargetAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);

                AddComponent(entity, new CBoidTarget());
            }
        }
    }
}