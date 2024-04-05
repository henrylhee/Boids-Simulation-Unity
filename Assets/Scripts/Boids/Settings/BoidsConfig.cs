using UnityEngine;

namespace Boids
{
    [CreateAssetMenu(menuName = "ScriptableObjects/BoidsConfig", fileName = "BoidsConfig")]
    public class BoidsConfig : ScriptableObject
    {
        // cohesionDistance has to be greater than repulsionDistance

        [field: SerializeField, 
         Header("Behaviour Settings"),
         Tooltip("The distance used to calculate the center of mass.")]
        public float CohesionDistance { get; private set; } = 1;

        [field: SerializeField,
         Tooltip("The strength with whom the boid is getting pulled towards the center of mass."),
         Range(0, 1)]
        public float CohesionStrength { get; private set; } = 1;

        [field: SerializeField,
         Tooltip("The distance used to apply the rule of repulsion.")]
        public float RepulsionDistance { get; private set; } = 0.5f;

        [field: SerializeField,
         Tooltip("The strength with whom the boid is getting repulsed from other closeby boids."),
         Range(0, 1)]
        public float RepulsionStrength { get; private set; } = 1;

        [field: SerializeField,
         Tooltip("The strength with whom the boid is getting repulsed from other closeby boids.")]
        public float AllignmentStrength { get; private set; } = 1;


        [field: SerializeField,
         Header("MovementSettings Settings")]
        public float Speed { get; private set; } = 2f;

        [field: SerializeField, 
         Tooltip("Time in which the boid accelerates from 0 to max speed. Deceleration works in the same way.")]
        public float Acceleration { get; private set; } = 1f;

        [field: SerializeField]
        public float AngularSpeed { get; private set; } = 90f;

        //[field: SerializeField]
        //public float AngularAcceleration { get; private set; }
    }
}
