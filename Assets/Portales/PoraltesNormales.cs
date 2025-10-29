using UnityEngine;

public class PoraltesNormales : MonoBehaviour
{
    public Transform destinationPortal; // El otro portal

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CharacterController controller = other.GetComponent<CharacterController>();

            if (controller != null)
            {
                // Para evitar glitches de colisión
                controller.enabled = false;
                other.transform.position = destinationPortal.position + destinationPortal.forward * 2f;
                controller.enabled = true;
            }
            else
            {
                // Si no usa CharacterController
                other.transform.position = destinationPortal.position + destinationPortal.forward * 2f;
            }
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}
