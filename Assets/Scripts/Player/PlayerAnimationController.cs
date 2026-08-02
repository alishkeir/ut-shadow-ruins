using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    Animator animator;
    SpriteRenderer spriteRenderer;
    PlayerStateMachine playerStateMachine;

    private static readonly int speedHash = Animator.StringToHash("Speed");
    private static readonly int velocityYHash = Animator.StringToHash("VelocityY");
    private static readonly int groundedHash = Animator.StringToHash("Grounded");
    private static readonly int landHash = Animator.StringToHash("Land");
    private static readonly int rollHash = Animator.StringToHash("Roll");
    private static readonly int dashHash = Animator.StringToHash("Dash");

    private bool grounded;
    private float speed;
    private float velocityY;

    void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        playerStateMachine = GetComponent<PlayerStateMachine>();
    }

    void Update()
    {
        animator.SetFloat(speedHash, speed);
        animator.SetFloat(velocityYHash, velocityY);
        animator.SetBool(groundedHash, grounded);
    }

    void OnEnable()
    {
        playerStateMachine.OnStateChanged += UpdateAnimation;
    }

    void OnDisable()
    {
        playerStateMachine.OnStateChanged -= UpdateAnimation;
    }

    public void UpdateMovementAnimation(float speed, float velocityY, bool grounded)
    {
        this.speed = speed;
        this.velocityY = velocityY;
        this.grounded = grounded;
    }

    void UpdateAnimation(PlayerStateMachine.PlayerState state)
    {
        switch (state)
        {
            case PlayerStateMachine.PlayerState.Rolling:
                animator.SetTrigger(rollHash);
                break;

            case PlayerStateMachine.PlayerState.Dashing:
                animator.SetTrigger(dashHash);
                break;

            case PlayerStateMachine.PlayerState.Landing:
                animator.SetBool(groundedHash, grounded);
                animator.SetTrigger(landHash);
                break;
        }
    }


    public void FlipSprite(float direction)
    {
        if (direction > 0)
            spriteRenderer.flipX = false;

        else if (direction < 0)
            spriteRenderer.flipX = true;
    }
}
