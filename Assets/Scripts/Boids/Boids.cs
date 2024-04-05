using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Boids
{
    public class Boids : MonoBehaviour
    {
        //[SerializeField]
        //private Material material;
        //[SerializeField]
        //private Mesh mesh;
        [SerializeField]
        private GameObject boidPrefab;


        private Vector3[] boidPositions;


        //private void Start()
        //{
        //    SpawnShapeGenerator generator = new SpawnShapeGenerator();
        //    boidPositions = generator.GenerateBoidPositions(settings.GenerationSettings);

        //    foreach(var position in boidPositions)
        //    {
        //        Instantiate(boidPrefab, position, Quaternion.identity);
        //    }
        //}
    }
}

