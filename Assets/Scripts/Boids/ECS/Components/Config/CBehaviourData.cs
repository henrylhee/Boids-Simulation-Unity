using System;
using Unity.Entities;
using UnityEngine;

[Serializable]
public struct CBehaviourData : IComponentData
{
    // cohesionDistance has to be greater than repulsionDistance

    [field: SerializeField,
     Header("Behaviour Settings"),
     Tooltip("The distance used to calculate the center of mass.")]
    public float CohesionDistance { get; private set; }

    [field: SerializeField,
     Tooltip("The strength with whom the boid is getting pulled towards the center of mass."),
     Range(0, 1)]
    public float CohesionStrength { get; private set; }

    [field: SerializeField,
     Tooltip("The distance used to apply the rule of repulsion.")]
    public float RepulsionDistance { get; private set; }

    [field: SerializeField,
     Tooltip("The strength with whom the boid is getting repulsed from other closeby boids."),
     Range(0, 1)]
    public float RepulsionStrength { get; private set; }

    [field: SerializeField,
     Tooltip("The strength with whom the boid is getting repulsed from other closeby boids.")]
    public float AllignmentStrength { get; private set; }
}
