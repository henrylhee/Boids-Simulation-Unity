using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst;

[BurstCompile]
public struct SpatialHashGridBuilder
{
    public float conversionFactor { get; private set; }
    public float3 boundsMax;
    public float3 boundsMin { get; private set; }
    public int3 cellCountAxis { get; private set; }
    public int cellCountXY { get; private set; }
    int cellCountXYZ;


    [BurstCompile]
    public void Build(in NativeArray<CPosition> positions, in BehaviourData settings, out NativeArray<int3> pivots, ref NativeArray<int> cellIndices,
                             ref NativeArray<int> hashTable)
    {
        conversionFactor = 1f / settings.CohesionDistance;

        //positions.Reinterpret<float3>().CopyTo(this.positions);

        UpdateBounds(in positions);
        SetCellCount();

        pivots = new NativeArray<int3>(cellCountXYZ, Allocator.Temp);

        BuildContainers(in positions, ref pivots, ref cellIndices, ref hashTable);
    }

    [BurstCompile]
    private void BuildContainers(in NativeArray<CPosition> positions, ref NativeArray<int3> pivots, ref NativeArray<int> cellIndices, ref NativeArray<int> hashTable)
    {
        for (int boidIndex = 0; boidIndex < positions.Length; boidIndex++)
        {
            int cellIndex = HashFunction(boidIndex, in positions);
            cellIndices[boidIndex] = cellIndex;
            if(cellIndex >= cellCountXYZ)
            {
                float3 convertedGridLength = (boundsMax - boundsMin) * conversionFactor;
                //UnityEngine.Debug.Log("#####");
                //UnityEngine.Debug.Log("convertedGridLength: " + convertedGridLength);
                //UnityEngine.Debug.Log("cellCountAxis: " + cellCountAxis);
                //UnityEngine.Debug.Log("boundsMin: " + boundsMin);
                //UnityEngine.Debug.Log("boundsMax: " + boundsMax);
                //UnityEngine.Debug.Log("cellIndex " + cellIndex + " is greater than cellCountXYZ: " + cellCountXYZ + 
                //                      " in boidIndex: " + boidIndex + " with position: " + positions[boidIndex]);
                return;
            }

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

        NativeArray<int> hashBucketCount = new NativeArray<int>(pivots.Length,Allocator.Temp);
        for (int boidIndex = 0; boidIndex < positions.Length; boidIndex++)
        {
            int cellIndex = cellIndices[boidIndex];
            hashTable[pivots[cellIndex].y + hashBucketCount[cellIndex]] = boidIndex;
            hashBucketCount[cellIndex]++;
        }
    }

    [BurstCompile]
    private int HashFunction(int i, in NativeArray<CPosition> positions)
    {
        float3 convertedPosition = (positions[i].value - boundsMin) * conversionFactor;
        int3 cell = new int3((int)math.ceil(convertedPosition.x), 
                             (int)math.ceil(convertedPosition.y),
                             (int)math.ceil(convertedPosition.z)) - 1;
        return math.clamp(cell.x, 0, cell.x) + 
               math.clamp(cell.y * cellCountAxis.x, 0, cell.y * cellCountAxis.x) + 
               math.clamp(cell.z * cellCountXY, 0, cell.z * cellCountXY);
    }

    [BurstCompile]
    private void UpdateBounds(in NativeArray<CPosition> positions)
    {
        float3 newBoundsMin = float3.zero;
        float3 newBoundsMax = float3.zero;

        for (int i = 0;  i < positions.Length; i++)
        {
            float3 position = positions[i].value;

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

    [BurstCompile]
    private void SetCellCount()
    {
        float3 convertedGridLength = (boundsMax - boundsMin) * conversionFactor;

        cellCountAxis = new int3((int)math.ceil(convertedGridLength).x, 
                                 (int)math.ceil(convertedGridLength).y, 
                                 (int)math.ceil(convertedGridLength).z);

        cellCountXY = cellCountAxis.x * cellCountAxis.y;
        cellCountXYZ = cellCountXY * cellCountAxis.z;
    }
}