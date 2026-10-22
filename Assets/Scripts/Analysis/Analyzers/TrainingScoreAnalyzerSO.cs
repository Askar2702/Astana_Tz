
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "TrainingScoreAnalyzer",
    menuName = "Scenario/Analyzers/Training Score")]
public class TrainingScoreAnalyzerSO : ScoreAnalyzerSO
{
    public override int CalculateScore(
        IReadOnlyList<ScenarioLogEntry> history)
    {
        return ScenarioAnalyzer.CalculateTrainingScore(history);
    }
}
