using System;
using Unity.Entities;
using UnityEngine;

[Serializable]
public struct CBehaviourData : IComponentData
{
    public BehaviourData Value;
}

[Serializable]
public struct BehaviourData
{
    // cohesionDistance has to be greater than separationDistance

    [Header("Behaviour Settings"),
     Tooltip("The distance used to calculate the center of mass.")]
    public float visionRadius;

    //[Tooltip("The distance used to apply the rule of separation.")]
    //public float separationDistance;

    [Tooltip("The strength with whom the boid is moving towards the local center of mass."),
     Range(0, 1f)]
    public float cohesionStrength;

    [Tooltip("The percentage with whom the boid is closing the distance towards the swarmcenter each second."),
     Range(0, 1f)]
    public float globalCohesionStrength;

    [Tooltip("The strength with whom the boid is getting repulsed from other closeby boids."),
     Range(0, 2)]
    public float separationStrength;

    [Tooltip("The strength with whom the boid is aligning with other closeby boids."),
     Range(0,1)]
    public float alignmentStrength;

    [Tooltip("The strength with whom the boid is moving towards an objective."),
     Range(0, 1f)]
    public float objectiveStrength;

    [Tooltip("A boid reacts to enemies within this range.")]
    public float visionRadiusEnemies;

    [Tooltip("A boid reacts to obstacles within this range.")]
    public float obstacleInteractionRadius;

    [Tooltip("The randomness of the targetVector direction in degrees."),
     Range(0, 10)]
    public float DirectionRandomness;
}
