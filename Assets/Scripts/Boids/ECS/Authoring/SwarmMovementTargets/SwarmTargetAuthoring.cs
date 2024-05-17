using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Entities;
using UnityEngine;

namespace Boids
{
    public class SwarmTargetAuthoring : MonoBehaviour
    {
        [SerializeField]
        private int index;

        private class Baker : Baker<SwarmTargetAuthoring>
        {
            public override void Bake(SwarmTargetAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.WorldSpace);
                AddComponent(entity, new CSwarmTarget 
                { 
                    index = authoring.index
                });
            }
        }
    }
}
