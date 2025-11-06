// AvatarApplier.cs
using UnityEngine;
using Sunbox.Avatars; // Asegúrate de que esto coincide con el namespace de AvatarCustomization

[RequireComponent(typeof(AvatarCustomization))]
public class AvatarApplier : MonoBehaviour
{
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

        // Obtenemos el componente que sabe cómo aplicar la apariencia
        var customizer = GetComponent<AvatarCustomization>();
        if (customizer != null)
        {
            // Le pasamos los datos y dejamos que él haga todo el trabajo
            customizer.ApplyData(dataToApply);
            Debug.Log("¡Apariencia aplicada correctamente desde el GameManager!");
        }
        else
        {
            Debug.LogError("No se encontró el componente AvatarCustomization en el jugador.");
        }
    }
}