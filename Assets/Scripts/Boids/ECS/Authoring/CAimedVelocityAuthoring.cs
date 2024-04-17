using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Boids
{
    public class CAimedVelocityAuthoring : MonoBehaviour
    {
        private class Baker : Baker<CAimedVelocityAuthoring>
        {
            public override void Bake(CAimedVelocityAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new CAimedVelocity());
            }
        }
    }
}
