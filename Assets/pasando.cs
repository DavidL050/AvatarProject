using UnityEngine;


public class pasando : MonoBehaviour
{
    public Animator laPuerta;

    private void OnTriggerEnter(Collider other)
    {
        laPuerta.Play("Abrir");
    }

    private void OnTriggerExit(Collider other)
    {
        laPuerta.Play("Cerrar");

    }
}
