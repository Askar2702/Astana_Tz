
using UnityEngine;

[CreateAssetMenu(
    fileName = "ScenarioStep",
    menuName = "Training Scenario/Step")]
public class ScenarioStepSO : ScriptableObject
{
    [Header("Step Information")]
    [SerializeField] private string _stepId;
    [SerializeField] private string _description;

    public string StepId => _stepId;
    public string Description => _description;
}
