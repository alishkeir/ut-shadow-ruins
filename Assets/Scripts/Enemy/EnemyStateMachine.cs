using System;
using UnityEngine;


// this script is used to control the state machine of the enemy
// it will only be used to store state and some shared variables
public class EnemyStateMachine : MonoBehaviour
{

    public enum EnemyState
    {
        Idle,
        Patrolling,
        Chasing,
        Attacking,
        Hurt,
        Dead
    }

    public EnemyState CurrentState { get; private set; }
    public GameObject DetectedPlayer { get; private set; }

    public float FacingDirection { get; private set; } = 1f;
    public float Speed { get; private set; }
    public float Health { get; private set; }

    public event Action<EnemyState> OnStateChanged;

    // priority order: dead > hurt > attacking > chasing > patrolling > idle
    // lower priority states can't override higher ones through ChangeState
    // use ForceChangeState for legit downward transitions like trigger exits
    public static int GetPriority(EnemyState state)
    {
        switch (state)
        {
            case EnemyState.Idle: return 0;
            case EnemyState.Patrolling: return 1;
            case EnemyState.Chasing: return 2;
            case EnemyState.Attacking: return 3;
            case EnemyState.Hurt: return 4;
            case EnemyState.Dead: return 5;
            default: return 0;
        }
    }

    public void ChangeState(EnemyState newState)
    {
        // once dead, no state changes are allowed — death is terminal
        if (CurrentState == EnemyState.Dead) return;

        if (CurrentState == newState) return;

        // lower priority can't override higher priority
        if (GetPriority(newState) < GetPriority(CurrentState)) return;

        CurrentState = newState;

        OnStateChanged?.Invoke(CurrentState);
    }

    // force a state change, bypassing the priority check
    // used by trigger exits (player leaving a zone) where a downward
    // transition makes sense. still respects the dead state
    public void ForceChangeState(EnemyState newState)
    {
        if (CurrentState == EnemyState.Dead) return;

        if (CurrentState == newState) return;

        CurrentState = newState;

        OnStateChanged?.Invoke(CurrentState);
    }


    void Awake()
    {
        CurrentState = EnemyState.Patrolling;
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
