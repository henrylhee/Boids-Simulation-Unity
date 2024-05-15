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

    [Tooltip("The strength with whom the boid is getting repulsed from other closeby boids."),
     Range(0,1)]
    public float AllignmentStrength;

    [Tooltip("A boid reacts to enemies within this range.")]
    public float enemyVisionRadius;

    [Tooltip("The randomness of the targetVector direction in degrees."),
     Range(0, 10)]
    public float DirectionRandomness;

    [Tooltip("The minimum Distance between boids. Getting enforced by internal calculations.")]
    public float MinDistance;

    [Tooltip("The ratio between the movement vectors of swarmObjective / swarmCenter."),
     Range(0, 1)]
    public float objectiveCenterRatio;
}
