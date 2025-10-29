using UnityEngine;
using UnityEngine.SceneManagement;

public class AvatarCarrier : MonoBehaviour
{
    public static AvatarCarrier I;          // Instancia global (singleton)
    public GameObject AvatarInstance;       // Aquí guardamos el avatar personalizado

    void Awake()
    {
        // Si ya existe otro Carrier, destruir este
        if (I != null)
        {
            Destroy(gameObject);
            return;
        }

        I = this;
        DontDestroyOnLoad(gameObject);

        // Suscribir evento: cada vez que se carga una escena nueva
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    /// <summary>
    /// Adjunta el avatar y lo marca como persistente entre escenas.
    /// </summary>
    public void Attach(GameObject avatarRoot)
    {
        AvatarInstance = avatarRoot;
        DontDestroyOnLoad(AvatarInstance);
    }

    /// <summary>
    /// Cuando se carga una escena nueva, reposiciona el avatar en el SpawnPoint.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (AvatarInstance == null) return;

        // Busca el objeto con la tag "SpawnPoint"
        GameObject spawn = GameObject.FindWithTag("SpawnPoint");
        if (spawn != null)
        {
            AvatarInstance.transform.SetPositionAndRotation(
                spawn.transform.position,
                spawn.transform.rotation
            );
        }
        else
        {
            Debug.LogWarning("[AvatarCarrier] No se encontró un objeto con tag 'SpawnPoint' en la escena.");
        }
    }
}
