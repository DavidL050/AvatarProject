using UnityEngine;
using Unity.XR.CoreUtils;

public class EditorAvatarPositioner : MonoBehaviour
{
    [SerializeField] private Transform playerSpawnPoint; // Para el XROrigin
    [SerializeField] private Transform avatarSpawnPoint; // Para el avatar
    [SerializeField] private GameObject avatarToPosition; // El avatar personalizable
    
    void Start()
    {
        PositionXROrigin();
        PositionAvatar();
    }
    
    private void PositionXROrigin()
    {
        XROrigin xrOrigin = UnityEngine.Object.FindFirstObjectByType<XROrigin>();
        
        if (xrOrigin != null && playerSpawnPoint != null)
        {
            xrOrigin.transform.position = playerSpawnPoint.position;
            xrOrigin.transform.rotation = playerSpawnPoint.rotation;
            Debug.Log("Jugador de VR (XROrigin) teletransportado al SpawnPoint.");
        }
        else
        {
            Debug.LogError("No se encontró XROrigin o PlayerSpawnPoint");
        }
    }
    
    private void PositionAvatar()
    {
        if (avatarToPosition != null && avatarSpawnPoint != null)
        {
            avatarToPosition.transform.position = avatarSpawnPoint.position;
            avatarToPosition.transform.rotation = avatarSpawnPoint.rotation;
            Debug.Log("Avatar posicionado en el pedestal correctamente");
        }
        else
        {
            Debug.LogWarning("No se asignó el Avatar o AvatarSpawnPoint en el Inspector");
        }
    }
}