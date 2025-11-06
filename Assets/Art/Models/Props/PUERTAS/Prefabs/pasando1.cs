using UnityEngine;

public class pasando1 : MonoBehaviour
{
    public Animator laPuerta;

    private void OnTriggerEnter(Collider other)
    {
        laPuerta.Play("Abrir1");
    }
    private void OnTriggerExit(Collider other)
    {
        laPuerta.Play("Cerrar1");
    }
}
