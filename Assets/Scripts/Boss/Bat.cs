using System.Collections;
using UnityEngine;


// this script handles the bat that the boss spawns
// the bat flies toward the player and explodes on contact
// if the player is rolling or dashing, the bat explodes without damaging
// the player can also hit the bat with their attack hitbox to make it explode early
// if the bat lives too long (lifetime), it just disappears on its own without damaging anything
public class Bat : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float explosionDamage = 5f;
    [SerializeField] private float lifetime = 4f;

    [Header("Player Overlap Check")]
    [SerializeField] private float playerCheckRadius = 0.2f;
    [SerializeField] private LayerMask playerCheckLayerMask;

    Animator animator;
    Rigidbody2D rb;

    GameObject player;
    bool isExploding = false;


    void Awake()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();

        // the bat flies so no gravity
        rb.gravityScale = 0f;
        playerCheckLayerMask = LayerMask.GetMask("Player");
    }

    void Start()
    {
        // find the player by tag so we know who to chase
        player = GameObject.FindGameObjectWithTag("Player");

        // start the lifetime countdown - the bat just disappears after this
        StartCoroutine(LifetimeCountdown());
    }

    void FixedUpdate()
    {
        if (isExploding) return;
        if (player == null) return;

        // fly straight toward the player
        Vector2 direction = ((Vector2)player.transform.position - rb.position).normalized;
        rb.linearVelocity = direction * moveSpeed;
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isExploding) return;

        // the player's attack hitbox touched the bat - explode without damaging the player
        if (other.TryGetComponent(out Hitbox hitbox))
        {
            Explode();
            return;
        }

        // the bat touched the player - check if we can damage them
        if (other.gameObject.CompareTag("Player"))
        {
            if (other.TryGetComponent(out PlayerStateMachine playerStateMachine))
            {
                // dead player can't be damaged
                if (playerStateMachine.CurrentState == PlayerStateMachine.PlayerState.Dead) return;

                // if the player is rolling or dashing, the bat explodes but deals no damage
                if (playerStateMachine.CurrentState == PlayerStateMachine.PlayerState.Rolling ||
                    playerStateMachine.CurrentState == PlayerStateMachine.PlayerState.Dashing)
                {
                    Explode();
                    return;
                }
            }

            // player is vulnerable - explode and damage them
            // keep the player reference so DealExplosionDamage can hurt them
            if (!isExploding)
            {
                isExploding = true;
                rb.linearVelocity = Vector2.zero;
                animator.SetTrigger("Explode");
            }
        }
    }


    // explode immediately without damaging the player
    // used when the player's hitbox hits the bat or when the player is rolling/dashing
    public void Explode()
    {
        if (isExploding) return;

        isExploding = true;
        rb.linearVelocity = Vector2.zero;
        player = null; // clear the player so DealExplosionDamage won't damage them

        animator.SetTrigger("Explode");
    }

    // called by the explosion animation event to deal damage
    // only damages if the player is still in range and wasn't cleared
    public void DealExplosionDamage()
    {
        if (player == null) return;

        // check if the player is still overlapping the bat
        if (Physics2D.OverlapCircle(transform.position, playerCheckRadius, playerCheckLayerMask))
        {


            if (player.TryGetComponent(out Health health))
            {

                if (player.TryGetComponent(out PlayerStateMachine playerStateMachine))
                {
                    if (playerStateMachine.CurrentState == PlayerStateMachine.PlayerState.Dead
                    || playerStateMachine.CurrentState == PlayerStateMachine.PlayerState.Rolling
                    || playerStateMachine.CurrentState == PlayerStateMachine.PlayerState.Dashing
                    || playerStateMachine.CurrentState == PlayerStateMachine.PlayerState.Parrying
                    ) { return; }
                    else
                    {
                        health.TakeDamage(explosionDamage);
                    }
                }

            }
        }
    }

    // called by the explosion animation event when it ends - destroy the bat
    public void OnExplosionEnd()
    {
        Destroy(gameObject);
    }

    // if the bat lives too long, it just disappears on its own without damaging anything
    private IEnumerator LifetimeCountdown()
    {
        yield return new WaitForSeconds(lifetime);

        if (!isExploding)
        {
            Destroy(gameObject);
        }
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, playerCheckRadius);
    }
}
