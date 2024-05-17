using System;
using Unity.Entities;
using UnityEngine;

[Serializable]
public struct CBoidEnemyConfig : IComponentData
{
    public BoidEnemyConfig Config;
}

[Serializable]
public struct BoidEnemyConfig
{
    [field: SerializeField]
    public float speed { get; private set; }
    [field: SerializeField]
    public float angularSpeed { get; private set; }
    [field: SerializeField]
    public float chaseTime { get; private set; }
}