
using UnityEngine;

public class ScenarioEventSource : MonoBehaviour
{
    [SerializeField] private string _eventId;

    public void RaiseEvent()
    {
        ScenarioEventBus.Publish(_eventId);
    }
}
