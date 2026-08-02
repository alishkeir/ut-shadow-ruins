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


    void Awake()
    {
        CurrentState = PlayerState.Idle;
    }


    public void ChangeState(PlayerState newState)
    {
        CurrentState = newState;
    }
}
