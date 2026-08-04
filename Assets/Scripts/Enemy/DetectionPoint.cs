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
        stateMachine.SetDetectedPlayer(other.gameObject);
        stateMachine.ChangeState(EnemyStateMachine.EnemyState.Chasing);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.gameObject.CompareTag("Player")) return;
        stateMachine.SetDetectedPlayer(null);
        stateMachine.ChangeState(EnemyStateMachine.EnemyState.Idle);

        StartCoroutine(ReturnToPatrolAfterCooldown(chaseIdleCooldown));
    }

    // to handle the cooldown before returning to patrol state
    private IEnumerator ReturnToPatrolAfterCooldown(float cooldown)
    {
        yield return new WaitForSeconds(cooldown);
        stateMachine.ChangeState(EnemyStateMachine.EnemyState.Patrolling);
    }
}
