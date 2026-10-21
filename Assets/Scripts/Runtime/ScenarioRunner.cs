
using System.Collections.Generic;
using UnityEngine;

public class ScenarioRunner : MonoBehaviour
{
    [SerializeField] private ScenarioConfigSO _scenario;

    private readonly List<StepRuntime> _steps = new();

    private void Awake()
    {
        if (_scenario == null)
        {
            Debug.LogError("Scenario is not assigned!");
            enabled = false;
            return;
        }

        foreach (var stepConfig in _scenario.Steps)
        {
            if (stepConfig == null)
                continue;

            _steps.Add(new StepRuntime(stepConfig));
        }
    }

    private void OnEnable()
    {
        ScenarioEventBus.OnEvent += HandleEvent;
    }

    private void OnDisable()
    {
        ScenarioEventBus.OnEvent -= HandleEvent;
    }

    private void HandleEvent(ScenarioEvent scenarioEvent)
    {
        foreach (var step in _steps)
        {
            if (step.IsCompleted)
                continue;

            bool matched = step.ProcessEvent(scenarioEvent);

            if (!matched)
                continue;

            Debug.Log(
                $"Step {step.Config.StepId}: " +
                $"Condition {step.CurrentConditionIndex}/" +
                $"{step.Config.Conditions.Count}"
            );

            if (step.IsCompleted)
            {
                Debug.Log(
                    $"STEP COMPLETED: {step.Config.StepId}"
                );
            }
        }
    }
}
