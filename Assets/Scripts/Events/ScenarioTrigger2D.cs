
using UnityEngine;

public class ScenarioTrigger2D : MonoBehaviour
{
    [SerializeField] private string _eventId;
    [SerializeField] private string _targetStepId;
    [SerializeField] private string _requiredTag = "Player";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(_requiredTag))
            return;

        ScenarioEventBus.Publish(_eventId, _targetStepId);
    }
}
