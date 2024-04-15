using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

public readonly partial struct BoidAspect : IAspect
{
    private readonly RefRO<CBoidTag> boidTag;

    private readonly RefRW<LocalTransform> transform;
    private readonly RefRW<CPosition> position;
    private readonly RefRW<CRotation> rotation;
    private readonly RefRW<CSpeed> speed;

    public float3 Position
    {
        get => position.ValueRO.value; 
        set => { transform.ValueRW.Position = value; position.ValueRO.value = value; }
    }
    public quaternion Rotation
    {
        get => transform.ValueRO.Rotation;
        set => transform.ValueRW.Rotation = value;
    }
    public float Speed
    {
        get => speed.ValueRO.value;
        set => speed.ValueRW.value = value;
    }

    //private readonly RefRW<CPosition> position;
    //private readonly RefRW<CDirection> direction;
}
