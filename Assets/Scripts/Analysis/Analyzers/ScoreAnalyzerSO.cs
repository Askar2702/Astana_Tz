
using System.Collections.Generic;
using UnityEngine;

public abstract class ScoreAnalyzerSO : ScriptableObject
{
    public abstract int CalculateScore(
        IReadOnlyList<ScenarioLogEntry> history);
}
