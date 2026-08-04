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

        UpdateFacingDirection();

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

    // update the facing direction every frame based on the current state
    // chasing/attacking → face the player, patrolling → face the patrol direction
    private void UpdateFacingDirection()
    {
        switch (enemyStateMachine.CurrentState)
        {
            case EnemyStateMachine.EnemyState.Chasing:
            case EnemyStateMachine.EnemyState.Attacking:
                if (enemyStateMachine.DetectedPlayer != null)
                {
                    int dir = Math.Sign(enemyStateMachine.DetectedPlayer.transform.position.x - transform.position.x);
                    if (dir != 0)
                        enemyStateMachine.SetFacingDirection(dir);
                }
                break;
            case EnemyStateMachine.EnemyState.Patrolling:
                enemyStateMachine.SetFacingDirection(patrolDirection);
                break;
            // idle, hurt, dead: keep current facing direction
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

        // just chase — move straight toward the player on the X axis (no vertical follow)
        Vector2 playerPos = enemyStateMachine.DetectedPlayer.transform.position;
        int direction = Math.Sign(playerPos.x - transform.position.x);

        // only stop if the enemy is touching a boundary AND trying to move past it
        // (the player is beyond that boundary). Otherwise keep chasing, even
        // when starting from a boundary edge, so it can move back inward.
        if ((direction < 0 && transform.position.x <= leftPatrolBoundary) ||
            (direction > 0 && transform.position.x >= rightPatrolBoundary))
        {
            StopMoving();
            // point patrol direction inward so patrol resumes cleanly if it resumes later
            patrolDirection = transform.position.x <= leftPatrolBoundary ? 1 : -1;
            isWaitingAtBoundary = false;
            // stay in chasing — facing direction is handled by UpdateFacingDirection()
            return;
        }

        rb.linearVelocity = new Vector2(direction * chaseSpeed, rb.linearVelocityY);
    }

    // draw gizmos for the ground check // visible only in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(patrolCheckTransform.position, new Vector3(patrolWidth, 0.1f, 0.1f));
    }

}