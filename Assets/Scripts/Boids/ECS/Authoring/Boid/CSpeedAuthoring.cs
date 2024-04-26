using Unity.Entities;
using UnityEngine;

public class CSpeedAuthoring : MonoBehaviour
{
    private class Baker : Baker<CSpeedAuthoring>
    {
        public override void Bake(CSpeedAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Renderable);
            AddComponent(entity, new CSpeed());
        }
    }
}
