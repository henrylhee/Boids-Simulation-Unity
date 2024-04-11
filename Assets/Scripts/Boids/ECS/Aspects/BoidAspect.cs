using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

public readonly partial struct BoidAspect : IAspect
{
    public readonly RefRW<LocalTransform> localTransform;
    public readonly RefRW<CSpeed> speed;
}
