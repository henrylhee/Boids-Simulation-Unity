using UnityEngine;

namespace Boids
{
    [CreateAssetMenu(menuName = "ScriptableObjects/BoidEnemyConfig", fileName = "BoidEnemyConfig")]
    public class BoidEnemyConfigSO : ScriptableObject
    {
        [field: SerializeField]
        public BoidEnemyConfig Value { get; private set; }
    }
}