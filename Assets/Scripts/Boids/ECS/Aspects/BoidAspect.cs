using Unity.Entities;
using Unity.Transforms;

public readonly partial struct BoidAspect : IAspect
{
    public readonly RefRO<CBoidTag> boidTag;
    public readonly RefRW<LocalToWorld> localToWorld;
    public readonly RefRW<CPosition> position;
    public readonly RefRW<CDirection> direction;
    public readonly RefRW<CSpeed> speed;
}
