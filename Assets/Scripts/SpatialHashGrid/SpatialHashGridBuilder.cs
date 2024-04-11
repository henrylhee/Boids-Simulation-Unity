using Unity.Mathematics;
using static DataConversion;
using Unity.Collections;

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


    public void Inititalize(float cellSize, float3 boundsMin, float3 boundsMax, int voxelSearchFactor)
    {
        this.cellSize = cellSize;
        conversionFactor = 1f / cellSize;

        this.voxelSearchFactor = voxelSearchFactor;

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

    private void BuildContainers()
    {
        pivots = new int3[cellCountXYZ];
        cellIndices = new int[positions.Length];
        hashTable = new int[positions.Length];

        for(int i = 0; i < positions.Length; i++)
        {
            int cellIndex = HashFunction(i);
            cellIndices[i] = cellIndex;
            pivots[cellIndex].x++;
        }

        int accum = 0;
        for (int i = 0; i < cellCountXYZ; i++)
        {
            int boidsCountCell = pivots[i].x;
            if (boidsCountCell != 0)
            {
                pivots[i].y = accum;
                accum += boidsCountCell;
                pivots[i].z = accum;
            }
        }

        int[] hashBucketCount = new int[pivots.Length];
        for (int i = 0; i < positions.Length; i++)
        {
            int cellIndex = cellIndices[i];
            hashTable[pivots[cellIndex].y + hashBucketCount[cellIndex]] = i;
            hashBucketCount[cellIndex]++;
        }
    }

    private int HashFunction(int i)
    {
        int3 cell = FloorFloatToInt((positions[i] - boundsMin) * conversionFactor);
        return GetCellIndex(cell);
    }

    private int GetCellIndex(int3 cell)
    {
        return cell.x + cell.y*cellCountAxis.x + cell.z*cellCountXY;
    }

    private void SetCellCount()
    {
        float3 v = (boundsMax - boundsMin) * conversionFactor;
        cellCountAxis = FloorFloatToInt(v);
        cellCountXY = cellCountAxis.x * cellCountAxis.y;
        cellCountXYZ = cellCountAxis.x * cellCountAxis.y * cellCountAxis.z;
    }
}
