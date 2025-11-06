using UnityEngine;

public class Mostrar_panel : MonoBehaviour //nombre del archivo
{
    public GameObject panel_Enunciado;//nombre del panel
    public float distancia = 1f;

    private Transform jugador;

    void Start()
    {
        // Buscar el jugador automáticamente por tag
        GameObject jugadorGO = GameObject.FindGameObjectWithTag("Player");
        if (jugadorGO != null)
            jugador = jugadorGO.transform;
        else
            Debug.LogWarning("No se encontró un objeto con el tag 'Player'.");
    }

    void Update()
    {
        if (jugador == null) return;

        if (Vector3.Distance(transform.position, jugador.position) < distancia)
        {
            panel_Enunciado.SetActive(true);//cambiar al nuevo nombre del panel pero se cambia en el nuevo scripts
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            panel_Enunciado.SetActive(false);//cambiar al nuevo nombre del panel pero se cambia en el nuevo scripts
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}//OJO, ESTE VA EN EL OBJETO, YA SEA CUBO O DEMAS...... Y CUANDO LO AGREGUE SE PONE EL PANEL ARRIBA DEL PARAMETRO DE DISTANCIA DESPUES DE AGREGAR EL SCRIP AL OBJETO.