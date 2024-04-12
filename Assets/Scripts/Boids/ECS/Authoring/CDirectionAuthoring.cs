using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class CDirectionAuthoring : MonoBehaviour
{
    public class Baker : Baker<CDirectionAuthoring> 
    {
        public override void Bake(CDirectionAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Renderable);
            AddComponent(entity, new CDirection());
        }
    }
}
