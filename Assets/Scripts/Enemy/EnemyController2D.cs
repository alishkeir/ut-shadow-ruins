using System;
using UnityEngine;


// base class for all enemy controllers
// this class will handle the enemy's movement and attack statesd only
// script will only update the enemy's state and not handle any animations or other components
public class EnemyController2D : MonoBehaviour
{
    [Header("Enemy Movement")]
    [SerializeField] private float chaseSpeed = 5f;

    [Header("Patrol")]
    [SerializeField] private float patrolSpeed = 5f;
    [SerializeField] private float patrolWidth = 20f;
    [SerializeField] private float patrolWaitTime = 1f;
    [SerializeField] private Transform patrolCheckTransform;

    Rigidbody2D rb;
    EnemyStateMachine enemyStateMachine;

    private float patrolCenterX;
    private int patrolDirection = 1;

    private float leftPatrolBoundary;
    private float rightPatrolBoundary;

    private bool isWaitingAtBoundary;
    private float patrolWaitTimer;



    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        enemyStateMachine = GetComponent<EnemyStateMachine>();
    }

    void Start()
    {
        patrolCenterX = transform.position.x;
        leftPatrolBoundary = patrolCenterX - patrolWidth * 0.5f;
        rightPatrolBoundary = patrolCenterX + patrolWidth * 0.5f;

        Debug.Log("Left: " + leftPatrolBoundary);
        Debug.Log("Right: " + rightPatrolBoundary);
        Debug.Log("Center: " + patrolCenterX);
    }



    void FixedUpdate()
    {
        enemyStateMachine.SetSpeed(rb.linearVelocityX);

        switch (enemyStateMachine.CurrentState)
        {
            case EnemyStateMachine.EnemyState.Patrolling:
                Patrol();
                break;
            case EnemyStateMachine.EnemyState.Chasing:
                Chase();
                break;
            case EnemyStateMachine.EnemyState.Dead:
                StopMoving();
                break;
            default:
                StopMoving();
                break;
        }
    }



    private void Patrol()
    {
        // wait at the boundary before turning around
        if (isWaitingAtBoundary)
        {
            StopMoving();
            patrolWaitTimer -= Time.fixedDeltaTime;
            if (patrolWaitTimer <= 0f)
            {
                isWaitingAtBoundary = false;
                // flip direction after the wait
                patrolDirection = -patrolDirection;
                enemyStateMachine.SetFacingDirection(patrolDirection);
            }
            return;
        }

        // start waiting when the enemy reaches a patrol boundary
        if ((patrolDirection > 0 && transform.position.x >= rightPatrolBoundary) ||
            (patrolDirection < 0 && transform.position.x <= leftPatrolBoundary))
        {
            isWaitingAtBoundary = true;
            patrolWaitTimer = patrolWaitTime;
            StopMoving();
            return;
        }

        rb.linearVelocity = new Vector2(
            patrolDirection * patrolSpeed,
            rb.linearVelocityY
        );
    }

    private void StopMoving()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    private void Chase()
    {
        if (!enemyStateMachine.DetectedPlayer) return;

        // if the enemy reaches a patrol boundary while chasing, return to patrol
        if (transform.position.x <= leftPatrolBoundary || transform.position.x >= rightPatrolBoundary)
        {
            StopMoving();
            // point patrol direction inward so it doesn't immediately walk into the boundary
            patrolDirection = transform.position.x <= leftPatrolBoundary ? 1 : -1;
            enemyStateMachine.SetFacingDirection(patrolDirection);
            isWaitingAtBoundary = false;
            enemyStateMachine.ChangeState(EnemyStateMachine.EnemyState.Patrolling);
            return;
        }

        Vector2 playerPos = enemyStateMachine.DetectedPlayer.transform.position;
        float xDistance = playerPos.x - transform.position.x;

        // chase immediately as soon as the player is detected, on the X axis only (no vertical follow)
        if (Mathf.Abs(xDistance) > 0.01f)
        {
            int direction = Math.Sign(xDistance);
            rb.linearVelocity = new Vector2(direction * chaseSpeed, rb.linearVelocityY);
            enemyStateMachine.SetFacingDirection(direction);
        }
        else
        {
            StopMoving();
        }
    }

    // draw gizmos for the ground check // visible only in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(patrolCheckTransform.position, new Vector3(patrolWidth, 0.1f, 0.1f));
    }

}