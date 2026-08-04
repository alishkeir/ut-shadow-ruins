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
        if (!other.gameObject.CompareTag("Player")) return;

        // ignore trigger events when inactive (e.g. stopping the game in editor)
        // or when the enemy is already dead
        if (!isActiveAndEnabled) return;
        if (stateMachine == null || stateMachine.CurrentState == EnemyStateMachine.EnemyState.Dead) return;

        stateMachine.ChangeState(EnemyStateMachine.EnemyState.Attacking);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag("Player")) return;

        // ignore trigger events when inactive (e.g. stopping the game in editor)
        // or when the enemy is already dead
        if (!isActiveAndEnabled) return;
        if (stateMachine == null || stateMachine.CurrentState == EnemyStateMachine.EnemyState.Dead) return;

        // player left attack range — drop to Chasing if still detected,
        // otherwise drop to Idle (the DetectionPoint cooldown coroutine
        // will eventually transition to Patrolling)
        if (stateMachine.DetectedPlayer != null)
            stateMachine.ForceChangeState(EnemyStateMachine.EnemyState.Chasing);
        else
            stateMachine.ForceChangeState(EnemyStateMachine.EnemyState.Idle);
    }
}
