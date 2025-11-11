// SaveAndContinue.cs
using UnityEngine;
using UnityEngine.SceneManagement;
using Sunbox.Avatars; // ¡Bien hecho al añadir esta línea!

public class SaveAndContinue : MonoBehaviour
{

    [SerializeField] private AvatarCustomization avatarCustomization;

    public void OnSaveAndContinueClicked()
    {
        if (avatarCustomization != null)
        {
       
            avatarCustomization.SaveDataAndContinue();
        }
        else
        {
            Debug.LogError("AvatarCustomization no está asignado en el Inspector de SaveAndContinue.");
        }
    }
}