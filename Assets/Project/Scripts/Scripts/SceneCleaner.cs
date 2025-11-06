using UnityEngine;
using StarterAssets; 

public class SceneCleaner : MonoBehaviour
{
    void Awake()
    {
        // Ahora el script sabrá qué es un ThirdPersonController
        ThirdPersonController[] players = FindObjectsOfType<ThirdPersonController>();
        
        Debug.Log($"[SceneCleaner] Se encontraron {players.Length} jugadores persistentes.");

        foreach (var player in players)
        {
            Debug.LogWarning($"[SceneCleaner] Destruyendo jugador fantasma: {player.gameObject.name}");
            Destroy(player.gameObject);
        }
    }
}