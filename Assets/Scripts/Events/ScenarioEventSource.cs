
using UnityEngine;

public class ScenarioEventSource : MonoBehaviour
{
    [SerializeField] private string _eventId;
    [SerializeField] private string _targetStepId;


    public void RaiseEvent()
    {
        ScenarioEventBus.Publish(_eventId, _targetStepId);
    }
}
