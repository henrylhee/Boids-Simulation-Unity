using System;
using Unity.Entities;
using UnityEngine;

[Serializable]
public struct CMovementData : IComponentData
{
    [SerializeField]
    private MovementData value;
    public MovementData Value => value;
}

[Serializable]
public struct MovementData
{
    public float startSpeed;

    public float minSpeed;

    public float maxSpeed;

    public float maxSpeedFleeing;

    public float speedTowardsObjective;

    public float startAngularSpeed;

    public float speedMul;

    [Tooltip("1/seconds in which the boid accelerates from 0 to 1. Deceleration works in the same way.")]
    public float acceleration;

    public float accelerationFleeing;

    public float angularAcceleration;
}