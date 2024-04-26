using UnityEngine;

namespace Boids
{
    [CreateAssetMenu(menuName = "ScriptableObjects/MovementConfig", fileName = "MovementConfig")]
    public class MovementConfigSO : ScriptableObject
    {
        [field: SerializeField]
        public CMovementData Value { get; private set; }
    }
}
