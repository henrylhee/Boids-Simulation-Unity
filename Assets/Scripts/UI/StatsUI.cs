using Boids;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatsUI : MonoBehaviour
{
    [SerializeField]
    private TMPro.TextMeshProUGUI fps;
    [SerializeField]
    private TMPro.TextMeshProUGUI cores;
    [SerializeField]
    private TMPro.TextMeshProUGUI boidCount;
    [SerializeField]
    private BoidConfigSO boidConfig;

    [SerializeField]
    private int updateFpsPerSecond = 5;
    private bool updateFps = true;

    private FPSCounter fpsCounter;



    private void Start()
    {
        fpsCounter = GetComponent<FPSCounter>();

        cores.text = "Cores in use: " + SystemInfo.processorCount;

        boidCount.text = "Boidcount: " + boidConfig.spawnConfigSO.Value.Value.boidCount;
    }

    private void Update()
    {
        if(updateFps)
        {
            StartCoroutine(UpdateFps());
        }
    }

    private IEnumerator UpdateFps()
    {
        fps.text = "FPS: " + fpsCounter.GetCurrentFps();
        updateFps = false;
        yield return new WaitForSeconds(1f/updateFpsPerSecond);
        updateFps = true;
    }
}
