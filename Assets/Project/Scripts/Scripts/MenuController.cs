using UnityEngine;
using UnityEngine.UI; // Necesario para controlar el componente Button
using TMPro;          // Necesario para controlar el componente TMP_InputField

public class MenuController : MonoBehaviour
{
    [Header("Referencias de la UI")]
    [Tooltip("Arrastra aquí el Input Field donde el usuario escribe su nombre.")]
    public TMP_InputField nameInputField;

    [Tooltip("Arrastra aquí el botón que quieres activar/desactivar.")]
    public Button playButton;

    
    void Start()
    {

        playButton.interactable = false;

        
        nameInputField.onValueChanged.AddListener(OnInputFieldValueChanged);
    }

    
    private void OnInputFieldValueChanged(string newText)
    {
    
        bool hasText = newText.Length > 0;

        playButton.interactable = hasText;
    }

   
    void OnDestroy()
    {
        nameInputField.onValueChanged.RemoveListener(OnInputFieldValueChanged);
    }
}