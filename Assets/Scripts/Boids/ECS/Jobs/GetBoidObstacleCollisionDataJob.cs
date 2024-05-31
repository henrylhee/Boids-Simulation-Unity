using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;


namespace Boids
{
    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    partial struct GetBoidObstacleCollisionDataJob : IJobParallelFor
    {
        [ReadOnly] public int cellCountX;
        [ReadOnly] public int cellCountXY;
        [ReadOnly] public int areaCellCountX;
        [ReadOnly] public int areaCellCountXY;
        [ReadOnly] public int3 areaStartCell;
        [ReadOnly] public int3 areaCellOffset;

        [ReadOnly] public float boidObstacleInteractionRadius;

        [ReadOnly] public NativeArray<int3> hashPivots;
        [ReadOnly] public NativeArray<int> hashTable;
        [ReadOnly] public NativeArray<BoidData> boidDataArr;

        [ReadOnly] public PhysicsCollider col;

        public NativeArray<ObstacleData> obstacleData;


        [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
        public void Execute(int areaCellIndex)
        {
            Collider collider = col.Value.Value;

            int x = areaCellIndex % areaCellCountX;
            int z = /*(int)*/areaCellIndex / areaCellCountXY;
            int y = ((areaCellIndex % areaCellCountXY) - x) / areaCellCountX;
            int3 areaCell = new int3(x,y,z);

            int3 cell = areaStartCell + areaCell;
            int cellIndex = cell.x + cell.y * cellCountX + cell.z * cellCountXY;

            int3 pivot = hashPivots[cellIndex];
            for (int i = pivot.y; i < pivot.z; i++)
            {
                int boidIndex = hashTable[i];

                BoidData boidData = boidDataArr[boidIndex];
                float3 boidPosition = boidData.position;
                PointDistanceInput input = new PointDistanceInput();
                input.Position = boidPosition;
                input.MaxDistance = boidObstacleInteractionRadius;
                DistanceHit hit = new DistanceHit();
                if(collider.CalculateDistance(input, out hit))
                {
                    float3 boidDirection = math.mul(boidData.rotation, new float3(0,0,1));
                    float3 rotationAxis = math.normalizesafe(math.cross(boidDirection, hit.SurfaceNormal));
                    float3 avoidanceDirection = math.mul(quaternion.AxisAngle(rotationAxis, 0.5f*math.PI), hit.SurfaceNormal);
                }
            }
        }
    }
}
