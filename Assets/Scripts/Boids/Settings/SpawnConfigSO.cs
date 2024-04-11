using UnityEngine;

namespace Boids
{
    [CreateAssetMenu(menuName = "ScriptableObjects/SpawnConfig", fileName = "SpawnConfig")]
    public class SpawnConfigSO : ScriptableObject
    {
        [field: SerializeField]
        public CSpawnData Value { get; private set; }
    }
}
