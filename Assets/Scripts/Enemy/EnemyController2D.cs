using System;
using UnityEngine;
using UnityEngine.Rendering;


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
    [SerializeField] private Transform patrolCheckTransform;

    Rigidbody2D rb;
    EnemyStateMachine enemyStateMachine;

    private float patrolCenterX;
    private int patrolDirection = 1;

    private float leftPatrolBoundary;
    private float rightPatrolBoundary;



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
            default:
                StopMoving();
                break;
        }
    }



    private void Patrol()
    {
        // flip direction when the enemy reaches a patrol boundary
        if (patrolDirection > 0 && transform.position.x >= rightPatrolBoundary)
        {
            patrolDirection = -1;
            enemyStateMachine.SetFacingDirection(-1);
        }
        else if (patrolDirection < 0 && transform.position.x <= leftPatrolBoundary)
        {
            patrolDirection = 1;
            enemyStateMachine.SetFacingDirection(1);
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

    private void Chase() { }

    // draw gizmos for the ground check // visible only in the editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(patrolCheckTransform.position, new Vector3(patrolWidth, 0.1f, 0.1f));
    }

}
