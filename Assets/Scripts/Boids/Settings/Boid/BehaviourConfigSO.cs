using UnityEngine;

namespace Boids
{
    [CreateAssetMenu(menuName = "ScriptableObjects/BehaviourConfig", fileName = "BehaviourConfig")]
    public class BehaviourConfigSO : ScriptableObject
    {
        [field: SerializeField]
        public CBehaviourData Value { get; private set; }
    }
}
