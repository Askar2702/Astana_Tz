
using System;

[Serializable]
public class ScenarioLogEntry
{
    public DateTime Timestamp { get; }
    public string EventId { get; }
    public string StepId { get; }
    public string Result { get; }
    public int Attempts { get; }
    public int Violations { get; }

    public ScenarioLogEntry(
        string eventId,
        string stepId,
        string result,
        int attempts,
        int violations)
    {
        Timestamp = DateTime.UtcNow;
        EventId = eventId;
        StepId = stepId;
        Result = result;
        Attempts = attempts;
        Violations = violations;
    }
}
