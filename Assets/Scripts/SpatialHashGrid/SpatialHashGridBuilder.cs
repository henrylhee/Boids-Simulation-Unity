using Unity.Mathematics;
using static DataConversion;
using Unity.Collections;
using System.Diagnostics;

public class SpatialHashGridBuilder
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


    public void Inititalize(float cellSize, float3 boundsMin, float3 boundsMax)
    {
        this.cellSize = cellSize;
        conversionFactor = 1f / cellSize;

        //this.voxelSearchFactor = voxelSearchFactor;

        UpdateBounds(boundsMin, boundsMax);
    }

    public void UpdateBounds(float3 boundsMin, float3 boundsMax)
    {
        this.boundsMin = boundsMin;
        this.boundsMax = boundsMax;

        SetCellCount();
    }

    public void Build(NativeArray<float3> positions)
    {
        this.positions = positions.ToArray();

        BuildContainers();
    }

    public NativeArray<int3> GetPivots()
    {
        return new NativeArray<int3>(pivots, Allocator.TempJob);
    }

    public NativeArray<int> GetCellIndices()
    {
        return new NativeArray<int>(cellIndices, Allocator.TempJob);
    }

    public NativeArray<int> GetHashTable()
    {
        return new NativeArray<int>(hashTable, Allocator.TempJob);
    }

    public int3 GetCellCountAxis() => cellCountAxis;

    private void BuildContainers()
    {
        pivots = new int3[cellCountXYZ];
        cellIndices = new int[positions.Length];
        hashTable = new int[positions.Length];

        for(int boidIndex = 0; boidIndex < positions.Length; boidIndex++)
        {
            int cellIndex = HashFunction(boidIndex);
            cellIndices[boidIndex] = cellIndex;
            if(cellIndex >= cellCountXYZ)
            {
                UnityEngine.Debug.Log("cellIndex " + cellIndex + " is greater than cellCountXYZ: " + cellCountXYZ + 
                                      " in boidIndex: " + boidIndex + " with position: " + positions[boidIndex]);
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

    private int HashFunction(int i)
    {
        int3 cell = FloorToInt((positions[i] - boundsMin) * conversionFactor);
        return GetCellIndex(cell);
    }

    private int GetCellIndex(int3 cell)
    {
        return cell.x + cell.y*cellCountAxis.x + cell.z*cellCountXY;
    }

    private void SetCellCount()
    {
        float3 v = (boundsMax - boundsMin) * conversionFactor;

        cellCountAxis = CeilToInt(v);
        cellCountXY = cellCountAxis.x * cellCountAxis.y;
        cellCountXYZ = cellCountAxis.x * cellCountAxis.y * cellCountAxis.z;
    }
}
