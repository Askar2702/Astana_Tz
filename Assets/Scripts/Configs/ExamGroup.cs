
using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ExamGroup
{
    [SerializeField] private string _groupName;
    [Min(1)]
    [SerializeField] private int _maxViolations = 2;
    [SerializeField] private List<StepBatch> _batches = new();

    public string GroupName => _groupName;
    public int MaxViolations => _maxViolations;
    public IReadOnlyList<StepBatch> Batches => _batches;
}
