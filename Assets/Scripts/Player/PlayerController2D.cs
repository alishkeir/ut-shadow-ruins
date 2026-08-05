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
    PlayerStateMachine playerStateMachine;


    float coyoteTimeCounter;
    float moveAmount;
    bool grounded;

    void Awake()
    {
        playerInputActions = new PlayerInputActions();
        rb = GetComponent<Rigidbody2D>();
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
        grounded = Physics2D.OverlapCircle(groundCheckTransform.position, groundCheckRadius, groundCheckLayerMask);

        // move the player with the current move amount we get from the input system
        if (!playerStateMachine.IsLockedState())
        {
            rb.linearVelocity = new Vector2(moveAmount * moveSpeed, rb.linearVelocityY);
        }



        HandleCoyoteTime();

        HandleMovementStateChange();

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

            playerStateMachine.ChangeState(PlayerStateMachine.PlayerState.Running);
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
            rb.linearVelocity = new Vector2(playerStateMachine.FacingDirection * rollSpeed, rb.linearVelocityY);
            playerStateMachine.ChangeState(PlayerStateMachine.PlayerState.Rolling);
        }
    }
    void Dash(InputAction.CallbackContext context)
    {
        if (CantPerformAction()) return;

        if (context.performed)
        {
            rb.linearVelocity = new Vector2(playerStateMachine.FacingDirection * dashSpeed, rb.linearVelocityY);
            playerStateMachine.ChangeState(PlayerStateMachine.PlayerState.Dashing);
        }
    }

    public bool CantPerformAction()
    {
        if (playerStateMachine.IsLockedState()) return true;
        if (rb.linearVelocityX == 0) return true;
        if (!Physics2D.OverlapCircle(groundCheckTransform.position, groundCheckRadius, groundCheckLayerMask)) return true;
        return false;
    }

    void HandleCoyoteTime()
    {

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
    }

    void HandleMovementStateChange()
    {


        playerStateMachine.UpdateMovement(Mathf.Abs(rb.linearVelocityX), rb.linearVelocityY, grounded);

        if (playerStateMachine.IsLockedState()) return;


        // use ForceChangeState because landing (Jumping/Falling → Idle/Running)
        // is a downward transition that ChangeState would block
        if (!grounded)
        {
            playerStateMachine.ForceChangeState(rb.linearVelocityY > 0 ? PlayerStateMachine.PlayerState.Jumping : PlayerStateMachine.PlayerState.Falling);
            return;
        }

        if (Mathf.Abs(rb.linearVelocityX) > 0.01f)
        {
            playerStateMachine.UpdateMovement(Mathf.Abs(rb.linearVelocityX), rb.linearVelocityY, true);
            playerStateMachine.ForceChangeState(PlayerStateMachine.PlayerState.Running);


        }
        else
        {
            playerStateMachine.UpdateMovement(0, rb.linearVelocityY, true);
            playerStateMachine.ForceChangeState(PlayerStateMachine.PlayerState.Idle);
   
        }

    }


    void HandleRollOrDashEnd()
    {
        // force the transition back to Idle — ChangeState would block it
        // because Idle has lower priority than Rolling/Dashing
        playerStateMachine.ForceChangeState(PlayerStateMachine.PlayerState.Idle);
        HandleMovementStateChange();
    }


    // draw gizmos for the ground check // visible only in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheckTransform.position, groundCheckRadius);
    }
}
