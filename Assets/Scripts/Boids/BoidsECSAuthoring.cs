using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class BoidsECSAuthoring : MonoBehaviour
{
    public class Baker : Baker<BoidsECSAuthoring>
    {
        public override void Bake(BoidsECSAuthoring authoring)
        {
            Entity entity = GetEntity(TransformUsageFlags.None);
        }
    }
}
