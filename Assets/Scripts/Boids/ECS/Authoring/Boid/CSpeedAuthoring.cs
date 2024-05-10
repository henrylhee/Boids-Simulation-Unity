using Unity.Entities;
using UnityEngine;

public class CSpeedAuthoring : MonoBehaviour
{
    private class Baker : Baker<CSpeedAuthoring>
    {
        public override void Bake(CSpeedAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new CSpeed());
        }
    }
}
