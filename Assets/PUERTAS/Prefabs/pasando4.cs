using UnityEngine;

public class pasando4 : MonoBehaviour
{
    public Animator laPuerta;

    private void OnTriggerEnter(Collider other)
    {
        laPuerta.Play("Abrir4");
    }
    private void OnTriggerExit(Collider other)
    {
        laPuerta.Play("Cerrar4");
    }
}
