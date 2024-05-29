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

        [ReadOnly] public NativeArray<int3> hashPivots;
        [ReadOnly] public NativeArray<int> hashTable;
        [ReadOnly] public NativeArray<BoidData> boidData;

        [ReadOnly] public PhysicsCollider col;

        public NativeArray<ObstacleData> obstacleData;


        [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
        public void Execute([EntityIndexInQuery] int areaCellIndex)
        {
            int x = areaCellIndex % areaCellCountX;
            int z = /*(int)*/areaCellIndex / areaCellCountXY;
            int y = ((areaCellIndex % areaCellCountXY) - x) / areaCellCountX;
            int3 areaCell = new int3(x,y,z);

            int3 cell = areaStartCell + areaCell;
            int cellIndex = cell.x + cell.y * cellCountX + cell.z * cellCountXY;

            int3 pivot = hashPivots[cellIndex];
            for (int i = pivot.y; i < pivot.z; i++)
            {
                int boidIndexToCheck = hashTable[i];

                
            }
        }
    }
}
