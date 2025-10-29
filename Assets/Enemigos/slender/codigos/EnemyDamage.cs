using UnityEngine;

public class EnemyDamage : MonoBehaviour, IDamageSource
{
    public int damageAmount = 10;
    public float damageCooldown = 1.5f;
    private float lastAttackTime = 0f;

    void OnTriggerEnter(Collider other) // ✅ trigger en vez de collision
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("¡Enemigo tocó al jugador!");
            ApplyDamage(other.gameObject);
        }
    }

    void ApplyDamage(GameObject player)
    {
        if (Time.time >= lastAttackTime + damageCooldown)
        {
            Health playerHealth = player.GetComponent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damageAmount);
                lastAttackTime = Time.time;
            }
        }
    }

    public int GetDamageAmount() => damageAmount;
    public float GetCooldown() => damageCooldown;
}


