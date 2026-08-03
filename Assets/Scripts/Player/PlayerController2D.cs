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
    Animator animator;

    private static readonly int animatorNoComboHash = Animator.StringToHash("NoCombo");

    float coyoteTimeCounter;
    float moveAmount;

    void Awake()
    {
        playerInputActions = new PlayerInputActions();
        rb = GetComponent<Rigidbody2D>();
        animationController = GetComponent<PlayerAnimationController>();
        playerStateMachine = GetComponent<PlayerStateMachine>();
        animator = GetComponent<Animator>();
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
        if (!playerStateMachine.IsLockedState())
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

        HandleMovementStateChange();

        animationController.UpdateMovementAnimation(Mathf.Abs(rb.linearVelocityX), rb.linearVelocityY, grounded);

    }


    void Move(InputAction.CallbackContext context)
    {
        // read the move (X only) amount from the input system
        moveAmount = context.ReadValue<Vector2>().x;

        if (moveAmount != 0)
        {
            playerStateMachine.SetFacingDirection(Mathf.Sign(moveAmount));
        }

        if (playerStateMachine.IsLockedState()) return;

        if (moveAmount != 0)
        {
            animationController.FlipSprite();
        }
    }


    // variable height jumping
    void Jump(InputAction.CallbackContext context)
    {
        // if the player performed the jump action
        if (context.performed)
        {
            if (playerStateMachine.IsLockedState()) return;

            if (!Physics2D.OverlapCircle(groundCheckTransform.position, groundCheckRadius, groundCheckLayerMask)) return;

            // allows the playerto jump after leaving the ground for a short period of time
            if (coyoteTimeCounter <= 0) return;

            coyoteTimeCounter = 0;

            rb.linearVelocityY = 0;

            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
        }
        // if the player canceled the jump action // released the jump button before the jump reaches its maximum height
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
        if (CantPerformAction()) return;

        if (context.performed)
        {
            playerStateMachine.ChangeState(PlayerStateMachine.PlayerState.Rolling);
            rb.linearVelocity = new Vector2(playerStateMachine.FacingDirection * rollSpeed, rb.linearVelocityY);
        }
    }
    void Dash(InputAction.CallbackContext context)
    {
        if (CantPerformAction()) return;

        if (context.performed)
        {
            playerStateMachine.ChangeState(PlayerStateMachine.PlayerState.Dashing);
            rb.linearVelocity = new Vector2(playerStateMachine.FacingDirection * dashSpeed, rb.linearVelocityY);
        }
    }

    public bool CantPerformAction()
    {
        if (playerStateMachine.IsLockedState()) return true;
        if (rb.linearVelocityX == 0) return true;
        if (!Physics2D.OverlapCircle(groundCheckTransform.position, groundCheckRadius, groundCheckLayerMask)) return true;
        return false;
    }

    void HandleRollOrDashEnd()
    {
        // Reset NoCombo so the Any State → Fall transition (which requires NoCombo = true) is allowed to fire while the player is still in the air.
        animator.SetBool(animatorNoComboHash, true);

        // if the player is still in the air when the roll/dash ends, keep the falling instead of switching to Idle/Running.
        bool grounded = Physics2D.OverlapCircle(groundCheckTransform.position, groundCheckRadius, groundCheckLayerMask);
        if (!grounded)
        {
            playerStateMachine.ChangeState(rb.linearVelocityY > 0
                ? PlayerStateMachine.PlayerState.Jumping
                : PlayerStateMachine.PlayerState.Falling);
            return;
        }

        if (Mathf.Abs(moveAmount) > 0.01f)
        {
            playerStateMachine.ChangeState(PlayerStateMachine.PlayerState.Running);
            animationController.UpdateMovementAnimation(moveAmount, rb.linearVelocityY, true);

        }
        else
        {
            playerStateMachine.ChangeState(PlayerStateMachine.PlayerState.Idle);
            animationController.UpdateMovementAnimation(0, rb.linearVelocityY, true);
        }

        animationController.FlipSprite();
    }


    void HandleMovementStateChange()
    {
        if (playerStateMachine.IsLockedState()) return;

        bool grounded = Physics2D.OverlapCircle(groundCheckTransform.position, groundCheckRadius, groundCheckLayerMask);

        if (!grounded)
        {
            playerStateMachine.ChangeState(rb.linearVelocityY > 0 ? PlayerStateMachine.PlayerState.Jumping : PlayerStateMachine.PlayerState.Falling);
        }
        else if (Mathf.Abs(moveAmount) > 0.01f)
        {
            playerStateMachine.ChangeState(PlayerStateMachine.PlayerState.Running);
        }
        else
        {
            playerStateMachine.ChangeState(PlayerStateMachine.PlayerState.Idle);
        }
    }


    // draw gizmos for the ground check // visible only in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheckTransform.position, groundCheckRadius);
    }
}
