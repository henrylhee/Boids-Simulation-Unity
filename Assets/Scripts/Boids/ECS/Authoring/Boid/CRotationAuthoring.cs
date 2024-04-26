using Unity.Entities;
using UnityEngine;

public class CRotationAuthoring : MonoBehaviour
{
    public class Baker : Baker<CRotationAuthoring> 
    {
        public override void Bake(CRotationAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Renderable);
            AddComponent(entity, new CRotation());
        }
    }
}
