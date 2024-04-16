using Unity.Entities;
using UnityEngine;

public class CAngularSpeedAuthoring : MonoBehaviour
{
    private class Baker : Baker<CAngularSpeedAuthoring>
    {
        public override void Bake(CAngularSpeedAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.Renderable);
            AddComponent(entity, new CAngularSpeed());
        }
    }
}
