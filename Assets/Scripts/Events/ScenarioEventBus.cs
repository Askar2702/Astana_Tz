
using System;

public static class ScenarioEventBus
{
    public static event Action<ScenarioEvent> OnEvent;

    public static void Publish(string eventId)
    {
        if (string.IsNullOrWhiteSpace(eventId))
            return;

        var scenarioEvent = new ScenarioEvent(eventId);

        OnEvent?.Invoke(scenarioEvent);
    }
}
