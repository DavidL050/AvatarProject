// GameManager.cs - VERSIÓN CORREGIDA
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public AvatarData CurrentAvatarData { get; private set; }
    public string ActiveUserProfile { get; private set; }

    public RuntimeAnimatorController maleAnimatorOverride;
    public RuntimeAnimatorController femaleAnimatorOverride;
    public Avatar maleAvatarAsset;
    public Avatar femaleAvatarAsset;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LoginUser(string profileID)
    {
        if (string.IsNullOrEmpty(profileID))
        {
            Debug.LogError("El nombre de perfil no puede estar vacío.");
            return;
        }

        ActiveUserProfile = profileID;
        CurrentAvatarData = SaveSystem.LoadAvatar(ActiveUserProfile);
        Debug.Log("Sesión iniciada para el perfil: " + ActiveUserProfile);
    }

    public void SavePlayerData()
    {
        if (string.IsNullOrEmpty(ActiveUserProfile))
        {
            Debug.LogError("No hay ningún perfil de usuario activo. No se puede guardar.");
            return;
        }
        SaveSystem.SaveAvatar(CurrentAvatarData, ActiveUserProfile);
    }

    // ==========================================================
    // ======> ¡FUNCIÓN AÑADIDA QUE CORRIGE EL SEGUNDO ERROR! <=======
    // ==========================================================
    /// <summary>
    /// Actualiza los datos del avatar que están en memoria en el GameManager.
    /// </summary>
    public void SetCurrentAvatarData(AvatarData data)
    {
        CurrentAvatarData = data;
    }
}