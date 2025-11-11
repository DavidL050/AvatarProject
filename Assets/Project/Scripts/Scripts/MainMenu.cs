using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public TMP_InputField profileNameInputField;
    public Button loginButton;



    void Start()
    {
        // Configuración inicial de la UI
        loginButton.interactable = false;
        profileNameInputField.onValueChanged.AddListener(CheckInput);
    }

    private void CheckInput(string inputText)
    {
        // Activa el botón solo si hay texto en el campo de entrada
        loginButton.interactable = !string.IsNullOrEmpty(inputText);
    }

    public void OnLoginClicked()
    {
        string profileName = profileNameInputField.text;
        Debug.Log("Botón 'Jugar' presionado.");
        Debug.Log("Intentando iniciar sesión con el perfil: '" + profileName + "'");

        // Guarda el perfil y carga los datos del avatar
        GameManager.Instance.LoginUser(profileName);

        // Comprueba si el usuario ya tiene un archivo de guardado
        string saveFilePath = SaveSystem.GetSavePath(profileName);
        Debug.Log("Buscando archivo en la ruta: " + saveFilePath);
        Debug.Log("¿El archivo existe? -> " + File.Exists(saveFilePath));

        if (File.Exists(saveFilePath))
        {
            // Si el usuario existe, va directamente a la escena de juego
            Debug.Log("Resultado: El archivo existe. Cargando escena Gameplay.");
            SceneManager.LoadScene("Proyecto_General");
        }
        else
        {
            // Si es un usuario nuevo, va a la escena de personalización
            Debug.Log("Resultado: El archivo NO existe. Cargando escena de Personalización.");
            SceneManager.LoadScene("Avatars_EditorScene");
        }
    }
}