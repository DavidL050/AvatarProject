using UnityEngine;


public class SceneCleaner : MonoBehaviour
{
    void Awake()
    {

        PlayerMovement[] players = FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None);

        if (players.Length > 1)
        {
            Debug.Log($"[SceneCleaner] Se encontraron {players.Length} jugadores. Procediendo a limpiar el duplicado.");

            for (int i = 1; i < players.Length; i++)
            {
                 Debug.LogWarning($"[SceneCleaner] Destruyendo jugador fantasma: {players[i].gameObject.name}");
                 Destroy(players[i].gameObject);
            }
        }
        else
        {
            Debug.Log($"[SceneCleaner] Se encontró {players.Length} jugador. No se requiere limpieza.");
        }
    }
}