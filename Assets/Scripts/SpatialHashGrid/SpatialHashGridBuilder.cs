using Unity.Mathematics;
using Unity.Collections;
using Unity.Burst;
using UnityEngine.UIElements;

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

        UpdateBounds(in positions);
        SetCellCount();

        pivots = new NativeArray<int3>(cellCountXYZ, Allocator.TempJob);

        BuildContainers(in positions, ref pivots, ref cellIndices, ref hashTable);
    }

    [BurstCompile]
    private void BuildContainers(in NativeArray<CPosition> positions, ref NativeArray<int3> pivots, ref NativeArray<int> cellIndices, ref NativeArray<int> hashTable)
    {
        for (int boidIndex = 0; boidIndex < positions.Length; boidIndex++)
        {
            int cellIndex = HashFunction(positions[boidIndex].value);

            //if (cellIndex >= cellCountXYZ || cellIndex < 0)
            //{
            //    float3 convertedGridLength = (boundsMax - boundsMin) * conversionFactor;
            //    UnityEngine.Debug.Log("#####-----------:");
            //    UnityEngine.Debug.Log("boundsMin: " + boundsMin);
            //    UnityEngine.Debug.Log("boundsMax: " + boundsMax);
            //    UnityEngine.Debug.Log("position: " + positions[boidIndex].value);
            //    UnityEngine.Debug.Log("position: " + positions[boidIndex+1].value);
            //    UnityEngine.Debug.Log("convertedGridLength: " + convertedGridLength);
            //    UnityEngine.Debug.Log("cellCountAxis: " + cellCountAxis);
            //    UnityEngine.Debug.Log("cellIndex " + cellIndex + " is greater than cellCountXYZ: " + cellCountXYZ +
            //                          " in boidIndex: " + boidIndex + " with position: " + positions[boidIndex].value);
            //    return;
            //}
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

        NativeArray<int> hashBucketCount = new NativeArray<int>(pivots.Length,Allocator.Temp);
        for (int boidIndex = 0; boidIndex < positions.Length; boidIndex++)
        {
            int cellIndex = cellIndices[boidIndex];
            hashTable[pivots[cellIndex].y + hashBucketCount[cellIndex]] = boidIndex;
            hashBucketCount[cellIndex]++;
        }
    }

    [BurstCompile]
    private int HashFunction(float3 position)
    {
        float3 convertedPosition = (position - boundsMin) * conversionFactor;
        int3 cell = new int3((int)convertedPosition.x, 
                             (int)convertedPosition.y,
                             (int)convertedPosition.z);
        return cell.x + cell.y * cellCountAxis.x + cell.z * cellCountXY;
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

        cellCountAxis = new int3((int)convertedGridLength.x + 1, 
                                 (int)convertedGridLength.y + 1, 
                                 (int)convertedGridLength.z + 1);

        cellCountXY = cellCountAxis.x * cellCountAxis.y;
        cellCountXYZ = cellCountXY * cellCountAxis.z;
    }
}