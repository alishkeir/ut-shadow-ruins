using System.Collections;
using UnityEngine;

public class Bat : MonoBehaviour
{
    [SerializeField] private float explosionDelay = 1f;
    [SerializeField] private float explosionDamage = 5f;

    [Header("Player Overlap Check")]
    [SerializeField] private float playerCheckRadius = 0.1f;
    // [SerializeField] private LayerMask playerCheckLayerMask;

    Animator animator;

    GameObject player;


    void Awake()
    {
        animator = GetComponent<Animator>();
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // start chasing the player
        // if we go into trigger enter 2d (we will create it, we check if it's player,)
        // if the collider is player, we check if the status is rolling or dashing, the bat will fire the explode animation
        // but won't damage the player, otherwise we damage the player
    }



    private void OnTriggerEnter2D(Collider2D other)
    {

        if (other.gameObject.CompareTag("Player"))
        {
            if (other.gameObject.TryGetComponent(out PlayerStateMachine playerStateMachine))
            {
                if (playerStateMachine.CurrentState == PlayerStateMachine.PlayerState.Dead) return;

                if (playerStateMachine.CurrentState == PlayerStateMachine.PlayerState.Rolling || playerStateMachine.CurrentState == PlayerStateMachine.PlayerState.Dashing)
                {
                    player = null;
                }
                else
                {
                    player = other.gameObject;
                }

                StartCoroutine(ExplodeAfterDelay());

            }

        }

        if (other.gameObject.TryGetComponent(out Health health))
        {
            health.TakeDamage(explosionDamage);
        }
    }

    private IEnumerator ExplodeAfterDelay()
    {
        yield return new WaitForSeconds(explosionDelay);
        animator.SetTrigger("Explode");
    }

    public void DealExplosionDamage()
    {

        // check if the collider overlaps with the player
        // if it does, deal damage

        if (Physics2D.OverlapCircle(transform.position, playerCheckRadius, LayerMask.GetMask("Player")) && player != null)
        {
            player.TryGetComponent(out Health health);
            health.TakeDamage(explosionDamage);
        }


    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, playerCheckRadius);
    }


}
