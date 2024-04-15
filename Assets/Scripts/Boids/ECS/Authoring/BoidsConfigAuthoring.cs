using Boids;
using Unity.Entities;
using UnityEngine;

namespace Boids
{
    public class BoidsConfigAuthoring : MonoBehaviour
    {
        public Config configSO;
        public GameObject boidPrefab;

        private class Baker : Baker<BoidsConfigAuthoring>
        {
            public override void Bake(BoidsConfigAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);
                
                AddComponent(entity, new CBoidsConfig
                {
                    boidPrefabEntity = GetEntity(authoring.boidPrefab, TransformUsageFlags.Renderable),
                    spawnData = authoring.configSO.spawnConfigSO.Value,
                    movementData = authoring.configSO.movementConfigSO.Value,
                    behaviourData = authoring.configSO.behaviourConfigSO.Value
                });
            }
        }
    }
}