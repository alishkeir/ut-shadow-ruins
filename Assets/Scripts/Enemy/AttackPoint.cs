using UnityEngine;

public class AttackPoint : MonoBehaviour
{

    EnemyStateMachine stateMachine;

    void Awake()
    {
        stateMachine = GetComponentInParent<EnemyStateMachine>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        stateMachine.ChangeState(EnemyStateMachine.EnemyState.Attacking);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        stateMachine.ChangeState(EnemyStateMachine.EnemyState.Chasing);
    }
}
