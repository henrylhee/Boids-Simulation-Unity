using UnityEngine;

namespace Boids
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Config", fileName = "Config")]
    public class Config : ScriptableObject
    {
        [field:SerializeField]
        public SpawnConfigSO spawnConfigSO { get; private set; }

        [field:SerializeField]
        public MovementConfigSO movementConfigSO { get; private set; }

        [field:SerializeField]
        public BehaviourConfigSO behaviourConfigSO { get; private set; }
    }
}
