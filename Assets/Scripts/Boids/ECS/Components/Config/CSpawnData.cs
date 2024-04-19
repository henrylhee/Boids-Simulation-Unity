using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct CSpawnData : IComponentData
{
    [SerializeField]
    private SpawnData value;
    public SpawnData Value => value;
}

[Serializable]
public struct SpawnData
{

    public int boidCount;

    public float3 center;

    public GenerationShape shape;

    public float spawnDistance;

    //[field: SerializeField]
    //public float shapeSize { get; private set; }
}
