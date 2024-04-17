using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using static UnityEngine.Rendering.DebugUI;

namespace Boids
{
    [BurstCompile]
    public readonly partial struct BoidAspect : IAspect
    {
        private readonly RefRO<CBoidTag> boidTag;

        private readonly RefRW<LocalTransform> transform;
        private readonly RefRW<CPosition> position;
        private readonly RefRW<CVelocity> velocity;
        private readonly RefRW<CAimedVelocity> aimedVelocity;
        private readonly RefRW<CAngularSpeed> angularSpeed;

        public float3 Position
        {
            get => position.ValueRO.value;
            set { transform.ValueRW.Position = value; position.ValueRW.value = value; }
        }
        public float3 Velocity
        {
            get => velocity.ValueRO.value;
            set { transform.ValueRW.Rotation = TransformHelpers.LookAtRotation(new float3(0f,0f,1f), value, new float3(0f,1f,0f)); 
                  velocity.ValueRW.value = value; }
        }
        public float Speed
        {
            get => math.length(velocity.ValueRO.value);
            set => velocity.ValueRW.value = math.normalize(velocity.ValueRW.value) * value;
        }
        public float AngularSpeed
        {
            get => angularSpeed.ValueRO.value;
            set => angularSpeed.ValueRW.value = value;
        }

        [BurstCompile]
        public void Initialize()
        {
            transform.ValueRW.Position = position.ValueRO.value;
            transform.ValueRW.Rotation = TransformHelpers.LookAtRotation(new float3(0f, 0f, 1f), velocity.ValueRO.value, new float3(0f, 1f, 0f));
        }

        [BurstCompile]
        public void ApplyAimedVelocity()
        {
            Velocity = aimedVelocity.ValueRO.value;
        }

        [BurstCompile]
        public void Move(in float deltaTime)
        {
            position.ValueRW.value = position.ValueRO.value + velocity.ValueRO.value * deltaTime;
            transform.ValueRW.Position = position.ValueRO.value;
        }
    }
}

