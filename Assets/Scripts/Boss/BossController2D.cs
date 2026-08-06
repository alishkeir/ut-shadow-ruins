using System;
using UnityEngine;
using Random = UnityEngine.Random;



// this script controls the boss character
// it handles the flying movement and the summon bat attack
// the boss flies randomly inside a boundary and summons bats on a cooldown
// animation events on the boss clips call methods here (OnSummonBat, OnSummonEnd, etc)
public class BossController2D : MonoBehaviour
{
    [Header("Flying")]
    [SerializeField] private float flySpeed = 3f;
    [SerializeField] private Vector2 flyCenter = Vector2.zero;
    [SerializeField] private Vector2 flySize = new Vector2(20f, 10f);
    [SerializeField] private float flyPadding = 1f;
    [SerializeField] private float pickNewTargetDistance = 0.5f;

    [Header("Summon Bat")]
    [SerializeField] private float summonCooldown = 5f;
    [SerializeField] private float startDelay = 3f;
    [SerializeField] private int minBats = 2;
    [SerializeField] private int maxBats = 5;
    [SerializeField] private GameObject batPrefab;
    [SerializeField] private Transform batSpawnPoint;

    Rigidbody2D rb;
    BossStateMachine bossStateMachine;
    BossAnimationController animationController;

    Vector2 targetPosition;
    float summonTimer;
    bool isFirstAttack = true;
    int batsRemaining;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        bossStateMachine = GetComponent<BossStateMachine>();
        animationController = GetComponent<BossAnimationController>();

        // the boss flies so no gravity
        rb.gravityScale = 0f;
    }

    void Start()
    {
        // find the player by tag - the boss is always active in the arena
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            bossStateMachine.SetDetectedPlayer(player);

        PickNewTarget();
    }

    void FixedUpdate()
    {
        if (bossStateMachine.CurrentState == BossStateMachine.BossState.Dead) return;

        bossStateMachine.SetSpeed(rb.linearVelocity.magnitude);

        UpdateFacingDirection();

        switch (bossStateMachine.CurrentState)
        {
            case BossStateMachine.BossState.Flying:
                Fly();
                HandleSummonCooldown();
                break;
            // summoning, smashing, recovering, hurt - stay in place
            default:
                rb.linearVelocity = Vector2.zero;
                break;
        }
    }

    // fly toward the current target, pick a new one when we get close
    void Fly()
    {
        Vector2 direction = (targetPosition - rb.position).normalized;
        rb.linearVelocity = direction * flySpeed;

        if (Vector2.Distance(rb.position, targetPosition) < pickNewTargetDistance)
        {
            PickNewTarget();
        }
    }

    // pick a random point inside the fly boundary, with padding from the edges
    void PickNewTarget()
    {
        float halfWidth = flySize.x * 0.5f - flyPadding;
        float halfHeight = flySize.y * 0.5f - flyPadding;

        float randomX = flyCenter.x + UnityEngine.Random.Range(-halfWidth, halfWidth);
        float randomY = flyCenter.y + UnityEngine.Random.Range(-halfHeight, halfHeight);

        targetPosition = new Vector2(randomX, randomY);
    }

    // count up the summon cooldown and trigger the attack when it's ready
    // the first attack waits for startDelay instead of the full cooldown
    void HandleSummonCooldown()
    {
        summonTimer += Time.fixedDeltaTime;

        float threshold = isFirstAttack ? startDelay : summonCooldown;

        if (summonTimer >= threshold)
        {
            summonTimer = 0;
            isFirstAttack = false;
            // pick a random number of bats to summon this round
            batsRemaining = Random.Range(minBats, maxBats + 1);
            bossStateMachine.ChangeState(BossStateMachine.BossState.Summoning);
        }
    }

    // face the player so the summon/smash animations face the right way
    // the default sprite is facing left, so we need to flip it
    void UpdateFacingDirection()
    {
        if (rb.linearVelocityX > 0)
        {
            bossStateMachine.SetFacingDirection(-1);
        }
        else if (rb.linearVelocityX < 0)
        {
            bossStateMachine.SetFacingDirection(1);
        }
    }



    // called by the summon animation event to spawn one bat
    void OnSummonBat()
    {
        if (batPrefab == null || batSpawnPoint == null) return;

        Instantiate(batPrefab, batSpawnPoint.position, Quaternion.identity);
        batsRemaining--;
    }

    // called by the summon animation event when it ends
    // if there are more bats to summon, replay the summon animation
    // otherwise go back to flying
    void OnSummonEnd()
    {
        if (batsRemaining > 0)
        {
            animationController.ReplaySummon();
        }
        else
        {
            bossStateMachine.ForceChangeState(BossStateMachine.BossState.Flying);
        }
    }

    // called by the hurt animation event when it ends - go back to flying
    void OnHurtEnd()
    {
        bossStateMachine.ForceChangeState(BossStateMachine.BossState.Flying);
    }


    // draw the fly boundary in the editor so we can see it
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(flyCenter, flySize);
    }
}
