using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public class CBoidTagAuthoring : MonoBehaviour 
{
    public class Baker : Baker<CBoidTagAuthoring> 
    {
        public override void Bake(CBoidTagAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
            AddComponent(entity, new CBoidTag());
        }
    }
}
