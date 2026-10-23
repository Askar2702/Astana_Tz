
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement2D : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 5f;

    private Rigidbody2D _rb;
    private Vector2 _moveInput;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        _rb.freezeRotation = true;
    }

    private void Update()
    {
        _moveInput = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        ).normalized;
    }

    private void FixedUpdate()
    {
        _rb.MovePosition(
            _rb.position + _moveInput * _moveSpeed * Time.fixedDeltaTime
        );
    }
}
