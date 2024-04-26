using UnityEngine;

namespace Boids
{
    [CreateAssetMenu(menuName = "ScriptableObjects/BoidConfig", fileName = "BoidConfig")]
    public class BoidConfigSO : ScriptableObject
    {
        [field: SerializeField]
        public SpawnConfigSO spawnConfigSO { get; private set; }

        [field: SerializeField]
        public MovementConfigSO movementConfigSO { get; private set; }

        [field: SerializeField]
        public BehaviourConfigSO behaviourConfigSO { get; private set; }
    }
}
