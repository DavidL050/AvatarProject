using UnityEngine;
using Sunbox.Avatars;

[RequireComponent(typeof(AvatarCustomization))]
public class AvatarApplier : MonoBehaviour
{
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool positionFixed = false;
    
    void Awake()
    {
        // Guardar la posición original del avatar ANTES de cualquier modificación
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        Debug.Log($"Posición original del avatar guardada: {originalPosition}");
    }
    
    void Start()
    {
        Debug.Log("AvatarApplier: Buscando datos en GameManager...");
        
        if (GameManager.Instance == null)
        {
            Debug.LogError("¡GameManager no encontrado! No se puede aplicar la apariencia. Inicia desde el Menú Principal.");
            return;
        }

        AvatarData dataToApply = GameManager.Instance.CurrentAvatarData;
        
        if (dataToApply == null)
        {
            Debug.LogError("Los datos del avatar en GameManager son nulos. No se puede aplicar la apariencia.");
            return;
        }

        var customizer = GetComponent<AvatarCustomization>();
        
        if (customizer != null)
        {
            customizer.ApplyData(dataToApply);
            Debug.Log("¡Apariencia aplicada correctamente desde el GameManager!");
            
            // FORZAR que el avatar vuelva a su posición original
            Invoke(nameof(RestorePosition), 0.1f);
        }
        else
        {
            Debug.LogError("No se encontró el componente AvatarCustomization en el jugador.");
        }
    }
    
    private void RestorePosition()
    {
        if (!positionFixed)
        {
            transform.position = originalPosition;
            transform.rotation = originalRotation;
            positionFixed = true;
            Debug.Log("Posición del avatar restaurada");
        }
    }
}