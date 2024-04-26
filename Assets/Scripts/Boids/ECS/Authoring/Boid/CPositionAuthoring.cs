using Unity.Entities;
using UnityEngine;

public class CPositionAuthoring : MonoBehaviour
{
    public class Baker : Baker<CPositionAuthoring> 
    {
        public override void Bake(CPositionAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Renderable);
            AddComponent(entity, new CPosition());
        }
    }
}
