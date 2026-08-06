using UnityEngine;

public class Hitbox : MonoBehaviour
{
    [SerializeField] private float damage = 2f;
    [SerializeField] private float parryKnockbackX = 10f;
    [SerializeField] private float parryKnockbackY = 2f;


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.TryGetComponent(out Health health))
        {
            // check if the target is a player and if they're parrying
            if (other.TryGetComponent(out PlayerStateMachine playerStateMachine))
            {
                if (playerStateMachine.CurrentState == PlayerStateMachine.PlayerState.Parrying)
                {
                    // check if the hitbox owner is a boss
                    if (GetComponentInParent<BossController2D>() != null)
                    {
                        // boss: deal 50% damage and push the player back
                        health.TakeDamage(damage * 0.5f);

                        Rigidbody2D playerRb = other.GetComponent<Rigidbody2D>();
                        if (playerRb != null)
                        {
                            float dir = Mathf.Sign(other.transform.position.x - transform.position.x);
                            playerRb.AddForce(new Vector2(dir * parryKnockbackX, parryKnockbackY), ForceMode2D.Impulse);
                        }
                    }
                    // else: enemy hitbox, parry blocks fully - no damage
                    return;
                }
            }

            health.TakeDamage(damage);
        }
    }
}
