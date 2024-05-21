using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class SettingsUIRuntime : MonoBehaviour
{
    EntityQuery configQuery;

    [SerializeField]
    Slider sliderVisionRadius;
    [SerializeField]
    TMPro.TextMeshProUGUI textVisionRadius;

    [SerializeField]
    Slider sliderRepulsionDistance;
    [SerializeField]
    TMPro.TextMeshProUGUI textRepulsionDistance;

    [SerializeField]
    Slider sliderCohesionStrength;
    [SerializeField]
    TMPro.TextMeshProUGUI textCohesionStrength;

    [SerializeField]
    Slider sliderRepulsionStrength;
    [SerializeField]
    TMPro.TextMeshProUGUI textRepulsionStrength;

    [SerializeField]
    Slider sliderAlignmentStrength;
    [SerializeField]
    TMPro.TextMeshProUGUI textAlignmentStrength;

    [SerializeField]
    Slider sliderObjectiveStrength;
    [SerializeField]
    TMPro.TextMeshProUGUI textObjectiveStrength;

    private void Start()
    {
        configQuery = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(new EntityQueryDesc()
        {
            All = new ComponentType[] { ComponentType.ReadOnly<CBoidsConfig>() }
        });

        if (configQuery.TryGetSingleton<CBoidsConfig>(out var singleton))
        {
            // Do stuff
        }
        //q.GetSingletonEntity();
    }

    private void Initialize()
    {
        if (configQuery.TryGetSingleton<CBoidsConfig>(out var config))
        {
            sliderVisionRadius.minValue = 0;
            sliderVisionRadius.maxValue = 0.1f;
            sliderVisionRadius.value = config.behaviourData.Value.visionRange;
            textVisionRadius.text = sliderVisionRadius.value.ToString();

            sliderRepulsionDistance.minValue = 0;
            sliderRepulsionDistance.maxValue = 0.1f;
            sliderRepulsionDistance.value = config.behaviourData.Value.repulsionDistance;
            textRepulsionDistance.text = sliderRepulsionDistance.value.ToString();

            sliderRepulsionStrength.minValue = 0;
            sliderRepulsionStrength.maxValue = 3f;
            sliderRepulsionStrength.value = config.behaviourData.Value.repulsionStrength;
            textRepulsionStrength.text = sliderRepulsionStrength.value.ToString();

            sliderCohesionStrength.minValue = 0;
            sliderCohesionStrength.maxValue = 1f;
            sliderCohesionStrength.value = config.behaviourData.Value.cohesionStrength;
            textCohesionStrength.text = sliderCohesionStrength.value.ToString();

            sliderAlignmentStrength.minValue = 0;
            sliderAlignmentStrength.maxValue = 1f;
            sliderAlignmentStrength.value = config.behaviourData.Value.allignmentStrength;
            textAlignmentStrength.text = sliderAlignmentStrength.value.ToString();

            sliderObjectiveStrength.minValue = 0;
            sliderObjectiveStrength.maxValue = 1f;
            sliderObjectiveStrength.value = config.behaviourData.Value.objectiveStrength;
            textObjectiveStrength.text = sliderObjectiveStrength.value.ToString();
        }
    }

    public void ChangeVisionRadius()
    {
        if (configQuery.TryGetSingleton<CBoidsConfig>(out var config))
        {
            config.behaviourData.Value.visionRange = sliderVisionRadius.value;
        }
    }

    public void ChangeRepulsionDistance()
    {
        if (configQuery.TryGetSingleton<CBoidsConfig>(out var config))
        {
            config.behaviourData.Value.repulsionDistance = sliderRepulsionDistance.value;
        }
    }

    public void ChangeCohesionStrength()
    {
        if (configQuery.TryGetSingleton<CBoidsConfig>(out var config))
        {
            config.behaviourData.Value.cohesionStrength = sliderCohesionStrength.value;
        }
    }

    public void ChangeRepulsionStrength()
    {
        if (configQuery.TryGetSingleton<CBoidsConfig>(out var config))
        {
            config.behaviourData.Value.repulsionStrength = sliderRepulsionStrength.value;
        }
    }

    public void ChangeAlignmentStrength()
    {
        if (configQuery.TryGetSingleton<CBoidsConfig>(out var config))
        {
            config.behaviourData.Value.allignmentStrength = sliderAlignmentStrength.value;
        }
    }

    public void ChangeObjectiveStrength()
    {
        if (configQuery.TryGetSingleton<CBoidsConfig>(out var config))
        {
            config.behaviourData.Value.objectiveStrength = sliderObjectiveStrength.value;
        }
    }
}
