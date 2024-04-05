using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

public partial struct BoidsSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<LocalTransform>();
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        foreach(RefRW<LocalTransform> localTransform in SystemAPI.Query<RefRW<LocalTransform>>())
        {
            localTransform.ValueRW = localTransform.ValueRO.RotateY(5f * SystemAPI.Time.DeltaTime);
        }
    }
}
