using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

namespace Boids
{
    public class CBoidEnemyTagAuthoring : MonoBehaviour
    {
        private class Baker : Baker<CBoidEnemyTagAuthoring>
        {
            public override void Bake(CBoidEnemyTagAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new CBoidEnemyTag());
            }
        }
    }
}
