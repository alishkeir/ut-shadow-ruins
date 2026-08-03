using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombatController : MonoBehaviour
{

    [SerializeField] private float comboResetTime = 0.5f;


    PlayerInputActions playerInputActions;
    PlayerStateMachine playerStateMachine;

    float comboTimer;

    bool isAttacking = false;

    void Awake()
    {
        playerInputActions = new PlayerInputActions();
        playerStateMachine = GetComponent<PlayerStateMachine>();
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
                playerStateMachine.SetAttackIndex(1);
            }
        }
    }


    private void Attack(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (isAttacking) return;
        if (playerStateMachine.IsLockedState()) return;

        isAttacking = true;
        playerStateMachine.ChangeState(PlayerStateMachine.PlayerState.Attacking);

    }

    private void Parry(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        if (isAttacking) return;
        if (CantPerformAction()) return;

        playerStateMachine.ChangeState(PlayerStateMachine.PlayerState.Parrying);
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

        if (playerStateMachine.AttackIndex >= 4)
        {
            playerStateMachine.SetAttackIndex(1);
        }
        else
        {
            playerStateMachine.SetAttackIndex(playerStateMachine.AttackIndex + 1);
        }

        playerStateMachine.ChangeState(PlayerStateMachine.PlayerState.Idle);
    }

    void OnParryEnd()
    {
        playerStateMachine.ChangeState(PlayerStateMachine.PlayerState.Idle);
    }

}
