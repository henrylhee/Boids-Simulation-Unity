using Unity.Entities;
using UnityEngine;

public class CRuleVectorAuthoring : MonoBehaviour
{
    public class Baker : Baker<CRuleVectorAuthoring>
    {
        public override void Bake(CRuleVectorAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Renderable);
            AddComponent(entity, new CRuleVector());
        }
    }
}
