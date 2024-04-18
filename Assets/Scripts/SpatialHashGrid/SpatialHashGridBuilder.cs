using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst;

[BurstCompile]
public struct SpatialHashGridBuilder
{
    [BurstCompile]
    public static void Build(in NativeArray<CPosition> positions, CBoidsConfig config, out NativeArray<int3> pivots, ref NativeArray<int> cellIndices, ref NativeArray<int> hashTable)
    {
        SpatialHashGridData data = new SpatialHashGridData()
        {
            cellSize = config.behaviourData.CohesionDistance,
            conversionFactor = 1f / config.behaviourData.CohesionDistance,
            positions = positions.Reinterpret<float3>().ToArray(),
        };

        UpdateBounds(ref data);
        SetCellCount(ref data);
        BuildContainers(ref data, ref cellIndices, ref hashTable);
        pivots = new NativeArray<int3>(data.pivotsTemp, Allocator.Temp);
    }

    [BurstCompile]
    private static void BuildContainers(ref SpatialHashGridData data, ref NativeArray<int> cellIndices, ref NativeArray<int> hashTable)
    {
        for (int boidIndex = 0; boidIndex < data.positions.Length; boidIndex++)
        {
            int cellIndex = HashFunction(boidIndex, ref data);
            cellIndices[boidIndex] = cellIndex;
            if(cellIndex >= data.cellCountXYZ)
            {
                float3 convertedGridLength = (data.boundsMax - data.boundsMin) * data.conversionFactor;
                //UnityEngine.Debug.Log("#####");
                //UnityEngine.Debug.Log("convertedGridLength: " + convertedGridLength);
                //UnityEngine.Debug.Log("cellCountAxis: " + cellCountAxis);
                //UnityEngine.Debug.Log("boundsMin: " + boundsMin);
                //UnityEngine.Debug.Log("boundsMax: " + boundsMax);
                //UnityEngine.Debug.Log("cellIndex " + cellIndex + " is greater than cellCountXYZ: " + cellCountXYZ + 
                //                      " in boidIndex: " + boidIndex + " with position: " + positions[boidIndex]);
                return;
            }

            data.pivotsTemp[cellIndex].x++;
        }

        int accum = 0;
        for (int cellIndex = 0; cellIndex < data.cellCountXYZ; cellIndex++)
        {
            int boidsCountCell = data.pivotsTemp[cellIndex].x;
            if (boidsCountCell != 0)
            {
                data.pivotsTemp[cellIndex].y = accum;
                accum += boidsCountCell;
                data.pivotsTemp[cellIndex].z = accum;
            }
        }

        int[] hashBucketCount = new int[data.pivotsTemp.Length];
        for (int boidIndex = 0; boidIndex < data.positions.Length; boidIndex++)
        {
            int cellIndex = cellIndices[boidIndex];
            hashTable[data.pivotsTemp[cellIndex].y + hashBucketCount[cellIndex]] = boidIndex;
            hashBucketCount[cellIndex]++;
        }
    }

    [BurstCompile]
    private static int HashFunction(int i, ref SpatialHashGridData data)
    {
        float3 convertedPosition = (data.positions[i] - data.boundsMin) * data.conversionFactor;
        int3 cell = new int3((int)math.ceil(convertedPosition.x), 
                             (int)math.ceil(convertedPosition.y),
                             (int)math.ceil(convertedPosition.z)) - 1;
        return math.clamp(cell.x, 0, cell.x) + 
               math.clamp(cell.y * data.cellCountAxis.x, 0, cell.y * data.cellCountAxis.x) + 
               math.clamp(cell.z * data.cellCountXY, 0, cell.z * data.cellCountXY);
    }

    [BurstCompile]
    private static void UpdateBounds(ref SpatialHashGridData data)
    {
        float3 newBoundsMin = float3.zero;
        float3 newBoundsMax = float3.zero;

        for (int i = 0;  i < data.positions.Length; i++)
        {
            float3 position = data.positions[i];

            if(position.x > newBoundsMax.x) { newBoundsMax.x = position.x; }
            if(position.x < newBoundsMin.x) { newBoundsMin.x = position.x; }

            if (position.y > newBoundsMax.y) { newBoundsMax.y = position.y; }
            if (position.y < newBoundsMin.y) { newBoundsMin.y = position.y; }

            if (position.z > newBoundsMax.z) { newBoundsMax.z = position.z; }
            if (position.z < newBoundsMin.z) { newBoundsMin.z = position.z; }
        }

        data.boundsMin = newBoundsMin;
        data.boundsMax = newBoundsMax;
    }

    [BurstCompile]
    private static void SetCellCount(ref SpatialHashGridData data)
    {
        float3 convertedGridLength = (data.boundsMax - data.boundsMin) * data.conversionFactor;

        data.cellCountAxis = new int3((int)math.ceil(convertedGridLength).x, 
                                 (int)math.ceil(convertedGridLength).y, 
                                 (int)math.ceil(convertedGridLength).z);

        data.cellCountXY = data.cellCountAxis.x * data.cellCountAxis.y;
        data.cellCountXYZ = data.cellCountXY * data.cellCountAxis.z;
        data.pivotsTemp = new int3[data.cellCountXYZ];
    }

    struct SpatialHashGridData
    {
        public float cellSize;
        public float3 boundsMin;
        public float3 boundsMax;

        public int3 cellCountAxis;
        public int cellCountXY;
        public int cellCountXYZ;

        public float conversionFactor;
        public int voxelSearchFactor;
        public int3 voxelSearchCount;

        public float3[] positions;

        public int3[] pivotsTemp;
    }
}
