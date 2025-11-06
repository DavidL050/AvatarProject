// SaveAndContinue.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using Sunbox.Avatars; // ¡Bien hecho al añadir esta línea!

public class SaveAndContinue : MonoBehaviour
{
    // Arrastra aquí el objeto que tiene el script AvatarCustomization
    [SerializeField] private AvatarCustomization avatarCustomization;

    public void OnSaveAndContinueClicked()
    {
        if (avatarCustomization != null)
        {
            // CAMBIO: Ahora llamamos a la función correcta que sí existe.
            avatarCustomization.SaveDataAndContinue();
        }
        else
        {
            Debug.LogError("AvatarCustomization no está asignado en el Inspector de SaveAndContinue.");
        }
    }
}