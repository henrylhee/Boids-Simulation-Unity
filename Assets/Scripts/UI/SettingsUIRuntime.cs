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
            All = new ComponentType[] { ComponentType.ReadWrite<CBoidsConfig>() }
        });

        Initialize();
    }

    private void Initialize()
    {
        if (configQuery.TryGetSingletonRW<CBoidsConfig>(out var config))
        {
            sliderVisionRadius.minValue = 0;
            sliderVisionRadius.maxValue = 0.1f;
            sliderVisionRadius.value = config.ValueRW.behaviourData.Value.visionRange;
            textVisionRadius.text = sliderVisionRadius.value.ToString();

            sliderRepulsionDistance.minValue = 0;
            sliderRepulsionDistance.maxValue = 0.1f;
            sliderRepulsionDistance.value = config.ValueRW.behaviourData.Value.repulsionDistance;
            textRepulsionDistance.text = sliderRepulsionDistance.value.ToString();

            sliderRepulsionStrength.minValue = 0;
            sliderRepulsionStrength.maxValue = 3f;
            sliderRepulsionStrength.value = config.ValueRW.behaviourData.Value.repulsionStrength;
            textRepulsionStrength.text = sliderRepulsionStrength.value.ToString();

            sliderCohesionStrength.minValue = 0;
            sliderCohesionStrength.maxValue = 1f;
            sliderCohesionStrength.value = config.ValueRW.behaviourData.Value.cohesionStrength;
            textCohesionStrength.text = sliderCohesionStrength.value.ToString();

            sliderAlignmentStrength.minValue = 0;
            sliderAlignmentStrength.maxValue = 1f;
            sliderAlignmentStrength.value = config.ValueRW.behaviourData.Value.allignmentStrength;
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
            config.ValueRW.behaviourData.Value.visionRange = sliderVisionRadius.value;
            textVisionRadius.text = sliderVisionRadius.value.ToString();
        }
    }

    public void ChangeRepulsionDistance()
    {
        if (configQuery.TryGetSingletonRW<CBoidsConfig>(out var config))
        {
            config.ValueRW.behaviourData.Value.repulsionDistance = sliderRepulsionDistance.value;
            textRepulsionDistance.text = sliderRepulsionDistance.value.ToString();
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

    public void ChangeRepulsionStrength()
    {
        if (configQuery.TryGetSingletonRW<CBoidsConfig>(out var config))
        {
            config.ValueRW.behaviourData.Value.repulsionStrength = sliderRepulsionStrength.value;
            textRepulsionStrength.text = sliderRepulsionStrength.value.ToString();
        }
    }

    public void ChangeAlignmentStrength()
    {
        if (configQuery.TryGetSingletonRW<CBoidsConfig>(out var config))
        {
            config.ValueRW.behaviourData.Value.allignmentStrength = sliderAlignmentStrength.value;
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
