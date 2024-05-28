using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Boids
{
    public class CBoidObstacleTagAuthoring : MonoBehaviour
    {
        private class Baker : Baker<CBoidObstacleTagAuthoring>
        {
            public override void Bake(CBoidObstacleTagAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new CBoidObstacleTag());
            }
        }
    }

    public struct CBoidObstacleTag : IComponentData
    {
    }
}
