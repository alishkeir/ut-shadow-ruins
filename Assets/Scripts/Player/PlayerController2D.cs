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


    [Header("Animations")]
    [SerializeField] private float rollSpeed = 8f;
    [SerializeField] private float dashSpeed = 12f;

    Rigidbody2D rb;
    PlayerInputActions playerInputActions;
    Animator animator;
    SpriteRenderer spriteRenderer;

    float coyoteTimeCounter;
    float moveAmount;
    bool wasGrounded;
    bool isRolling;
    bool isDashing;

    private static readonly int speedHash = Animator.StringToHash("Speed");
    private static readonly int velocityYHash = Animator.StringToHash("VelocityY");
    private static readonly int groundedHash = Animator.StringToHash("Grounded");
    private static readonly int landHash = Animator.StringToHash("Land");
    private static readonly int rollHash = Animator.StringToHash("Roll");
    private static readonly int dashHash = Animator.StringToHash("Dash");


    void Awake()
    {
        playerInputActions = new PlayerInputActions();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnEnable()
    {
        // enable the input actions
        playerInputActions.Player.Enable();

        // subscribe to the input actions
        playerInputActions.Player.Move.performed += Move;
        playerInputActions.Player.Jump.performed += Jump;
        playerInputActions.Player.Roll.performed += Roll;
        playerInputActions.Player.Dash.performed += Dash;

        // subscribe to the canceled events
        playerInputActions.Player.Jump.canceled += Jump;
        playerInputActions.Player.Move.canceled += Move;
        playerInputActions.Player.Roll.canceled += Roll;
        playerInputActions.Player.Dash.canceled += Dash;
    }

    void OnDisable()
    {
        // disable the input actions
        playerInputActions.Player.Disable();

        // unsubscribe from the input actions
        playerInputActions.Player.Move.performed -= Move;
        playerInputActions.Player.Jump.performed -= Jump;
        playerInputActions.Player.Roll.performed -= Roll;
        playerInputActions.Player.Dash.performed -= Dash;

        // unsubscribe from the canceled events
        playerInputActions.Player.Jump.canceled -= Jump;
        playerInputActions.Player.Move.canceled -= Move;
        playerInputActions.Player.Roll.canceled -= Roll;
        playerInputActions.Player.Dash.canceled -= Dash;
    }

    void FixedUpdate()
    {

        // check if the player is on the ground
        bool grounded = Physics2D.OverlapCircle(groundCheckTransform.position, groundCheckRadius, groundCheckLayerMask);

        // move the player with the current move amount we get from the input system
        if (!isRolling && !isDashing)
        {
            rb.linearVelocity = new Vector2(moveAmount * moveSpeed, rb.linearVelocityY);
        }

        // coyote time
        // if the player is on the ground, reset the coyote time counter
        if (grounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        // otherwise, decrement the coyote time counter
        else
        {
            coyoteTimeCounter -= Time.fixedDeltaTime;
        }

        animator.SetBool(groundedHash, grounded);
        animator.SetFloat(speedHash, Mathf.Abs(rb.linearVelocity.x));
        animator.SetFloat(velocityYHash, rb.linearVelocity.y);

        if (!wasGrounded && grounded)
        {
            animator.SetTrigger(landHash);
        }

        wasGrounded = grounded;

    }


    void Move(InputAction.CallbackContext context)
    {

        if (isRolling || isDashing) return;

        // read the move (X only) amount from the input system
        moveAmount = context.ReadValue<Vector2>().x;

        if (moveAmount > 0)
        {
            spriteRenderer.flipX = false;
        }
        else if (moveAmount < 0)
        {
            spriteRenderer.flipX = true;
        }
    }

    void Jump(InputAction.CallbackContext context)
    {

        if (isRolling || isDashing) return;

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

    void Roll(InputAction.CallbackContext context)
    {

        if (!Physics2D.OverlapCircle(groundCheckTransform.position, groundCheckRadius, groundCheckLayerMask)) return;
        if (rb.linearVelocityX == 0) return;
        if (isDashing || isRolling) return;

        if (context.performed)
        {
            animator.SetTrigger(rollHash);
        }
    }
    void Dash(InputAction.CallbackContext context)
    {
        if (!Physics2D.OverlapCircle(groundCheckTransform.position, groundCheckRadius, groundCheckLayerMask)) return;
        if (rb.linearVelocityX == 0) return;
        if (isDashing || isRolling) return;

        if (context.performed)
        {
            animator.SetTrigger(dashHash);
        }
    }

    void StartRoll()
    {
        isRolling = true;

        rb.linearVelocity = new Vector2(
            spriteRenderer.flipX ? -rollSpeed : rollSpeed,
            rb.linearVelocityY
        );

        Debug.Log("Start Roll");
    }

    void EndRoll()
    {
        isRolling = false;
        Debug.Log("End Roll");
    }

    void StartDash()
    {
        isDashing = true;

        rb.linearVelocity = new Vector2(
            spriteRenderer.flipX ? -dashSpeed : dashSpeed,
            rb.linearVelocityY
        );

        Debug.Log("Start Dash");
    }

    void EndDash()
    {
        isDashing = false;
        Debug.Log("End Dash");
    }


    // draw gizmos for the ground check // visible only in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheckTransform.position, groundCheckRadius);
    }
}
