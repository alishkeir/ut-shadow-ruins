using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombatController : MonoBehaviour
{

    [SerializeField] private float comboResetTime = 0.5f;


    Rigidbody2D rb;
    PlayerInputActions playerInputActions;
    PlayerAnimationController animationController;
    PlayerStateMachine playerStateMachine;
    Animator animator;
    PlayerController2D playerController;

    private static readonly int attack1Hash = Animator.StringToHash("Attack1");
    private static readonly int attack2Hash = Animator.StringToHash("Attack2");
    private static readonly int attack3Hash = Animator.StringToHash("Attack3");
    private static readonly int attack4Hash = Animator.StringToHash("Attack4");
    private static readonly int noComboHash = Animator.StringToHash("NoCombo");
    private static readonly int parryHash = Animator.StringToHash("Parry");

    float comboTimer;
    int attackIndex = 1;
    bool isAttacking = false;

    void Awake()
    {
        playerInputActions = new PlayerInputActions();
        rb = GetComponent<Rigidbody2D>();
        animationController = GetComponent<PlayerAnimationController>();
        playerStateMachine = GetComponent<PlayerStateMachine>();
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController2D>();

    }


    void OnEnable()
    {
        // enable the input actions
        playerInputActions.Player.Enable();

        // subscribe to the attack input action (performed only — canceled is not an attack)
        playerInputActions.Player.Attack.performed += Attack;
        playerInputActions.Player.Parry.performed += Parry;

        playerStateMachine.OnStateChanged += UpdateAnimation;


    }

    private void UpdateAnimation(PlayerStateMachine.PlayerState state)
    {
        if (state == PlayerStateMachine.PlayerState.Idle)
        {
            comboTimer = 0;
        }
    }

    void OnDisable()
    {
        // disable the input actions
        playerInputActions.Player.Disable();

        // unsubscribe from the attack input action
        playerInputActions.Player.Attack.performed -= Attack;
        playerInputActions.Player.Parry.performed -= Parry;

        playerStateMachine.OnStateChanged -= UpdateAnimation;

    }

    void Update()
    {
        if (!isAttacking)
        {
            if (comboTimer < comboResetTime)
            {
                comboTimer += Time.deltaTime;
            }
            else
            {
                attackIndex = 1;
            }
        }
    }


    private void Attack(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (isAttacking) return;
        if (playerStateMachine.IsLockedState()) return;


        // tell the Animator that a combo is in progress // Attack → Idle exit transitions (which require NoCombo = true) won't fire.
        animator.SetBool(noComboHash, false);

        isAttacking = true;
        playerStateMachine.ChangeState(PlayerStateMachine.PlayerState.Attacking);

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

    private void Parry(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (isAttacking) return;
        if (CantPerformAction()) return;

        playerStateMachine.ChangeState(PlayerStateMachine.PlayerState.Parrying);
        animator.SetTrigger(parryHash);
    }

    private bool CantPerformAction()
    {
        if (playerStateMachine.CurrentState == PlayerStateMachine.PlayerState.Jumping) return true;
        if (playerStateMachine.CurrentState == PlayerStateMachine.PlayerState.Falling) return true;
        if (playerStateMachine.CurrentState == PlayerStateMachine.PlayerState.Landing) return true;
        if (playerStateMachine.CurrentState == PlayerStateMachine.PlayerState.Rolling) return true;
        if (playerStateMachine.CurrentState == PlayerStateMachine.PlayerState.Dashing) return true;
        // if (playerStateMachine.CurrentState == PlayerStateMachine.PlayerState.Attacking) return true;
        if (playerStateMachine.CurrentState == PlayerStateMachine.PlayerState.Parrying) return true;
        if (playerStateMachine.CurrentState == PlayerStateMachine.PlayerState.Hurt) return true;
        if (playerStateMachine.CurrentState == PlayerStateMachine.PlayerState.Dead) return true;
        if (playerStateMachine.IsLockedState()) return true;
        return false;
    }

    void OnAttackEnd()
    {
        isAttacking = false;
        comboTimer = 0;

        if (attackIndex >= 4)
        {
            attackIndex = 1;
        }
        else
        {
            attackIndex++;
        }

        // allow the Animator to leave the attack state back to Idle.
        animator.SetBool(noComboHash, true);

        // snap the sprite to the latest facing direction set by Move() during the
        // attack (which doesn't flip the sprite because the state is locked).
        animationController.FlipSprite();

        playerStateMachine.ChangeState(PlayerStateMachine.PlayerState.Idle);
    }

    void OnParryEnd()
    {
        playerStateMachine.ChangeState(PlayerStateMachine.PlayerState.Idle);
    }

}
