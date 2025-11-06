using UnityEngine;

public class pasando2 : MonoBehaviour
{
    public Animator laPuerta;

    private void OnTriggerEnter(Collider other)
    {
        laPuerta.Play("Abrir2");
    }

    private void OnTriggerExit(Collider other)
    {
        laPuerta.Play("Cerrar2");
    }
}
