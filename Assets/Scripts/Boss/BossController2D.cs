using System;
using System.Collections;
using UnityEngine;


// this script controls the boss character
// it handles the flying movement, the summon bat attack, and the smash attack
// the boss flies randomly inside a boundary and attacks on a cooldown
// it randomly picks between summoning bats or smashing the ground
// animation events on the boss clips call methods here (OnSummonBat, OnSummonEnd, etc)
public class BossController2D : MonoBehaviour
{
    [Header("Flying")]
    [SerializeField] private float flySpeed = 3f;
    [SerializeField] private Vector2 flyCenter = Vector2.zero;
    [SerializeField] private Vector2 flySize = new Vector2(20f, 10f);
    [SerializeField] private float flyPadding = 1f;
    [SerializeField] private float pickNewTargetDistance = 0.5f;

    [Header("Attack")]
    [SerializeField] private float attackCooldown = 5f;
    [SerializeField] private float startDelay = 3f;

    [Header("Summon Bat")]
    [SerializeField] private int minBats = 2;
    [SerializeField] private int maxBats = 5;
    [SerializeField] private GameObject batPrefab;
    [SerializeField] private Transform batSpawnPoint;

    [Header("Smash")]
    [SerializeField] private float smashSpeed = 15f;
    [SerializeField] private float smashIdleTime = 2f;
    [SerializeField] private float smashEndDuration = 2f;
    [SerializeField] private float hitboxActiveTime = 1f;

    Rigidbody2D rb;
    BossStateMachine bossStateMachine;
    BossAnimationController animationController;
    CapsuleCollider2D capsuleCollider;
    Hitbox hitbox;

    Vector2 targetPosition;
    float attackTimer;
    bool isFirstAttack = true;
    int batsRemaining;

    // smash state
    Vector2 smashTarget;
    float smashIdleTimer;
    float smashEndTimer;
    float hitboxTimer;
    bool hitboxActive = false;


    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        bossStateMachine = GetComponent<BossStateMachine>();
        animationController = GetComponent<BossAnimationController>();
        capsuleCollider = GetComponent<CapsuleCollider2D>();
        hitbox = GetComponentInChildren<Hitbox>();

        // the boss flies so no gravity
        rb.gravityScale = 0f;

