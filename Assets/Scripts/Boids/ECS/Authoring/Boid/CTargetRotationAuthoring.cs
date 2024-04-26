using Unity.Entities;
using UnityEngine;

public class CTargetRotationAuthoring : MonoBehaviour
{
    public class Baker : Baker<CTargetRotationAuthoring> 
    {
        public override void Bake(CTargetRotationAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Renderable);
            AddComponent(entity, new CTargetRotation());
        }
    }
}
