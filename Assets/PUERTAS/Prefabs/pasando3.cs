using UnityEngine;

public class pasando3 : MonoBehaviour
{
    public Animator laPuerta;

    private void OnTriggerEnter(Collider other)
    {
        laPuerta.Play("Abrir3");
    }

    private  void OnTriggerExit(Collider other)
    {
        laPuerta.Play("Cerrar3");
    }
}
