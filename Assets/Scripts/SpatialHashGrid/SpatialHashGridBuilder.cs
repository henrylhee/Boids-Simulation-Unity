using Unity.Mathematics;
using static DataConversion;
using Unity.Collections;
using System.Diagnostics;
using Unity.VisualScripting;
using Unity.Burst;

[BurstCompile]
public struct SpatialHashGridBuilder
{
    private float cellSize;
    private float3 boundsMin;
    private float3 boundsMax;

    private int3 cellCountAxis;
    private int cellCountXY;
    private int cellCountXYZ;

    private float conversionFactor;
    private int voxelSearchFactor;
    private int3 voxelSearchCount;

    private float3[] positions;

    int[] cellIndices;
    int3[] pivots;
    int[] hashTable;


    public void Inititalize(float cellSize, int boidCount)
    {
        this.cellSize = cellSize;
        conversionFactor = 1f / cellSize;

        SetCellCount();

        cellIndices = new int[boidCount];
        hashTable = new int[boidCount];

        //this.voxelSearchFactor = voxelSearchFactor;
    }

    public void Build(in NativeArray<CPosition> positions)
    {
        this.positions = positions.Reinterpret<float3>().ToArray();

        UpdateBounds();
        SetCellCount();
        BuildContainers();
    }

    public void GetPivots(ref NativeArray<int3> pivots) => pivots.CopyFrom(this.pivots);
    public void GetCellIndices(ref NativeArray<int> cellIndices) => cellIndices.CopyFrom(this.cellIndices);
    public void GetHashTable(ref NativeArray<int> hashTable) => hashTable.CopyFrom(this.hashTable);
    public int3 GetCellCountAxis() => cellCountAxis;
    public int GetCellCountXY() => cellCountXY;
    public float GetConversionFactor() => conversionFactor;
    public float3 GetBoundsMin() => boundsMin;
    public float3 GetBoundsMax() => boundsMax;

    private void BuildContainers()
    {
        pivots = new int3[positions.Length];

        for (int boidIndex = 0; boidIndex < positions.Length; boidIndex++)
        {
            int cellIndex = HashFunction(boidIndex);
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
            pivots[cellIndex].x++;
        }

        int accum = 0;
        for (int cellIndex = 0; cellIndex < cellCountXYZ; cellIndex++)
        {
            int boidsCountCell = pivots[cellIndex].x;
            if (boidsCountCell != 0)
            {
                pivots[cellIndex].y = accum;
                accum += boidsCountCell;
                pivots[cellIndex].z = accum;
            }
        }

        int[] hashBucketCount = new int[pivots.Length];
        for (int boidIndex = 0; boidIndex < positions.Length; boidIndex++)
        {
            int cellIndex = cellIndices[boidIndex];
            hashTable[pivots[cellIndex].y + hashBucketCount[cellIndex]] = boidIndex;
            hashBucketCount[cellIndex]++;
        }
    }

    [BurstCompile]
    private int HashFunction(int i)
    {
        float3 convertedPosition = (positions[i] - boundsMin) * conversionFactor;
        int3 cell = new int3((int)math.ceil(convertedPosition.x), 
                             (int)math.ceil(convertedPosition.y),
                             (int)math.ceil(convertedPosition.z)) - 1;
        return math.clamp(cell.x, 0, cell.x) + 
               math.clamp(cell.y * cellCountAxis.x, 0, cell.y * cellCountAxis.x) + 
               math.clamp(cell.z * cellCountXY, 0, cell.z * cellCountXY);
    }

    [BurstCompile]
    private void UpdateBounds()
    {
        float3 newBoundsMin = float3.zero;
        float3 newBoundsMax = float3.zero;

        for (int i = 0;  i < positions.Length; i++)
        {
            float3 position = positions[i];

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
