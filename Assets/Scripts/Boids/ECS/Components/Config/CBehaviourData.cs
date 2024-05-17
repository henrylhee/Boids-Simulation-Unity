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
    // cohesionDistance has to be greater than repulsionDistance

    [Header("Behaviour Settings"),
     Tooltip("The distance used to calculate the center of mass.")]
    public float visionRange;

    [Tooltip("The distance used to apply the rule of repulsion.")]
    public float repulsionDistance;

    [Tooltip("The percentage with whom the boid is closing the distance towards the swarmcenter each second."),
     Range(0, 0.1f)]
    public float cohesionStrength;
    
    [Tooltip("The strength with whom the boid is getting repulsed from other closeby boids."),
     Range(0, 2)]
    public float repulsionStrength;

    [Tooltip("The strength with whom the boid is alligning with other closeby boids."),
     Range(0,1)]
    public float allignmentStrength;

    [Tooltip("The strength with whom the boid is moving towards an objective."),
     Range(0, 0.1f)]
    public float objectiveStrength;

    [Tooltip("A boid reacts to enemies within this range.")]
    public float enemyVisionRadius;

    [Tooltip("The randomness of the targetVector direction in degrees."),
     Range(0, 10)]
    public float DirectionRandomness;

    [Tooltip("The ratio between the movement vectors of swarmObjective / swarmCenter."),
     Range(0, 1)]
    public float objectiveCenterRatio;

    [Tooltip("This radius around the swarm target position describes a sphere. " +
             "When the mass center of the swarm is inside, the swarm target position is updated.")]
    public float swarmTargetRadius;

    public float maxSpeedCohesion;

    public float maxSpeedObjective;

}
