using System;
using UnityEngine;


// this script is responsible for controlling the animations of the enemy characters in the game
// it will only listen to the enemy's state change event and play the appropriate animations
public class EnemyAnimationController : MonoBehaviour
{

    [SerializeField] private GameObject healthbarCanvas;
    [SerializeField] private float chaseAnimationSpeed = 1.5f;
    [SerializeField] private float attackCooldown = 1.5f;

    private static readonly int speedHash = Animator.StringToHash("Speed");
    private static readonly int attack1Hash = Animator.StringToHash("Attack");
    private static readonly int hurtHash = Animator.StringToHash("Hurt");
    private static readonly int deathhHash = Animator.StringToHash("Die");


    EnemyStateMachine enemyStateMachine;
    Animator animator;

    float attackTimer;
    bool isAttackPlaying;

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
        animator = GetComponent<Animator>();
    }

    void FixedUpdate()
    {

        if (enemyStateMachine.CurrentState == EnemyStateMachine.EnemyState.Dead) return;

        animator.SetFloat(speedHash, Math.Abs(enemyStateMachine.Speed));

        FlipSprite();

        // only count the cooldown while in Attacking and no attack animation
        // is currently playing (OnAttackEnds clears isAttackPlaying)
        if (enemyStateMachine.CurrentState == EnemyStateMachine.EnemyState.Attacking && !isAttackPlaying)
        {
            attackTimer += Time.fixedDeltaTime;
            if (attackTimer >= attackCooldown)
            {
                attackTimer = 0;
                isAttackPlaying = true;
                animator.SetTrigger(attack1Hash);
            }
        }
        else if (enemyStateMachine.CurrentState != EnemyStateMachine.EnemyState.Attacking)
        {
            attackTimer = 0;
        }
    }

    public void UpdateAnimation(EnemyStateMachine.EnemyState state)
    {
        // speed up the animator while chasing to look like running, reset otherwise
        animator.speed = (state == EnemyStateMachine.EnemyState.Chasing) ? chaseAnimationSpeed : 1f;

        switch (state)
        {
            case EnemyStateMachine.EnemyState.Attacking:
                // fire the first attack immediately, then let FixedUpdate
                // handle the cooldown for the next ones
                isAttackPlaying = true;
                attackTimer = 0;
                animator.SetTrigger(attack1Hash);
                break;
            case EnemyStateMachine.EnemyState.Hurt:
                // if hurt interrupted an attack, OnAttackEnds won't fire,
                // so clear the playing flag here so the cooldown can restart
                isAttackPlaying = false;
                attackTimer = 0;
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

    // rotate the sprite to match the facing direction, so the hitbox with animations is facing the right direction.
    // also rotate the healthbar canvas again to match the facing direction, so the healthbar is facing the right direction.
    public void FlipSprite()
    {
        if (enemyStateMachine.FacingDirection > 0)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
            healthbarCanvas.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 0, 0);
        }
        else if (enemyStateMachine.FacingDirection < 0)
        {
            transform.rotation = Quaternion.Euler(0, 180f, 0);
            healthbarCanvas.GetComponent<RectTransform>().localRotation = Quaternion.Euler(0, 180f, 0);

        }
    }

    // called by the attack animation event when it ends
    // clears the playing flag so FixedUpdate can start counting
    // the cooldown for the next attack
    public void OnAttackEnds()
    {
        isAttackPlaying = false;
        attackTimer = 0;
    }
}
