
using System;

public readonly struct ScenarioEvent
{
    public readonly string EventId;
    public readonly DateTime Timestamp;
    public string TargetStepId { get; }


    public ScenarioEvent(string eventId , string targetStepId = null)
    {
        EventId = eventId;
        Timestamp = DateTime.UtcNow;
        TargetStepId = targetStepId;
    }
}