        // exclude the player layer by default so the boss doesn't collide
        // with the player while flying around
        capsuleCollider.excludeLayers = LayerMask.GetMask("Player");
    }

    void Start()
    {
        // find the player by tag - the boss is always active in the arena
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
            bossStateMachine.SetDetectedPlayer(player);

        PickNewTarget();
    }

    void OnEnable()
    {
        bossStateMachine.OnStateChanged += OnBossStateChanged;
    }

    void OnDisable()
    {
        bossStateMachine.OnStateChanged -= OnBossStateChanged;
    }

    // listen to state changes to handle layer and hitbox changes
    void OnBossStateChanged(BossStateMachine.BossState state)
    {
        if (state == BossStateMachine.BossState.SmashIdle)
        {
            // include the player layer so the player can attack the boss
            capsuleCollider.excludeLayers = 0;
            capsuleCollider.includeLayers = LayerMask.GetMask("Player");

            // enable the hitbox for a short time to damage the player
            if (hitbox != null)
            {
                hitbox.gameObject.GetComponent<BoxCollider2D>().enabled = true;
                hitboxActive = true;
                hitboxTimer = 0;
            }
        }
        else if (state == BossStateMachine.BossState.SmashRecover)
        {
            // exclude the player layer again
            capsuleCollider.excludeLayers = LayerMask.GetMask("Player");
            capsuleCollider.includeLayers = 0;

            // disable the hitbox
            hitboxActive = false;
            if (hitbox != null)
                hitbox.gameObject.GetComponent<BoxCollider2D>().enabled = false;
        }
        else if (state == BossStateMachine.BossState.Flying)
        {
            // exclude the player layer by default
            capsuleCollider.excludeLayers = LayerMask.GetMask("Player");
            capsuleCollider.includeLayers = 0;
        }
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
                HandleAttackCooldown();
                break;
            case BossStateMachine.BossState.Smashing:
                Smash();
                break;
            case BossStateMachine.BossState.SmashIdle:
                SmashIdle();
                break;
            case BossStateMachine.BossState.SmashRecover:
                SmashRecover();
                break;
            // summoning, hurt - stay in place
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

    // count up the attack cooldown and trigger a random attack when it's ready
    // the first attack waits for startDelay instead of the full cooldown
    void HandleAttackCooldown()
    {
        attackTimer += Time.fixedDeltaTime;

        float threshold = isFirstAttack ? startDelay : attackCooldown;

        if (attackTimer >= threshold)
        {
            attackTimer = 0;
            isFirstAttack = false;

            // randomly pick between summon and smash
            if (UnityEngine.Random.value > 0.5f)
            {
                StartSummon();
            }
            else
            {
                StartSmash();
            }
        }
    }

    void StartSummon()
    {
        // pick a random number of bats to summon this round
        batsRemaining = UnityEngine.Random.Range(minBats, maxBats + 1);
        bossStateMachine.ChangeState(BossStateMachine.BossState.Summoning);
    }

    void StartSmash()
    {
        // get the player's last known position on this frame
        // the boss will charge towards this spot and smash the ground
        if (bossStateMachine.DetectedPlayer != null)
        {
            smashTarget = bossStateMachine.DetectedPlayer.transform.position;
        }
        else
        {
            smashTarget = rb.position;
        }

        // set the velocity once here so the boss charges in a straight line
        Vector2 direction = (smashTarget - rb.position).normalized;
        rb.linearVelocity = direction * smashSpeed;

        bossStateMachine.ChangeState(BossStateMachine.BossState.Smashing);
    }

    // the boss is charging toward the smash target
    // the velocity was set once in StartSmash(), we just wait for the
    // ground collision which is handled in OnCollisionEnter2D
    void Smash()
    {
        // fallback: if the boss overshoots the target without hitting ground,
        // stop and enter smash idle
        if (Vector2.Distance(rb.position, smashTarget) < 0.5f)
        {
            rb.linearVelocity = Vector2.zero;
            smashIdleTimer = 0;
            bossStateMachine.ForceChangeState(BossStateMachine.BossState.SmashIdle);
        }
    }

    // called when the boss collides with something
    // if we're smashing, this is the ground - stop and enter smash idle
    void OnCollisionEnter2D(Collision2D collision)
    {
        if (bossStateMachine.CurrentState != BossStateMachine.BossState.Smashing) return;




        if (collision.gameObject.CompareTag("Ground"))
        {
            // switch to kinematic so the ground physics doesn't push the boss around
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.linearVelocity = Vector2.zero;

            // reset and transition to smash idle
            // (layer changes and hitbox are handled in OnBossStateChanged)
            smashIdleTimer = 0;
            bossStateMachine.ForceChangeState(BossStateMachine.BossState.SmashIdle);
        }


    }

    // the boss is on the ground idling
    // count the idle timer, disable the hitbox after hitboxActiveTime,
    // and after smashIdleTime transition to smash recover
    void SmashIdle()
    {
        rb.linearVelocity = Vector2.zero;

        // disable the hitbox after hitboxActiveTime
        if (hitboxActive)
        {
            hitboxTimer += Time.fixedDeltaTime;
            if (hitboxTimer >= hitboxActiveTime)
            {
                hitboxActive = false;
                if (hitbox != null)
                    // hitbox.gameObject.SetActive(false);
                    hitbox.gameObject.GetComponent<BoxCollider2D>().enabled = false;

            }
        }

        // after smashIdleTime, go to smash recover
        smashIdleTimer += Time.fixedDeltaTime;
        if (smashIdleTimer >= smashIdleTime)
        {
            smashEndTimer = 0;
            bossStateMachine.ForceChangeState(BossStateMachine.BossState.SmashRecover);
        }
    }

    // the boss is recovering - playing the smash end animation
    // stay in place, after smashEndDuration go back to flying (fallback)
    void SmashRecover()
    {
        rb.linearVelocity = Vector2.zero;

        // fallback: after smashEndDuration, go back to flying
        // in case the OnRecoverEnd animation event doesn't fire
        smashEndTimer += Time.fixedDeltaTime;
        if (smashEndTimer >= smashEndDuration)
        {
            // layer and hitbox changes are handled in OnBossStateChanged
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            PickNewTarget();
            bossStateMachine.ForceChangeState(BossStateMachine.BossState.Flying);
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


    // ---- summon animation event callbacks ----

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


    // ---- smash animation event callbacks ----

    // called by the smash end animation event when it ends
    // switch back to dynamic, pick a new target, go back to flying
    void OnRecoverEnd()
    {
        // layer and hitbox changes are handled in OnBossStateChanged
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        PickNewTarget();
        bossStateMachine.ForceChangeState(BossStateMachine.BossState.Flying);
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
