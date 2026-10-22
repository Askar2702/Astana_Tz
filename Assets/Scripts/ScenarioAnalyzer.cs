
using System.Collections.Generic;
using UnityEngine;

public static class ScenarioAnalyzer
{
    public static int CalculateTrainingScore(
        IReadOnlyList<ScenarioLogEntry> history)
    {
        var attempts = new Dictionary<string, int>();

        foreach (var entry in history)
        {
            if (string.IsNullOrEmpty(entry.StepId))
                continue;

            if (entry.Result == "Completed")
            {
                attempts[entry.StepId] = entry.Attempts;
            }
        }

        int score = 0;

        foreach (var attempt in attempts.Values)
        {
            if (attempt == 1)
                score += 10;
            else if (attempt == 2)
                score += 5;
        }

        return score;
    }

    public static int CalculateExamScore(
        IReadOnlyList<ScenarioLogEntry> history)
    {
        int penalties = 0;

        foreach (var entry in history)
        {
            if (entry.Result == "Failed")
                penalties += 10;
            else if (entry.Result == "Skipped")
                penalties += 5;
        }

        return Mathf.Max(0, 100 - penalties);
    }
}
