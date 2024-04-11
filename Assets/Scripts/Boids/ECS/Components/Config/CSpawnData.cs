using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[Serializable]
public struct CSpawnData : IComponentData
{
    [field: SerializeField]
    public int boidCount { get; private set; }

    [field: SerializeField]
    public float3 center { get; private set; }

    [field: SerializeField]
    public GenerationShape shape { get; private set; }

    [field: SerializeField]
    public float spawnDistance { get; private set; }

    //[field: SerializeField]
    //public float shapeSize { get; private set; }
}
