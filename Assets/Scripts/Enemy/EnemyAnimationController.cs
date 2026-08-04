using System;
using UnityEngine;


// this script is responsible for controlling the animations of the enemy characters in the game
// it will only listen to the enemy's state change event and play the appropriate animations
public class EnemyAnimationController : MonoBehaviour
{


    private static readonly int speedHash = Animator.StringToHash("Speed");
    private static readonly int attack1Hash = Animator.StringToHash("Attack1");
    private static readonly int attack2Hash = Animator.StringToHash("Attack2");
    private static readonly int hurtHash = Animator.StringToHash("Hurt");
    private static readonly int healthHash = Animator.StringToHash("Health");


    EnemyStateMachine enemyStateMachine;
    SpriteRenderer enemySprite;
    Animator animator;

    private void OnEnable()
    {
        enemyStateMachine.OnStateChanged += UpdateAnimation;
    }

    private void OnDisable()
    {
        enemyStateMachine.OnStateChanged -= UpdateAnimation;
    }

    void Awake()
    {
        enemyStateMachine = GetComponent<EnemyStateMachine>();
        enemySprite = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        animator.SetFloat(speedHash, Math.Abs(enemyStateMachine.Speed));
        FlipSprite();
    }

    public void UpdateAnimation(EnemyStateMachine.EnemyState state)
    {
        switch (state)
        {
            case EnemyStateMachine.EnemyState.Attacking:
                animator.SetTrigger(attack1Hash);
                break;
            case EnemyStateMachine.EnemyState.Hurt:
                animator.SetTrigger(hurtHash);
                break;
            case EnemyStateMachine.EnemyState.Dead:
                animator.SetTrigger(healthHash);
                break;


        }
    }


    public void FlipSprite()
    {
        if (enemyStateMachine.FacingDirection > 0)
            enemySprite.flipX = false;

        else if (enemyStateMachine.FacingDirection < 0)
            enemySprite.flipX = true;
    }
}
