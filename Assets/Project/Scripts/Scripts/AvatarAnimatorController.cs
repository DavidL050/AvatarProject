using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AvatarAnimationController : MonoBehaviour
{
    [Header("Configuración del Avatar")]
    public GameObject maleAvatar;
    public GameObject femaleAvatar;

    [Header("UI del Panel de Animación")]
    public GameObject animationPanel;
    public Transform buttonsContainer;
    public GameObject animationButtonTemplate;

    [System.Serializable]
    public struct AnimationData
    {
        public string animationName;
        public string animatorStateName; // Nombre del TRIGGER en el Animator
    }

    public AnimationData[] availableAnimations;

    void Start()
    {
        if (animationPanel != null)
            animationPanel.SetActive(false);
            
        if (animationButtonTemplate != null)
            animationButtonTemplate.SetActive(false);
            
        PopulateAnimationButtons();
        
        Debug.Log("🎮 AvatarAnimationController iniciado con el script CORRECTO.");
    }

    public void OpenAnimationPanel()
    {
        if (animationPanel != null)
        {
            animationPanel.SetActive(true);
        }
    }
    
    public void CloseAnimationPanel()
    {
        if (animationPanel != null)
        {
            animationPanel.SetActive(false);
        }
    }
    
    public void ToggleAnimationPanel()
    {
        if (animationPanel != null)
        {
            animationPanel.SetActive(!animationPanel.activeSelf);
        }
    }

    void PopulateAnimationButtons()
    {
        if (buttonsContainer == null || animationButtonTemplate == null)
        {
            Debug.LogError("❌ Buttons Container o Animation Button Template no están asignados");
            return;
        }

        foreach (var animData in availableAnimations)
        {
            GameObject newButtonObj = Instantiate(animationButtonTemplate, buttonsContainer);
            newButtonObj.SetActive(true);

            var buttonText = newButtonObj.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                buttonText.text = animData.animationName;
            }

            var buttonComponent = newButtonObj.GetComponent<Button>();
            if (buttonComponent != null)
            {
                string triggerName = animData.animatorStateName;
                buttonComponent.onClick.RemoveAllListeners();
                buttonComponent.onClick.AddListener(() => PlayAnimation(triggerName));
            }
        }
    }

    /// <summary>
    /// Activa las animaciones de los botones (Bailar, Saludar, etc.).
    /// </summary>
    public void PlayAnimation(string triggerName)
    {
        Debug.Log($"🎬 Activando trigger de UI: {triggerName}");
        
        Animator currentAnimator = GetActiveAnimator();
        if (currentAnimator == null) return;
        
        if (!currentAnimator.enabled || currentAnimator.runtimeAnimatorController == null)
        {
            Debug.LogError("❌ El Animator encontrado está deshabilitado o no tiene un Animator Controller.");
            return;
        }

        currentAnimator.SetTrigger(triggerName);
        
        Debug.Log($"✅ Trigger '{triggerName}' activado en el Animator correcto.");
    }

    // --- ¡ESTA ES LA NUEVA FUNCIÓN AÑADIDA! ---
    /// <summary>
    /// Activa la animación de cuerpo completo para agarrar un objeto.
    /// Esta función será llamada por el VRHandController.
    /// </summary>
    public void TriggerGrabAnimation()
    {
        Animator currentAnimator = GetActiveAnimator();
        if (currentAnimator == null) 
        {
            Debug.LogError("❌ No se encontró Animator para la animación de agarre.");
            return;
        }

        // Activamos el trigger que creaste en el Animator ("GrabObjectTrigger")
        Debug.Log($"🏃‍♂️ Activando trigger de animación de agarre: GrabObjectTrigger");
        currentAnimator.SetTrigger("GrabObjectTrigger");
    }

    /// <summary>
    /// Función de ayuda para encontrar el Animator del avatar activo (masculino o femenino).
    /// </summary>
    /// <returns>El componente Animator activo, o null si no se encuentra.</returns>
    private Animator GetActiveAnimator()
    {
        // Busca en el modelo masculino si está activo
        if (maleAvatar != null && maleAvatar.activeInHierarchy)
        {
            return maleAvatar.GetComponent<Animator>();
        }
        // Si no, busca en el modelo femenino si está activo
        else if (femaleAvatar != null && femaleAvatar.activeInHierarchy)
        {
            return femaleAvatar.GetComponent<Animator>();
        }

        // Si no se encontró ninguno, muestra un error.
        Debug.LogError("❌ No se encontró un modelo de avatar activo (ni masculino ni femenino).");
        return null;
    }
}