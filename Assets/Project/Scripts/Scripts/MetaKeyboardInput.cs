using UnityEngine;
using TMPro;

public class VRKeyboardForSimulator : MonoBehaviour
{
    private TMP_InputField inputField;
    private bool isSelected = false;
    private TouchScreenKeyboard mobileKeyboard;

    void Start()
    {
        inputField = GetComponent<TMP_InputField>();
        
        if (inputField != null)
        {
            inputField.onSelect.AddListener(OnInputFieldSelected);
            inputField.onDeselect.AddListener(OnInputFieldDeselected);
        }
        else
        {
            Debug.LogError("No se encontró TMP_InputField en este GameObject!");
        }
    }

    void OnInputFieldSelected(string text)
    {
        isSelected = true;
        Debug.Log("✓ InputField seleccionado");
        
        // Forzar la apertura del teclado
        OpenKeyboard();
    }

    void OnInputFieldDeselected(string text)
    {
        isSelected = false;
        Debug.Log("✗ InputField deseleccionado");
    }

    void OpenKeyboard()
    {
        #if UNITY_ANDROID && !UNITY_EDITOR
        // Método 1: TouchScreenKeyboard (estándar de Unity)
        if (mobileKeyboard == null || !mobileKeyboard.active)
        {
            Debug.Log("Abriendo teclado de Quest...");
            
            mobileKeyboard = TouchScreenKeyboard.Open(
                inputField.text,                    // Texto inicial
                TouchScreenKeyboardType.Default,    // Tipo de teclado
                false,                              // Autocorrection
                false,                              // Multiline
                false,                              // Secure (password)
                false,                              // Alert
                "",                                 // Placeholder
                0                                   // Character limit (0 = sin límite)
            );
            
            if (mobileKeyboard != null)
            {
                Debug.Log("✓ Teclado abierto!");
            }
            else
            {
                Debug.LogError("✗ Error: No se pudo abrir el teclado");
            }
        }
        #else
        Debug.Log("Modo Editor: Escribe con tu teclado PC");
        #endif
    }

    void Update()
    {
        // EN EL EDITOR: Permite escribir con el teclado del PC
        #if UNITY_EDITOR
        if (isSelected)
        {
            if (Input.anyKeyDown)
            {
                if (Input.GetKeyDown(KeyCode.Backspace) && inputField.text.Length > 0)
                {
                    inputField.text = inputField.text.Substring(0, inputField.text.Length - 1);
                }
                else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
                {
                    inputField.DeactivateInputField();
                    Debug.Log("Texto final: " + inputField.text);
                }
                else if (Input.inputString.Length > 0)
                {
                    inputField.text += Input.inputString;
                }
            }
        }
        #endif

        // EN QUEST: Monitorea el teclado
        #if UNITY_ANDROID && !UNITY_EDITOR
        if (mobileKeyboard != null)
        {
            // Actualiza el texto en tiempo real
            if (mobileKeyboard.active)
            {
                inputField.text = mobileKeyboard.text;
            }
            
            // Detecta cuando se cierra
            if (mobileKeyboard.status == TouchScreenKeyboard.Status.Done)
            {
                inputField.text = mobileKeyboard.text;
                inputField.DeactivateInputField();
                mobileKeyboard = null;
                Debug.Log("✓ Texto final: " + inputField.text);
            }
            else if (mobileKeyboard.status == TouchScreenKeyboard.Status.Canceled)
            {
                inputField.DeactivateInputField();
                mobileKeyboard = null;
                Debug.Log("✗ Teclado cancelado");
            }
            else if (mobileKeyboard.status == TouchScreenKeyboard.Status.LostFocus)
            {
                mobileKeyboard = null;
                Debug.Log("⚠ Teclado perdió el foco");
            }
        }
        #endif
    }

    void OnDestroy()
    {
        if (inputField != null)
        {
            inputField.onSelect.RemoveListener(OnInputFieldSelected);
            inputField.onDeselect.RemoveListener(OnInputFieldDeselected);
        }
    }
}