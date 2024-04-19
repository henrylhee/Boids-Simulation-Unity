using System;
using Unity.Entities;
using UnityEngine;

[Serializable]
public struct CBehaviourData : IComponentData
{
    [SerializeField]
    private BehaviourData value;
    public BehaviourData Value => value;
}

[Serializable]
public struct BehaviourData
{
    // cohesionDistance has to be greater than repulsionDistance

    [Header("Behaviour Settings"),
     Tooltip("The distance used to calculate the center of mass.")]
    public float CohesionDistance;

    [Tooltip("The strength with whom the boid is getting pulled towards the center of mass."),
     Range(0, 1)]
    public float CohesionStrength;

    [Tooltip("The distance used to apply the rule of repulsion.")]
    public float RepulsionDistance;

    [Tooltip("The strength with whom the boid is getting repulsed from other closeby boids."),
     Range(0, 1)]
    public float RepulsionStrength;

    [Tooltip("The strength with whom the boid is getting repulsed from other closeby boids.")]
    public float AllignmentStrength;
}
