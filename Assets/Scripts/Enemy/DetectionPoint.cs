using System.Collections;
using UnityEngine;

public class DetectionPoint : MonoBehaviour
{

    [SerializeField] private float chaseIdleCooldown = 2f;

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

        stateMachine.SetDetectedPlayer(other.gameObject);
        stateMachine.ChangeState(EnemyStateMachine.EnemyState.Chasing);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag("Player")) return;

        // ignore trigger events when inactive (e.g. stopping the game in editor)
        // or when the enemy is already dead
        if (!isActiveAndEnabled) return;
        if (stateMachine == null || stateMachine.CurrentState == EnemyStateMachine.EnemyState.Dead) return;

        stateMachine.SetDetectedPlayer(null);

        // if currently attacking, don't change state here — the player may
        // still be inside the attack range. AttackPoint will handle it
        // when the player leaves attack range
        if (stateMachine.CurrentState != EnemyStateMachine.EnemyState.Attacking)
        {
            stateMachine.ForceChangeState(EnemyStateMachine.EnemyState.Idle);
        }

        StartCoroutine(ReturnToPatrolAfterCooldown(chaseIdleCooldown));
    }

    // to handle the cooldown before returning to patrol state
    private IEnumerator ReturnToPatrolAfterCooldown(float cooldown)
    {
        if (stateMachine == null) yield return null;

        yield return new WaitForSeconds(cooldown);
        // ChangeState respects priority — Patrolling can only override Idle,
        // not Chasing or Attacking, so it's safe even if the player
        // re-entered the detection zone during the cooldown
        stateMachine.ChangeState(EnemyStateMachine.EnemyState.Patrolling);
    }
}
