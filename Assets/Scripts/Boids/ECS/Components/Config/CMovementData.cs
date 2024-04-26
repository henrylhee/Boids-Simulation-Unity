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

    public float startAngularSpeed;

    public float speedMulRules;

    [Tooltip("Time in which the boid accelerates from 0 to max speed. Deceleration works in the same way.")]
    public float Acceleration;

    public float AngularAcceleration;
}