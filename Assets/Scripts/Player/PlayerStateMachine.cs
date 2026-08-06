using System;
using UnityEngine;

public class PlayerStateMachine : MonoBehaviour
{
    public enum PlayerState
    {
        Idle,
        Running,
        Jumping,
        Falling,
        Landing,
        Rolling,
        Dashing,
        Attacking,
        Parrying,
        Hurt,
        Dead
    }

    public PlayerState CurrentState { get; private set; }

    public float FacingDirection { get; private set; } = 1f;
    public int AttackIndex { get; private set; } = 1;
    public bool Grounded { get; private set; }
    public float Speed { get; private set; }
    public float VelocityY { get; private set; }
    public float Health { get; private set; }

    public event Action<PlayerState> OnStateChanged;


    // priority order: dead > attacking > parrying > hurt > dashing > rolling > landing > falling > jumping > running > idle
    // lower priority states can't override higher ones through ChangeState
    // use ForceChangeState for legit downward transitions like animation ends
    public static int GetPriority(PlayerState state)
    {
        switch (state)
        {
            case PlayerState.Idle: return 0;
            case PlayerState.Running: return 0;
            case PlayerState.Jumping: return 1;
            case PlayerState.Falling: return 1;
            case PlayerState.Landing: return 1;
            case PlayerState.Rolling: return 2;
            case PlayerState.Dashing: return 2;
            case PlayerState.Hurt: return 3;
            case PlayerState.Parrying: return 4;
            case PlayerState.Attacking: return 5;
            case PlayerState.Dead: return 10;
            default: return 0;
        }
    }


    void Awake()
    {
        CurrentState = PlayerState.Idle;
    }

    public void ChangeState(PlayerState newState)
    {
        // once dead, no state changes are allowed — death is terminal
        if (CurrentState == PlayerState.Dead) return;

        if (CurrentState == newState) return;

        // lower priority can't override higher priority
        if (GetPriority(newState) < GetPriority(CurrentState)) return;

        CurrentState = newState;

        OnStateChanged?.Invoke(CurrentState);
    }

    // force a state change, bypassing the priority check
    // used by animation events and movement state changes where a downward
    // transition makes sense. still respects the dead state
    public void ForceChangeState(PlayerState newState)
    {
        if (CurrentState == PlayerState.Dead) return;

        if (CurrentState == newState) return;

        CurrentState = newState;

        OnStateChanged?.Invoke(CurrentState);
    }

    public bool IsLockedState()
    {
        return CurrentState == PlayerState.Rolling
            || CurrentState == PlayerState.Dashing
            || CurrentState == PlayerState.Attacking
            || CurrentState == PlayerState.Parrying
            || CurrentState == PlayerState.Hurt
            || CurrentState == PlayerState.Dead;
    }

    public void SetFacingDirection(float direction)
    {
        FacingDirection = direction;
    }

    public void UpdateMovement(float speed, float velocityY, bool grounded)
    {
        Speed = speed;
        VelocityY = velocityY;
        Grounded = grounded;
    }

    public void SetAttackIndex(int index)
    {
        AttackIndex = index;
    }

    public void SetHealth(float health)
    {
        Health = health;
    }
}
