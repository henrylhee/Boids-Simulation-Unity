using Unity.Entities;

public struct CBoidsConfig : IComponentData
{
    public Entity boidPrefabEntity;
    public CSpawnData spawnData;
    public CMovementData movementData;
    public CBehaviourData behaviourData;
}
