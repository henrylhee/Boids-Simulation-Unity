using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct CBoidsConfig : IComponentData
{
    public Entity boidPrefabEntity;
    public CSpawnData spawnData;
    public CMovementData movementData;
    public CBehaviourData behaviourData;
}
