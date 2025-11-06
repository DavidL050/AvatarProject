using UnityEngine;
using System.Collections;

public class Health : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;

    public Transform respawnPoint;
    private CharacterController characterController;

    void Start()
    {
        currentHealth = maxHealth;
        characterController = GetComponent<CharacterController>();

        if (characterController == null)
        {
            Debug.LogWarning("CharacterController no encontrado en el objeto.");
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("Vida restante: " + currentHealth);

        if (currentHealth <= 0)
        {
            StartCoroutine(RespawnCoroutine());
        }
    }

    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        Debug.Log("¡Jugador curado! Vida actual: " + currentHealth);
    }

    IEnumerator RespawnCoroutine()
    {
        Debug.Log("¡Jugador ha muerto!");

        characterController.enabled = false;
        transform.position = new Vector3(0, -1000, 0);
        yield return null;

        transform.position = respawnPoint.position;
        yield return null;

        characterController.enabled = true;
        characterController.Move(Vector3.zero);

        currentHealth = maxHealth;
        Debug.Log("Jugador reaparecido en: " + transform.position);
    }

    public int CurrentHealth => currentHealth;
}




