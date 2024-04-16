using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Boids
{
    public readonly partial struct BoidAspect : IAspect
    {
        private readonly RefRO<CBoidTag> boidTag;

        private readonly RefRW<LocalTransform> transform;
        private readonly RefRW<CPosition> position;
        private readonly RefRW<CVelocity> velocity;
        private readonly RefRW<CAngularSpeed> angularSpeed;

        public float3 Position
        {
            get => position.ValueRO.value;
            set { transform.ValueRW.Position = value; position.ValueRW.value = value; }
        }
        public float3 Velocity
        {
            get => velocity.ValueRO.value;
            set { transform.ValueRW.Rotation = value; velocity.ValueRW.value = value; }
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


        public void Initialize()
        {
            transform.ValueRW.Position = position.ValueRO.value;
            transform.ValueRW.Rotation = velocity.ValueRO.value;


        }
    }
}

