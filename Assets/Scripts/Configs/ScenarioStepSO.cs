
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "ScenarioStep",
    menuName = "Training Scenario/Step")]
public class ScenarioStepSO : ScriptableObject
{
    [Header("Step Information")]
    [SerializeField] private string _stepId;
    [SerializeField] private string _description;

    [Header("Completion Conditions")]
    [SerializeField] private List<StepCondition> _conditions = new();

    public string StepId => _stepId;
    public string Description => _description;
    public IReadOnlyList<StepCondition> Conditions => _conditions;
}
