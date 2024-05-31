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
    Slider sliderGlobalCohesionStrength;
    [SerializeField]
    TMPro.TextMeshProUGUI textGlobalCohesionStrength;

    [SerializeField]
    Slider sliderCohesionStrength;
    [SerializeField]
    TMPro.TextMeshProUGUI textCohesionStrength;

    [SerializeField]
    Slider sliderSeparationStrength;
    [SerializeField]
    TMPro.TextMeshProUGUI textSeparationStrength;

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
            All = new ComponentType[] { ComponentType.ReadWrite<CBoidsConfig>() }
        });

        Initialize();
    }

    private void Initialize()
    {
        if (configQuery.TryGetSingletonRW<CBoidsConfig>(out var config))
        {
            sliderVisionRadius.minValue = 0;
            sliderVisionRadius.maxValue = 1f;
            sliderVisionRadius.value = config.ValueRW.behaviourData.Value.visionRadius;
            textVisionRadius.text = sliderVisionRadius.value.ToString();

            sliderGlobalCohesionStrength.minValue = 0;
            sliderGlobalCohesionStrength.maxValue = 1f;
            sliderGlobalCohesionStrength.value = config.ValueRW.behaviourData.Value.globalCohesionStrength;
            textGlobalCohesionStrength.text = sliderGlobalCohesionStrength.value.ToString();

            sliderSeparationStrength.minValue = 0;
            sliderSeparationStrength.maxValue = 3f;
            sliderSeparationStrength.value = config.ValueRW.behaviourData.Value.separationStrength;
            textSeparationStrength.text = sliderSeparationStrength.value.ToString();

            sliderCohesionStrength.minValue = 0;
            sliderCohesionStrength.maxValue = 1f;
            sliderCohesionStrength.value = config.ValueRW.behaviourData.Value.cohesionStrength;
            textCohesionStrength.text = sliderCohesionStrength.value.ToString();

            sliderAlignmentStrength.minValue = 0;
            sliderAlignmentStrength.maxValue = 1f;
            sliderAlignmentStrength.value = config.ValueRW.behaviourData.Value.alignmentStrength;
            textAlignmentStrength.text = sliderAlignmentStrength.value.ToString();

            sliderObjectiveStrength.minValue = 0;
            sliderObjectiveStrength.maxValue = 1f;
            sliderObjectiveStrength.value = config.ValueRW.behaviourData.Value.objectiveStrength;
            textObjectiveStrength.text = sliderObjectiveStrength.value.ToString();
        }
    }

    public void ChangeVisionRadius()
    {
        if (configQuery.TryGetSingletonRW<CBoidsConfig>(out var config))
        {
            config.ValueRW.behaviourData.Value.visionRadius = sliderVisionRadius.value;
            textVisionRadius.text = sliderVisionRadius.value.ToString();
        }
    }

    public void ChangeGlobalCohesionStrength()
    {
        if (configQuery.TryGetSingletonRW<CBoidsConfig>(out var config))
        {
            config.ValueRW.behaviourData.Value.globalCohesionStrength = sliderGlobalCohesionStrength.value;
            textGlobalCohesionStrength.text = sliderGlobalCohesionStrength.value.ToString();
        }
    }

    public void ChangeCohesionStrength()
    {
        if (configQuery.TryGetSingletonRW<CBoidsConfig>(out var config))
        {
            config.ValueRW.behaviourData.Value.cohesionStrength = sliderCohesionStrength.value;
            textCohesionStrength.text = sliderCohesionStrength.value.ToString();
        }
    }

    public void ChangeSeparationStrength()
    {
        if (configQuery.TryGetSingletonRW<CBoidsConfig>(out var config))
        {
            config.ValueRW.behaviourData.Value.separationStrength = sliderSeparationStrength.value;
            textSeparationStrength.text = sliderSeparationStrength.value.ToString();
        }
    }

    public void ChangeAlignmentStrength()
    {
        if (configQuery.TryGetSingletonRW<CBoidsConfig>(out var config))
        {
            config.ValueRW.behaviourData.Value.alignmentStrength = sliderAlignmentStrength.value;
            textAlignmentStrength.text = sliderAlignmentStrength.value.ToString();
        }
    }

    public void ChangeObjectiveStrength()
    {
        if (configQuery.TryGetSingletonRW<CBoidsConfig>(out var config))
        {
            config.ValueRW.behaviourData.Value.objectiveStrength = sliderObjectiveStrength.value;
            textObjectiveStrength.text = sliderObjectiveStrength.value.ToString();
        }
    }
}
