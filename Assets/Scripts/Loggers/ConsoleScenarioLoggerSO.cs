
using UnityEngine;

[CreateAssetMenu(
    fileName = "ConsoleScenarioLogger",
    menuName = "Scenario/Loggers/Console Logger")]
public class ConsoleScenarioLoggerSO : ScenarioLoggerSO
{
    public override void Log(ScenarioLogEntry entry)
    {
        if (entry == null)
            return;

        Debug.Log(
            $"LOGGER | TIME: {entry.Timestamp:HH:mm:ss} | " +
            $"EVENT: {entry.EventId} | " +
            $"STEP: {entry.StepId} | " +
            $"RESULT: {entry.Result} | " +
            $"ATTEMPTS: {entry.Attempts} | " +
            $"VIOLATIONS: {entry.Violations}"
        );
    }
}
