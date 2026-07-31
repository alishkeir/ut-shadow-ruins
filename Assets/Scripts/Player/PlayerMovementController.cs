using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementController : MonoBehaviour
{
    [Header("Player Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 8f;

    [Header("Player Ground Check")]
    [SerializeField] private Transform groundCheckTransform;
    [SerializeField] private float groundCheckRadius = 0.1f;
    [SerializeField] private LayerMask groundCheckLayerMask;

    Rigidbody2D rb;
    InputSystem_Actions inputActions;

    float moveAmount;

    void Awake()
    {
        inputActions = new InputSystem_Actions();
        rb = GetComponent<Rigidbody2D>();
    }

    void OnEnable()
    {
        // enable the input actions
        inputActions.Player.Enable();

        // subscribe to the input actions
        inputActions.Player.Move.performed += Move;
        inputActions.Player.Jump.performed += Jump;

        // subscribe to the canceled events
        inputActions.Player.Jump.canceled += Jump;
        inputActions.Player.Move.canceled += Move;
    }

    void OnDisable()
    {
        // disable the input actions
        inputActions.Player.Disable();

        // unsubscribe from the input actions
        inputActions.Player.Move.performed -= Move;
        inputActions.Player.Jump.performed -= Jump;

        // unsubscribe from the canceled events
        inputActions.Player.Jump.canceled -= Jump;
        inputActions.Player.Move.canceled -= Move;
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveAmount * moveSpeed, rb.linearVelocityY);
    }


    void Move(InputAction.CallbackContext context)
    {
        moveAmount = context.ReadValue<Vector2>().x;
    }

    void Jump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            // don't jump if the player is not grounded... removed isGrounded to avoids a one-physics-frame delay 
            if (!Physics2D.OverlapCircle(groundCheckTransform.position, groundCheckRadius, groundCheckLayerMask)) return;

            // rb.linearVelocityY = jumpSpeed;
            rb.linearVelocityY = 0;
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }

        if (context.canceled)
        {
            if (rb.linearVelocityY > 0)
                rb.linearVelocityY *= 0.5f;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheckTransform.position, groundCheckRadius);
    }
}
