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
    PlayerAnimationController animationController;
    PlayerStateMachine playerStateMachine;

    float facingDirection = 1f;
    float coyoteTimeCounter;
    float moveAmount;
    bool wasGrounded;

    bool isRolling;
    bool isDashing;



    void Awake()
    {
        playerInputActions = new PlayerInputActions();
        rb = GetComponent<Rigidbody2D>();
        animationController = GetComponent<PlayerAnimationController>();
        playerStateMachine = GetComponent<PlayerStateMachine>();
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
        if (playerStateMachine.CurrentState != PlayerStateMachine.PlayerState.Rolling && playerStateMachine.CurrentState != PlayerStateMachine.PlayerState.Dashing)
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

        animationController.UpdateMovementAnimation(Mathf.Abs(rb.linearVelocity.x), rb.linearVelocity.y, grounded);

        if (!wasGrounded && grounded)
        {
            animationController.PlayLand();
        }

        wasGrounded = grounded;

    }


    void Move(InputAction.CallbackContext context)
    {

        if (playerStateMachine.CurrentState == PlayerStateMachine.PlayerState.Rolling || playerStateMachine.CurrentState == PlayerStateMachine.PlayerState.Dashing) return;

        // read the move (X only) amount from the input system
        moveAmount = context.ReadValue<Vector2>().x;

        animationController.FlipSprite(moveAmount);

        if (moveAmount != 0)
        {
            facingDirection = Mathf.Sign(moveAmount);
        }
    }

    void Jump(InputAction.CallbackContext context)
    {

        if (playerStateMachine.CurrentState == PlayerStateMachine.PlayerState.Rolling || playerStateMachine.CurrentState == PlayerStateMachine.PlayerState.Dashing) return;

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
        if (playerStateMachine.CurrentState == PlayerStateMachine.PlayerState.Rolling || playerStateMachine.CurrentState == PlayerStateMachine.PlayerState.Dashing) return;

        if (context.performed)
        {
            playerStateMachine.ChangeState(PlayerStateMachine.PlayerState.Rolling);
            rb.linearVelocity = new Vector2(facingDirection * rollSpeed, rb.linearVelocityY);
            animationController.PlayRoll();
        }
    }
    void Dash(InputAction.CallbackContext context)
    {
        if (!Physics2D.OverlapCircle(groundCheckTransform.position, groundCheckRadius, groundCheckLayerMask)) return;
        if (rb.linearVelocityX == 0) return;
        if (playerStateMachine.CurrentState == PlayerStateMachine.PlayerState.Rolling || playerStateMachine.CurrentState == PlayerStateMachine.PlayerState.Dashing) return;

        if (context.performed)
        {
            playerStateMachine.ChangeState(PlayerStateMachine.PlayerState.Dashing);
            rb.linearVelocity = new Vector2(facingDirection * dashSpeed, rb.linearVelocityY);
            animationController.PlayDash();
        }
    }

    void StartRoll()
    {


    }

    void EndRoll()
    {
    }

    void StartDash()
    {

    }

    void EndDash()
    {
    }


    // draw gizmos for the ground check // visible only in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheckTransform.position, groundCheckRadius);
    }
}
