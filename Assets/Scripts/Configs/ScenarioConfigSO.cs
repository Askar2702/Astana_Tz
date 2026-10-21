using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "ScenarioConfig",
    menuName = "Training Scenario/Scenario")]
public class ScenarioConfigSO : ScriptableObject
{
    [Header("Reusable Steps")]
    [SerializeField] private List<ScenarioStepSO> _steps = new();

    [Header("Training Activation")]
    [SerializeField] private List<StepBatch> _trainingBatches = new();

    [Header("Exam Activation")]
    [SerializeField] private List<StepBatch> _examBatches = new();

    [Header("Event Handling")]
    [SerializeField] private bool _ignoreInactiveSteps;

    public bool IgnoreInactiveSteps => _ignoreInactiveSteps;

    public IReadOnlyList<ScenarioStepSO> Steps => _steps;
    public IReadOnlyList<StepBatch> TrainingBatches => _trainingBatches;
    public IReadOnlyList<StepBatch> ExamBatches => _examBatches;
}
