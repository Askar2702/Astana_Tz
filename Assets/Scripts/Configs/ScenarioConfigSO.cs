
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "ScenarioConfig",
    menuName = "Training Scenario/Scenario")]
public class ScenarioConfigSO : ScriptableObject
{
    [SerializeField] private List<ScenarioStepSO> _steps = new();

    public IReadOnlyList<ScenarioStepSO> Steps => _steps;
}
