using UnityEngine;

public class PlayerHealZone : MonoBehaviour
{
    public int healAmount = 20; // Cantidad de vida que recupera

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Health playerHealth = other.GetComponent<Health>();

            if (playerHealth != null)
            {
                playerHealth.Heal(healAmount); // Aumentar la vida
                Debug.Log("¡Jugador curado con " + healAmount + " de vida!");

                Destroy(gameObject); // Destruir el objeto curativo
            }
        }
    }
}


