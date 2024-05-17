using Boids;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Config : ScriptableObject
{
    [SerializeField]
    private BoidConfigSO boidConfig;

    [SerializeField]
    private BoidEnemyConfigSO enemyConfig;
}
