using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{

    [SerializeField] private float health = 5f;
    [SerializeField] private float destroyDelay = 3f;
    [SerializeField] private Image healthBar;
    [SerializeField] private GameObject healthbarCanvas;

    private float currentHealth;

    private bool isDead = false;


    PlayerStateMachine playerStateMachine;
    EnemyStateMachine enemyStateMachine;

    void Awake()
    {
        if (TryGetComponent(out PlayerStateMachine pStateMachine))
        {
            playerStateMachine = pStateMachine;
        }
        else if (TryGetComponent(out EnemyStateMachine eStateMachine))
        {
            enemyStateMachine = eStateMachine;
        }
    }

    void Start()
    {
        currentHealth = health;

        UpdateHealthBar();
    }

    public void TakeDamage(float damage)
    {
        currentHealth -= damage;
        UpdateHealthBar();


        if (isDead) return;

        TriggerHurt();

        if (currentHealth <= 0)
        {
            TriggerDeath();
        }

    }

    void UpdateHealthBar()
    {
        if (healthBar == null) return;


        healthBar.fillAmount = currentHealth / health;
    }

    void TriggerHurt()
    {
        if (playerStateMachine != null)
        {
            playerStateMachine.ChangeState(PlayerStateMachine.PlayerState.Hurt);

        }
        else if (enemyStateMachine != null)
        {
            enemyStateMachine.ChangeState(EnemyStateMachine.EnemyState.Hurt);
        }
    }

    void TriggerDeath()
    {
        if (isDead) return;
        isDead = true;

        if (playerStateMachine != null)
        {
            playerStateMachine.ChangeState(PlayerStateMachine.PlayerState.Dead);

        }
        else if (enemyStateMachine != null)
        {
            enemyStateMachine.ChangeState(EnemyStateMachine.EnemyState.Dead);

            // make the body kinematic so it stays in place and doesn't fall underground
            Rigidbody2D rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.bodyType = RigidbodyType2D.Kinematic;
            }


            // disable only trigger colliders (detection/attack points) so they can't fire anymore
            // keep solid (non-trigger) colliders enabled so the body stays on the ground
            Collider2D[] colliders = GetComponentsInChildren<Collider2D>();
            foreach (Collider2D col in colliders)
            {
                if (col.isTrigger)
                {
                    col.enabled = false;
                }
                else
                {
                    col.excludeLayers = LayerMask.GetMask("Player", "Enemy");
                }
            }

            if (healthbarCanvas != null)
                healthbarCanvas.gameObject.SetActive(false);



            // play the death animation, then destroy the enemy after a delay
            StartCoroutine(DestroyAfterDelay());
        }
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(destroyDelay);
        Destroy(gameObject);
    }
}