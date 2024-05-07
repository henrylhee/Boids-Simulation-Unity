using Unity.Entities;
using UnityEngine;

namespace Boids
{
    public class BoidEnemyConfigAuthoring : MonoBehaviour
    {
        public BoidEnemyConfigSO configSO;

        private class Baker : Baker<BoidEnemyConfigAuthoring>
        {
            public override void Bake(BoidEnemyConfigAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);

                AddComponent(entity, new CBoidEnemyConfig
                {
                    Config = authoring.configSO.Value.Config,
                });
            }
        }
    }
}