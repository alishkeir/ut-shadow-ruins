using System;
using UnityEngine;


// this script is responsible for controlling the animations of the enemy characters in the game
// it will only listen to the enemy's state change event and play the appropriate animations
public class EnemyAnimationController : MonoBehaviour
{


    private static readonly int speedHash = Animator.StringToHash("Speed");
    private static readonly int attack1Hash = Animator.StringToHash("Attack");
    private static readonly int hurtHash = Animator.StringToHash("Hurt");
    private static readonly int deathhHash = Animator.StringToHash("Die");


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

        if (enemyStateMachine.CurrentState == EnemyStateMachine.EnemyState.Dead) return;

        animator.SetFloat(speedHash, Math.Abs(enemyStateMachine.Speed));
        FlipSprite();

        // keep re-triggering the attack animation as long as the enemy remains in the Attacking state
        if (enemyStateMachine.CurrentState == EnemyStateMachine.EnemyState.Attacking)
        {
            animator.SetTrigger(attack1Hash);
        }
    }

    public void UpdateAnimation(EnemyStateMachine.EnemyState state)
    {
        switch (state)
        {
            case EnemyStateMachine.EnemyState.Attacking:
                animator.SetTrigger(attack1Hash);
                break;
            case EnemyStateMachine.EnemyState.Hurt:
                animator.ResetTrigger(attack1Hash);
                animator.SetTrigger(hurtHash);
                break;
            case EnemyStateMachine.EnemyState.Dead:
                animator.ResetTrigger(attack1Hash);
                animator.ResetTrigger(hurtHash);
                animator.SetTrigger(deathhHash);
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
