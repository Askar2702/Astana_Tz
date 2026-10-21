
using System;

public readonly struct ScenarioEvent
{
    public readonly string EventId;
    public readonly DateTime Timestamp;

    public ScenarioEvent(string eventId)
    {
        EventId = eventId;
        Timestamp = DateTime.UtcNow;
    }
}
