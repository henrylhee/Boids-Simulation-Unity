using Unity.Entities;
using UnityEngine;

public class CTargetVectorAuthoring : MonoBehaviour
{
    public class Baker : Baker<CTargetVectorAuthoring>
    {
        public override void Bake(CTargetVectorAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Renderable);
            AddComponent(entity, new CTargetVector());
        }
    }
}
