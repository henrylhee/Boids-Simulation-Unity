using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Boids
{
    public class CVelocityAuthoring : MonoBehaviour
    {
        private class Baker : Baker<CVelocityAuthoring>
        {
            public override void Bake(CVelocityAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new CVelocity());
            }
        }
    }
}
