using System.Collections.Generic;
using UnityEngine;

public class ScenarioRunner : MonoBehaviour
{
    [SerializeField] private ScenarioConfigSO _scenario;
    [SerializeField] private ScenarioMode _mode;

    private readonly Dictionary<string, StepRuntime> _steps = new();

    private IReadOnlyList<StepBatch> _batches;
    private int _currentBatchIndex;

    private void Awake()
    {
        if (_scenario == null)
        {
            Debug.LogError("Scenario is not assigned!");
            enabled = false;
            return;
        }

        foreach (var config in _scenario.Steps)
        {
            if (config == null || string.IsNullOrWhiteSpace(config.StepId))
                continue;

            if (_steps.ContainsKey(config.StepId))
            {
                Debug.LogError($"Duplicate step ID: {config.StepId}");
                continue;
            }

            _steps.Add(config.StepId, new StepRuntime(config));
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

    private void Start()
    {
        StartScenario();
    }

    public void StartScenario()
    {
        foreach (var step in _steps.Values)
            step.Reset();

        _batches = _mode == ScenarioMode.Training
            ? _scenario.TrainingBatches
            : _scenario.ExamBatches;

        _currentBatchIndex = 0;

        ActivateCurrentBatch();
    }

    private void ActivateCurrentBatch()
    {
        if (_currentBatchIndex >= _batches.Count)
        {
            Debug.Log("SCENARIO COMPLETED!");
            return;
        }

        var batch = _batches[_currentBatchIndex];

        Debug.Log($"BATCH ACTIVATED: {batch.BatchName}");

        foreach (var config in batch.Steps)
        {
            if (config == null)
                continue;

            if (_steps.TryGetValue(config.StepId, out var step))
            {
                step.Activate();
                Debug.Log($"STEP ACTIVATED: {config.StepId}");
            }
        }
    }


    private void HandleEvent(ScenarioEvent scenarioEvent)
    {
        if (_batches == null || _currentBatchIndex >= _batches.Count)
            return;

        var relatedSteps = new List<StepRuntime>();

        foreach (var step in _steps.Values)
        {
            if (step.IsFinished)
                continue;

            foreach (var condition in step.Config.Conditions)
            {
                if (condition.EventId != scenarioEvent.EventId)
                    continue;

                relatedSteps.Add(step);
                break;
            }
        }

        if (relatedSteps.Count == 0)
        {
            Debug.LogWarning(
                $"UNKNOWN EVENT: {scenarioEvent.EventId}"
            );
            return;
        }

        foreach (var step in relatedSteps)
        {
            if (!step.IsActive)
            {
                if (!_scenario.IgnoreInactiveSteps)
                {
                    Debug.LogWarning(
                        $"OUT OF ORDER: {step.Config.StepId}"
                    );
                }

                continue;
            }

            var result = step.ProcessEvent(scenarioEvent);

            Debug.Log(
                $"STEP: {step.Config.StepId} | " +
                $"RESULT: {result} | " +
                $"ATTEMPTS: {step.Attempts} | " +
                $"VIOLATIONS: {step.Violations}"
            );
        }

        CheckBatchCompletion();
    }


    private void CheckBatchCompletion()
    {
        var batch = _batches[_currentBatchIndex];

        foreach (var config in batch.Steps)
        {
            if (config == null ||
                !_steps.TryGetValue(config.StepId, out var step) ||
                !step.IsFinished)
            {
                return;
            }
        }

        Debug.Log($"BATCH COMPLETED: {batch.BatchName}");

        _currentBatchIndex++;
        ActivateCurrentBatch();
    }
}
