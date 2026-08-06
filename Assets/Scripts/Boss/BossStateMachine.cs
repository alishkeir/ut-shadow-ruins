using System;
using UnityEngine;


// this script is the state machine for the boss
// it only stores the current state and some shared variables
// the boss controller and animation controller read from this
public class BossStateMachine : MonoBehaviour
{
    public enum BossState
    {
        Idle,
        Flying,
        Summoning,
        Smashing,
        SmashIdle,
        SmashRecover,
        Hurt,
        Dead
    }

    public BossState CurrentState { get; private set; }

    public float FacingDirection { get; private set; } = 1f;
    public float Speed { get; private set; }

    public GameObject DetectedPlayer { get; private set; }

    public event Action<BossState> OnStateChanged;

    // priority order: dead > hurt > smashing/summoning > smashidle/smashrecover > flying > idle
    // lower priority states can't override higher ones through ChangeState
    // use ForceChangeState for legit downward transitions like animation ends
    public static int GetPriority(BossState state)
    {
        switch (state)
        {
            case BossState.Idle: return 0;
            case BossState.Flying: return 1;
            case BossState.SmashRecover: return 2;
            case BossState.SmashIdle: return 2;
            case BossState.Summoning: return 3;
            case BossState.Smashing: return 3;
            case BossState.Hurt: return 4;
            case BossState.Dead: return 5;
            default: return 0;
        }
    }

    public void ChangeState(BossState newState)
    {
        // once dead, no state changes are allowed
        if (CurrentState == BossState.Dead) return;

        if (CurrentState == newState) return;

        // lower priority can't override higher priority
        if (GetPriority(newState) < GetPriority(CurrentState)) return;

        CurrentState = newState;

        OnStateChanged?.Invoke(CurrentState);
    }

    // force a state change, bypassing the priority check
    // used by animation events where a downward transition makes sense
    public void ForceChangeState(BossState newState)
    {
        if (CurrentState == BossState.Dead) return;

        if (CurrentState == newState) return;

        CurrentState = newState;

        OnStateChanged?.Invoke(CurrentState);
    }

    void Awake()
    {
        CurrentState = BossState.Flying;
    }

    public void SetFacingDirection(float direction)
    {
        FacingDirection = direction;
    }

    public void SetSpeed(float speed)
    {
        Speed = speed;
    }

    public void SetDetectedPlayer(GameObject player)
    {
        DetectedPlayer = player;
    }
}
