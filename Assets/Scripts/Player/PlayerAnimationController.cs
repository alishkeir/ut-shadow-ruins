using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{

    [SerializeField] private SpriteRenderer playerSprite;


    Animator animator;
    PlayerStateMachine playerStateMachine;

    // move animations
    private static readonly int speedHash = Animator.StringToHash("Speed");
    private static readonly int velocityYHash = Animator.StringToHash("VelocityY");
    private static readonly int groundedHash = Animator.StringToHash("Grounded");
    private static readonly int landHash = Animator.StringToHash("Land");
    private static readonly int rollHash = Animator.StringToHash("Roll");
    private static readonly int dashHash = Animator.StringToHash("Dash");

    // combat animations
    private static readonly int attack1Hash = Animator.StringToHash("Attack1");
    private static readonly int attack2Hash = Animator.StringToHash("Attack2");
    private static readonly int attack3Hash = Animator.StringToHash("Attack3");
    private static readonly int attack4Hash = Animator.StringToHash("Attack4");
    private static readonly int noComboHash = Animator.StringToHash("NoCombo");
    private static readonly int parryHash = Animator.StringToHash("Parry");
    private static readonly int deathHash = Animator.StringToHash("Die");
    private static readonly int hurtHash = Animator.StringToHash("Hurt");

    // variables
    private static readonly int healthHash = Animator.StringToHash("Health");





    void Awake()
    {
        animator = GetComponent<Animator>();
        playerStateMachine = GetComponent<PlayerStateMachine>();
    }

    void FixedUpdate()
    {
        animator.SetFloat(speedHash, playerStateMachine.Speed);
        animator.SetFloat(velocityYHash, playerStateMachine.VelocityY);
        animator.SetBool(groundedHash, playerStateMachine.Grounded);
        animator.SetFloat(healthHash, playerStateMachine.Health);

        FlipSprite();
    }

    void OnEnable()
    {
        playerStateMachine.OnStateChanged += UpdateAnimation;
    }

    void OnDisable()
    {
        playerStateMachine.OnStateChanged -= UpdateAnimation;
    }


    void UpdateAnimation(PlayerStateMachine.PlayerState state)
    {
        switch (state)
        {
            case PlayerStateMachine.PlayerState.Rolling:
                // set NoCombo to false so the Any State → Fall transition (which requires NoCombo = true) does not interrupt the roll while the player is still in the air.
                UpdateNoCombo(false);
                animator.SetTrigger(rollHash);
                break;

            case PlayerStateMachine.PlayerState.Dashing:
                UpdateNoCombo(false);
                animator.SetTrigger(dashHash);
                break;

            case PlayerStateMachine.PlayerState.Landing:
                animator.SetBool(groundedHash, playerStateMachine.Grounded);
                animator.SetTrigger(landHash);
                break;

            case PlayerStateMachine.PlayerState.Attacking:
                HandleAttackAnimation(playerStateMachine.AttackIndex);
                break;
            case PlayerStateMachine.PlayerState.Parrying:
                UpdateParryAnimation();
                break;

            case PlayerStateMachine.PlayerState.Idle:
                // leave the attack state back to Idle.
                UpdateNoCombo(true);
                FlipSprite();
                break;

            case PlayerStateMachine.PlayerState.Running:
                FlipSprite();
                break;

            case PlayerStateMachine.PlayerState.Dead:
                FlipSprite();
                animator.SetTrigger(deathHash);
                break;

            case PlayerStateMachine.PlayerState.Hurt:
                FlipSprite();
                animator.SetTrigger(hurtHash);
                break;

        }
    }

    public void UpdateNoCombo(bool noCombo)
    {
        animator.SetBool(noComboHash, noCombo);
    }

    public void HandleAttackAnimation(int attackIndex)
    {
        // tell the Animator that a combo is in progress // Attack → Idle exit transitions (which require NoCombo = true) won't fire.
        UpdateNoCombo(false);

        switch (attackIndex)
        {
            case 1:
                animator.SetTrigger(attack1Hash);
                break;
            case 2:
                animator.SetTrigger(attack2Hash);
                break;
            case 3:
                animator.SetTrigger(attack3Hash);
                break;
            case 4:
                animator.SetTrigger(attack4Hash);
                break;
        }
    }

    public void UpdateParryAnimation()
    {
        animator.SetTrigger(parryHash);
    }

    public void UpdateParryAnimation(bool parry)
    {
        animator.SetBool(parryHash, parry);
    }


    // snap the sprite to the latest facing direction set by Move() during the attack (which doesn't flip the sprite because the state is locked).
    // now it's rotating the sprite to match the facing direction, so the hitbox with animations is facing the right direction.
    public void FlipSprite()
    {
        if (playerStateMachine.FacingDirection > 0)
        {
            playerSprite.transform.rotation = Quaternion.Euler(0, 0, 0);
        }
        else if (playerStateMachine.FacingDirection < 0)
        {
            playerSprite.transform.rotation = Quaternion.Euler(0, 180f, 0);
        }
    }
}
