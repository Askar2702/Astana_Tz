
using UnityEngine;

public class ScenarioCollisionSource : MonoBehaviour
{
    [SerializeField] private string _eventId;
    [SerializeField] private string _requiredTag = "Player";

    private void OnCollisionEnter(Collision collision)
    {
        if (!collision.gameObject.CompareTag(_requiredTag))
            return;

        ScenarioEventBus.Publish(_eventId);
    }
}
