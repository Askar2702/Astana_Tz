
using System.Collections.Generic;
using UnityEngine;

public class ScenarioRunner : MonoBehaviour
{
    [SerializeField] private ScenarioConfigSO _scenario;
    [SerializeField] private ScenarioMode _mode;

    private readonly Dictionary<string, StepRuntime> _steps = new();

    private IReadOnlyList<StepBatch> _batches;
    private int _currentBatchIndex;

    private int _currentGroupIndex;
    private int _groupViolations;
    private bool _isRunning;

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

        _currentGroupIndex = 0;
        _currentBatchIndex = 0;
        _groupViolations = 0;
        _isRunning = true;

        if (_mode == ScenarioMode.Training)
        {
            _batches = _scenario.TrainingBatches;
            ActivateCurrentBatch();
        }
        else
        {
            ActivateExamGroup();
        }
    }

    private void ActivateExamGroup()
    {
        if (_currentGroupIndex >= _scenario.ExamGroups.Count)
        {
            FinishScenario();
            return;
        }

        var group = _scenario.ExamGroups[_currentGroupIndex];

        _groupViolations = 0;
        _currentBatchIndex = 0;
        _batches = group.Batches;

        Debug.Log($"EXAM GROUP ACTIVATED: {group.GroupName}");

        ActivateCurrentBatch();
    }

    private void ActivateCurrentBatch()
    {
        if (!_isRunning)
            return;

        if (_batches == null || _currentBatchIndex >= _batches.Count)
        {
            CompleteCurrentGroup();
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
        if (!_isRunning)
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
            Debug.LogWarning($"UNKNOWN EVENT: {scenarioEvent.EventId}");
            return;
        }

        var activeSteps = new List<StepRuntime>();

        foreach (var step in relatedSteps)
        {
            if (step.IsActive)
                activeSteps.Add(step);
        }

        if (activeSteps.Count == 0)
        {
            if (!_scenario.IgnoreInactiveSteps)
                Debug.LogWarning($"OUT OF ORDER: {scenarioEvent.EventId}");

            return;
        }

        foreach (var step in activeSteps)
        {
            var result = step.ProcessEvent(scenarioEvent);

            Debug.Log(
                $"STEP: {step.Config.StepId} | " +
                $"RESULT: {result} | " +
                $"ATTEMPTS: {step.Attempts} | " +
                $"VIOLATIONS: {step.Violations}"
            );

            if (result != StepEventResult.Violation)
                continue;

            if (_mode == ScenarioMode.Exam)
            {
                step.Fail();
                _groupViolations++;

                Debug.LogWarning(
                    $"STEP FAILED: {step.Config.StepId} | " +
                    $"GROUP VIOLATIONS: {_groupViolations}"
                );
            }
        }

        if (_mode == ScenarioMode.Exam)
        {
            var group = _scenario.ExamGroups[_currentGroupIndex];

            if (_groupViolations >= group.MaxViolations)
            {
                SkipRemainingGroupSteps(group);
                _currentGroupIndex++;
                ActivateExamGroup();
                return;
            }
        }

        CheckBatchCompletion();
    }

    private void SkipRemainingGroupSteps(ExamGroup group)
    {
        foreach (var batch in group.Batches)
        {
            foreach (var config in batch.Steps)
            {
                if (config == null ||
                    !_steps.TryGetValue(config.StepId, out var step) ||
                    step.IsFinished)
                    continue;

                step.Skip();
                Debug.Log($"STEP SKIPPED: {config.StepId}");
            }
        }
    }

    private void CheckBatchCompletion()
    {
        if (_batches == null || _currentBatchIndex >= _batches.Count)
            return;

        var batch = _batches[_currentBatchIndex];

        foreach (var config in batch.Steps)
        {
            if (config == null ||
                !_steps.TryGetValue(config.StepId, out var step) ||
                !step.IsFinished)
                return;
        }

        Debug.Log($"BATCH COMPLETED: {batch.BatchName}");

        _currentBatchIndex++;
        ActivateCurrentBatch();
    }

    private void CompleteCurrentGroup()
    {
        if (_mode == ScenarioMode.Training)
        {
            FinishScenario();
            return;
        }

        Debug.Log(
            $"EXAM GROUP COMPLETED: " +
            $"{_scenario.ExamGroups[_currentGroupIndex].GroupName}"
        );

        _currentGroupIndex++;
        ActivateExamGroup();
    }

    private void FinishScenario()
    {
        _isRunning = false;
        Debug.Log("SCENARIO COMPLETED!");
    }
}
