using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController2D : MonoBehaviour
{
    [Header("Player Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpForce = 12f;

    [Header("Player Ground Check")]
    [SerializeField] private Transform groundCheckTransform;
    [SerializeField] private float groundCheckRadius = 0.1f;
    [SerializeField] private LayerMask groundCheckLayerMask;

    [Header("Coyote Time")]
    [Tooltip("The amount of time the player can still jump after leaving the ground.")]
    [SerializeField] private float coyoteTime = 0.2f;

    Rigidbody2D rb;
    InputSystem_Actions inputActions;

    float coyoteTimeCounter;
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
        // move the player with the current move amount we get from the input system
        rb.linearVelocity = new Vector2(moveAmount * moveSpeed, rb.linearVelocityY);

        // coyote time
        // if the player is on the ground, reset the coyote time counter
        if (Physics2D.OverlapCircle(groundCheckTransform.position, groundCheckRadius, groundCheckLayerMask))
        {
            coyoteTimeCounter = coyoteTime;
        }
        // otherwise, decrement the coyote time counter
        else
        {
            coyoteTimeCounter -= Time.fixedDeltaTime;
        }
    }


    void Move(InputAction.CallbackContext context)
    {
        // read the move (X only) amount from the input system
        moveAmount = context.ReadValue<Vector2>().x;
    }

    void Jump(InputAction.CallbackContext context)
    {
        // if the player performed the jump action
        if (context.performed)
        {
            // allows the playerto jump after leaving the ground for a short period of time
            if (coyoteTimeCounter <= 0) return;

            coyoteTimeCounter = 0;

            rb.linearVelocityY = 0;

            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
        // if the player canceled the jump action // released the jump button
        if (context.canceled)
        {
            if (rb.linearVelocityY > 0)
            {
                rb.linearVelocityY *= 0.5f;
            }
        }
    }
    // draw gizmos for the ground check // visible only in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheckTransform.position, groundCheckRadius);
    }
}
