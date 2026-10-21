
using UnityEngine;

public class ScenarioTriggerSource : MonoBehaviour
{
    [SerializeField] private string _eventId;
    [SerializeField] private string _requiredTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(_requiredTag))
            return;

        ScenarioEventBus.Publish(_eventId);
    }
}
