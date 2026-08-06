using UnityEngine;


// this script controls the boss animations
// it listens to the boss state machine and plays the right animation
// animation events on the boss clips call methods on BossController2D
// (OnSummonBat, OnSmashImpact, OnRecoverEnd) to do the actual logic
public class BossAnimationController : MonoBehaviour
{

    private static readonly int speedHash = Animator.StringToHash("Speed");
    private static readonly int smashHash = Animator.StringToHash("Smash");
    private static readonly int summonHash = Animator.StringToHash("Summon");
    private static readonly int recoverHash = Animator.StringToHash("Recover");
    private static readonly int hurtHash = Animator.StringToHash("Hurt");
    private static readonly int dieHash = Animator.StringToHash("Die");

    BossStateMachine bossStateMachine;
    Animator animator;

    void Awake()
    {
        bossStateMachine = GetComponent<BossStateMachine>();
        animator = GetComponent<Animator>();
    }

    void OnEnable()
    {
        bossStateMachine.OnStateChanged += UpdateAnimation;
    }

    void OnDisable()
    {
        bossStateMachine.OnStateChanged -= UpdateAnimation;
    }

    void FixedUpdate()
    {
        if (bossStateMachine.CurrentState == BossStateMachine.BossState.Dead) return;

        // feed the speed so the animator can blend between idle and flying
        animator.SetFloat(speedHash, Mathf.Abs(bossStateMachine.Speed));

        FlipSprite();
    }

    void UpdateAnimation(BossStateMachine.BossState state)
    {
        switch (state)
        {
            case BossStateMachine.BossState.Smashing:
                animator.SetTrigger(smashHash);
                break;

            case BossStateMachine.BossState.Summoning:
                animator.SetTrigger(summonHash);
                break;

            case BossStateMachine.BossState.Recovering:
                animator.SetTrigger(recoverHash);
                break;

            case BossStateMachine.BossState.Hurt:
                animator.ResetTrigger(smashHash);
                animator.ResetTrigger(summonHash);
                animator.SetTrigger(hurtHash);
                break;

            case BossStateMachine.BossState.Dead:
                animator.ResetTrigger(smashHash);
                animator.ResetTrigger(summonHash);
                animator.ResetTrigger(hurtHash);
                animator.SetTrigger(dieHash);
                break;
        }
    }

    // replay the summon animation without going through the state machine
    // called by BossController2D when there are more bats to summon
    public void ReplaySummon()
    {
        animator.SetTrigger(summonHash);
    }

    // flip the sprite to match the facing direction
    public void FlipSprite()
    {
        if (bossStateMachine.FacingDirection > 0)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);

        }
        else if (bossStateMachine.FacingDirection < 0)
        {
            transform.rotation = Quaternion.Euler(0, 180f, 0);

        }
    }
}