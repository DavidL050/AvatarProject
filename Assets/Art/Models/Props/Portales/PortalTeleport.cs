using UnityEngine;

public class PortalTeleport : MonoBehaviour
{
    public Transform destinationPortal; // Portal al que se teletransporta

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            CharacterController controller = other.GetComponent<CharacterController>();

            if (controller != null)
            {
                controller.enabled = false;
                other.transform.position = destinationPortal.position + destinationPortal.forward * 2f;
                controller.enabled = true;
            }
            else
            {
                other.transform.position = destinationPortal.position + destinationPortal.forward * 2f;
            }

            // Eliminar el portal de destino después de usarlo
            if (destinationPortal != null && destinationPortal.CompareTag("EliminarPortal"))
            {
                Debug.Log("¡Se eliminó el portal de llegada!");
                Destroy(destinationPortal.gameObject);
            }
        }
    }
}



