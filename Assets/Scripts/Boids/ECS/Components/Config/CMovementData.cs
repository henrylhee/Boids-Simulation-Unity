using System;
using Unity.Entities;
using UnityEngine;

[Serializable]
public struct CMovementData : IComponentData
{
    [field: SerializeField]
    public float Speed { get; private set; }

    [field: SerializeField,
     Tooltip("Time in which the boid accelerates from 0 to max speed. Deceleration works in the same way.")]
    public float Acceleration { get; private set; }

    [field: SerializeField]
    public float AngularSpeed { get; private set; }

    //[field: SerializeField]
    //public float AngularAcceleration { get; private set; }
}
