using UnityEngine;

namespace Boids
{
    [CreateAssetMenu(menuName = "ScriptableObjects/BoidsSpawnConfig", fileName = "BoidsSpawnConfig")]
    public class BoidsSpawnConfig : ScriptableObject
    {
        [field: SerializeField]
        public int boidCount { get; private set; } = 100;

        [field: SerializeField]
        public Vector3 center { get; private set; } = Vector3.zero;

        [field: SerializeField]
        public GenerationShape shape { get; private set; } = GenerationShape.SPHERE;

        [field: SerializeField]
        public float spawnDistance { get; private set; } = 0.1f;

        //[field: SerializeField]
        //public float shapeSize { get; private set; } = 100f;
    }
}
