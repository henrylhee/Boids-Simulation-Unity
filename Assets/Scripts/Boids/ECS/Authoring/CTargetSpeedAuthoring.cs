using Unity.Entities;
using UnityEngine;

public class CTargetSpeedAuthoring : MonoBehaviour
{
    private class Baker : Baker<CTargetSpeedAuthoring>
    {
        public override void Bake(CTargetSpeedAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Renderable);
            AddComponent(entity, new CTargetSpeed());
        }
    }
}
