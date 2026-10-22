
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(
    fileName = "ExamScoreAnalyzer",
    menuName = "Scenario/Analyzers/Exam Score")]
public class ExamScoreAnalyzerSO : ScoreAnalyzerSO
{
    public override int CalculateScore(
        IReadOnlyList<ScenarioLogEntry> history)
    {
        return ScenarioAnalyzer.CalculateExamScore(history);
    }
}
