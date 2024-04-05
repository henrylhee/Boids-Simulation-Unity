using Unity.Entities;
using UnityEngine;


namespace Boids
{
    public class CStateAuthoring : MonoBehaviour
    {
        public int value;


        private class Baker : Baker<CStateAuthoring>
        {
            public override void Bake(CStateAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new CState
                {
                    value = authoring.value
                });
            }
        }
    }
}

