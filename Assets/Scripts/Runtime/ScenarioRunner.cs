
using System.Collections.Generic;
using UnityEngine;

public class ScenarioRunner : MonoBehaviour
{
    [SerializeField] private ScenarioConfigSO _scenario;
    [SerializeField] private ScenarioMode _mode;
    [SerializeField] private ScoreAnalyzerSO _scoreAnalyzer;
    [SerializeField] private ScenarioLoggerSO _logger;
    private readonly Dictionary<string, StepRuntime> _steps = new();
    private readonly ScenarioHistory _history = new();
    public IReadOnlyList<ScenarioLogEntry> History => _history.Entries;

    private IReadOnlyList<StepBatch> _batches;
    private int _currentBatchIndex;

    private int _currentGroupIndex;
    private int _groupViolations;
    private bool _isRunning;
    public ScenarioMode Mode => _mode;
    public bool IsRunning => _isRunning;

    public int TotalSteps
    {
        get
        {
            if (_mode == ScenarioMode.Training)
            {
                var ids = new HashSet<string>();

                foreach (var batch in _scenario.TrainingBatches)
                {
                    foreach (var step in batch.Steps)
                    {
                        if (step != null)
                            ids.Add(step.StepId);
                    }
                }

                return ids.Count;
            }

            return _steps.Count;
        }
    }

    public int CompletedSteps
    {
        get
        {
            int count = 0;

            foreach (var step in _steps.Values)
            {
                if (step.IsFinished)
                    count++;
            }

            return count;
        }
    }

    public string ActiveSteps
    {
        get
        {
            var ids = new List<string>();

            foreach (var step in _steps.Values)
            {
                if (step.IsActive && !step.IsFinished)
                    ids.Add(step.Config.StepId);
            }

            return ids.Count > 0
                ? string.Join(", ", ids)
                : "None";
        }
    }

    public int CurrentScore =>
        _scoreAnalyzer != null
            ? _scoreAnalyzer.CalculateScore(_history.Entries)
            : 0;

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
        _history.Clear();
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
        AddHistory(null, null, $"GroupActivated: {group.GroupName}");
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

            if (!string.IsNullOrEmpty(scenarioEvent.TargetStepId))
            {
                if (step.Config.StepId == scenarioEvent.TargetStepId)
                    relatedSteps.Add(step);

                continue;
            }

            foreach (var condition in step.Config.Conditions)
            {
                if (condition.EventId != scenarioEvent.EventId)
                    continue;

                relatedSteps.Add(step);
                break;
            }
        }


        if (relatedSteps.Count == 0 &&
            !string.IsNullOrEmpty(scenarioEvent.TargetStepId) &&
            _steps.TryGetValue(scenarioEvent.TargetStepId, out var finishedStep) &&
            finishedStep.IsFinished)
        {
            return;
        }


        if (relatedSteps.Count == 0)
        {
            Debug.LogWarning($"UNKNOWN EVENT: {scenarioEvent.EventId}");

            AddHistory(
                scenarioEvent.EventId,
                scenarioEvent.TargetStepId,
                "UnknownEvent"
            );

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
            {
                Debug.LogWarning($"OUT OF ORDER: {scenarioEvent.EventId}");

                AddHistory(
                    scenarioEvent.EventId,
                    scenarioEvent.TargetStepId,
                    "OutOfOrder"
                );
            }

            return;
        }


        // Не допускаем неоднозначной обработки.
        if (activeSteps.Count > 1)
        {
            Debug.LogWarning(
                $"AMBIGUOUS EVENT: {scenarioEvent.EventId}. " +
                "Specify TargetStepId."
            );

            AddHistory(
                scenarioEvent.EventId,
                scenarioEvent.TargetStepId,
                "AmbiguousEvent"
            );

            return;
        }

        var targetStep = activeSteps[0];
        var result = targetStep.ProcessEvent(scenarioEvent);


        AddHistory(
            scenarioEvent.EventId,
            targetStep.Config.StepId,
            result.ToString(),
            targetStep.Attempts,
            targetStep.Violations
        );

        Debug.Log(
            $"STEP: {targetStep.Config.StepId} | " +
            $"RESULT: {result} | " +
            $"ATTEMPTS: {targetStep.Attempts} | " +
            $"VIOLATIONS: {targetStep.Violations}"
        );

        if (result == StepEventResult.Violation &&
            _mode == ScenarioMode.Exam)
        {
            targetStep.Fail();
            _groupViolations++;

            Debug.LogWarning(
                $"STEP FAILED: {targetStep.Config.StepId} | " +
                $"GROUP VIOLATIONS: {_groupViolations}"
            );

            AddHistory(
                scenarioEvent.EventId,
                targetStep.Config.StepId,
                "Failed",
                targetStep.Attempts,
                targetStep.Violations
            );

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
                _history.Add(new ScenarioLogEntry(
                    null,
                    config.StepId,
                    "Skipped",
                    step.Attempts,
                    step.Violations));
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
        AddHistory(
            null,
            null,
            $"GroupCompleted: {_scenario.ExamGroups[_currentGroupIndex].GroupName}"
        );
        _currentGroupIndex++;
        ActivateExamGroup();
    }


    private void FinishScenario()
    {
        _isRunning = false;

        AddHistory(null, null, "ScenarioCompleted");

        Debug.Log("SCENARIO COMPLETED!");
        Debug.Log($"HISTORY ENTRIES: {_history.Entries.Count}");

        foreach (var entry in _history.Entries)
        {
            Debug.Log(
                $"HISTORY | EVENT: {entry.EventId} | " +
                $"STEP: {entry.StepId} | " +
                $"RESULT: {entry.Result}"
            );
        }

        int score = _scoreAnalyzer != null
            ? _scoreAnalyzer.CalculateScore(_history.Entries)
            : 0;

        Debug.Log($"FINAL SCORE: {score}");

        int recalculatedScore = _mode == ScenarioMode.Training
            ? ScenarioAnalyzer.CalculateTrainingScore(_history.Entries)
            : ScenarioAnalyzer.CalculateExamScore(_history.Entries);

        Debug.Log($"RECALCULATED SCORE: {recalculatedScore}");

    }


    private void AddHistory(string eventId, string stepId, string result, int attempts = 0, int violations = 0)
    {
        var entry = new ScenarioLogEntry(
                       eventId,
                       stepId,
                       result,
                       attempts,
                       violations
                   );

        _history.Add(entry);

        if (_logger != null)
            _logger.Log(entry);
    }


    public void RecalculateScore()
    {
        if (_isRunning)
        {
            Debug.LogWarning("Finish the scenario first!");
            return;
        }

        if (_history.Entries.Count == 0)
        {
            Debug.LogWarning("History is empty!");
            return;
        }

        if (_scoreAnalyzer == null)
        {
            Debug.LogWarning("Score analyzer is not assigned!");
            return;
        }

        int score = _scoreAnalyzer.CalculateScore(_history.Entries);
        Debug.Log($"MANUAL RECALCULATED SCORE: {score}");
    }


}
