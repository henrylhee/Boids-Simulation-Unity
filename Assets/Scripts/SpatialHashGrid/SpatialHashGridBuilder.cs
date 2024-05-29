using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst;

[BurstCompile(OptimizeFor = OptimizeFor.Performance)]
public struct SpatialHashGridBuilder
{
    public float conversionFactor { get; private set; }
    public float3 boundsMax;
    public float3 boundsMin { get; private set; }
    public int3 cellCountAxis { get; private set; }
    public int cellCountXY { get; private set; }
    public int cellCountXYZ{ get; private set; }


    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    public void Build(in NativeArray<BoidData> boidData, ref NativeArray<int3> pivots, ref NativeArray<int> cellIndices, ref NativeArray<int> hashTable)
    {
        BuildContainers(in boidData, ref pivots, ref cellIndices, ref hashTable);
    }

    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    public void SetUp(in BehaviourData settings, in NativeArray<BoidData> boidData)
    {
        conversionFactor = 1f / settings.visionRange;

        UpdateBounds(in boidData);
        SetCellCount();
    }

    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    private void BuildContainers(in NativeArray<BoidData> boidData, ref NativeArray<int3> pivots, 
                                 ref NativeArray<int> cellIndices, ref NativeArray<int> hashTable)
    {
        for (int boidIndex = 0; boidIndex < boidData.Length; boidIndex++)
        {
            int cellIndex = HashFunction(boidData[boidIndex].position);
            cellIndices[boidIndex] = cellIndex;
            pivots[cellIndex] = pivots[cellIndex] + new int3(1,0,0);
        }

        int accum = 0;
        for (int cellIndex = 0; cellIndex < cellCountXYZ; cellIndex++)
        {
            int boidsCountCell = pivots[cellIndex].x;
            if (boidsCountCell != 0)
            {
                pivots[cellIndex] = new int3(pivots[cellIndex].x, accum, accum + boidsCountCell);
                accum += boidsCountCell;
            }
        }

        NativeArray<int> hashBucketCount = new NativeArray<int>(pivots.Length, Allocator.Temp, NativeArrayOptions.ClearMemory);
        for (int boidIndex = 0; boidIndex < boidData.Length; boidIndex++)
        {
            int cellIndex = cellIndices[boidIndex];
            hashTable[pivots[cellIndex].y + hashBucketCount[cellIndex]] = boidIndex;
            hashBucketCount[cellIndex]++;
        }
    }

    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    private int HashFunction(float3 position)
    {
        float3 convertedPosition = (position - boundsMin) * conversionFactor;
        int3 cell = new int3((int)convertedPosition.x, 
                             (int)convertedPosition.y,
                             (int)convertedPosition.z);
        return cell.x + cell.y * cellCountAxis.x + cell.z * cellCountXY;
    }

    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    private void UpdateBounds(in NativeArray<BoidData> boidData)
    {
        float3 newBoundsMin = float3.zero;
        float3 newBoundsMax = float3.zero;

        for (int i = 0;  i < boidData.Length; i++)
        {
            float3 position = boidData[i].position;

            if(position.x > newBoundsMax.x) { newBoundsMax.x = position.x; }
            if(position.x < newBoundsMin.x) { newBoundsMin.x = position.x; }

            if (position.y > newBoundsMax.y) { newBoundsMax.y = position.y; }
            if (position.y < newBoundsMin.y) { newBoundsMin.y = position.y; }

            if (position.z > newBoundsMax.z) { newBoundsMax.z = position.z; }
            if (position.z < newBoundsMin.z) { newBoundsMin.z = position.z; }
        }

        boundsMin = newBoundsMin;
        boundsMax = newBoundsMax;
    }

    [BurstCompile(OptimizeFor = OptimizeFor.Performance)]
    private void SetCellCount()
    {
        float3 convertedGridLength = (boundsMax - boundsMin) * conversionFactor;

        cellCountAxis = new int3((int)convertedGridLength.x + 1, 
                                 (int)convertedGridLength.y + 1, 
                                 (int)convertedGridLength.z + 1);

        cellCountXY = cellCountAxis.x * cellCountAxis.y;
        cellCountXYZ = cellCountXY * cellCountAxis.z;
    }
}