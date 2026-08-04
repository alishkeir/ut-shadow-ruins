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

    public float FacingDirection { get; private set; } = 1f;
    public float Speed { get; private set; }
    public GameObject DetectedPlayer { get; private set; }

    public event Action<EnemyState> OnStateChanged;

    public void ChangeState(EnemyState newState)
    {
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
