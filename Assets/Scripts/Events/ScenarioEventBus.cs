using System;

public static class ScenarioEventBus
{
    public static event Action<ScenarioEvent> OnEvent;

    public static void Publish(
        string eventId,
        string targetStepId = null)
    {
        if (string.IsNullOrWhiteSpace(eventId))
            return;

        var scenarioEvent = new ScenarioEvent(
            eventId,
            targetStepId: targetStepId
        );

        OnEvent?.Invoke(scenarioEvent);
    }
}
