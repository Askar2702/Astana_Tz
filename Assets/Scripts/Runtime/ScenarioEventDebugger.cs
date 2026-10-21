
using UnityEngine;

public class ScenarioEventDebugger : MonoBehaviour
{
    private void OnEnable()
    {
        ScenarioEventBus.OnEvent += HandleEvent;
    }

    private void OnDisable()
    {
        ScenarioEventBus.OnEvent -= HandleEvent;
    }

    private void HandleEvent(ScenarioEvent scenarioEvent)
    {
        Debug.Log(
            $"Event: {scenarioEvent.EventId} | " +
            $"Time: {scenarioEvent.Timestamp:HH:mm:ss}"
        );
    }
}
