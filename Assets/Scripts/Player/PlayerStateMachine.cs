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
    }

    public PlayerState CurrentState { get; private set; }

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
        return CurrentState == PlayerState.Rolling || CurrentState == PlayerState.Dashing;
    }
}
