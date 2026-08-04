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

    public event Action<PlayerState> OnStateChanged;



    void Awake()
    {
        CurrentState = PlayerState.Idle;
    }

    public void ChangeState(PlayerState newState)
    {
        if (CurrentState == newState) return;

        CurrentState = newState;

        OnStateChanged?.Invoke(CurrentState);
    }

    public bool IsLockedState()
    {
        return CurrentState == PlayerState.Rolling
            || CurrentState == PlayerState.Dashing
            || CurrentState == PlayerState.Attacking
            || CurrentState == PlayerState.Parrying;
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
}
