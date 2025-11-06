using UnityEngine;

public class pasando6: MonoBehaviour
{
    public Animator laPuerta;

    private void OnTriggerEnter(Collider other)
    {
        laPuerta.Play("Abrir6");
    }

    private void OnTriggerExit(Collider other)
    {
        laPuerta.Play("Cerrar6");
    }
}
