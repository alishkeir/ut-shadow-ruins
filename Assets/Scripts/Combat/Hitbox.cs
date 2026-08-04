using UnityEngine;

public class Hitbox : MonoBehaviour
{
    [SerializeField] private float damage = 2f;


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.TryGetComponent(out Health health))
        {
            health.TakeDamage(damage);
        }

        Debug.Log("Hit!");
        Debug.Log(other.gameObject.name);
    }
}
