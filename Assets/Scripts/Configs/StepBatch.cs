
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StepBatch
{
    [SerializeField] private string _batchName;
    [SerializeField] private List<ScenarioStepSO> _steps = new();

    public string BatchName => _batchName;
    public IReadOnlyList<ScenarioStepSO> Steps => _steps;
}
