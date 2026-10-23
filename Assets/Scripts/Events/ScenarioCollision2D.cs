
using UnityEngine;

public class ScenarioCollision2D : MonoBehaviour
{
    [SerializeField] private string _eventId;
    [SerializeField] private string _targetStepId;
    [SerializeField] private string _requiredTag = "Player";

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag(_requiredTag))
            return;

        ScenarioEventBus.Publish(_eventId, _targetStepId);
    }
}
