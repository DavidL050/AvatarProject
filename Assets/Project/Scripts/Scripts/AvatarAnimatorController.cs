using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AvatarAnimationController : MonoBehaviour
{
    // --- CAMBIO 1: Referencias específicas para cada modelo ---
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
    /// Activa una animación en el Animator usando un Trigger.
    /// </summary>
    public void PlayAnimation(string triggerName)
    {
        Debug.Log($"🎬 Activando trigger: {triggerName}");
        
        // --- CAMBIO 2: Lógica de búsqueda inteligente ---
        Animator currentAnimator = null;

        // Primero, busca en el modelo masculino si está activo
        if (maleAvatar != null && maleAvatar.activeInHierarchy)
        {
            currentAnimator = maleAvatar.GetComponent<Animator>();
            Debug.Log("✓ Animator encontrado en el modelo MASCULINO activo.");
        }
        // Si no, busca en el modelo femenino si está activo
        else if (femaleAvatar != null && femaleAvatar.activeInHierarchy)
        {
            currentAnimator = femaleAvatar.GetComponent<Animator>();
            Debug.Log("✓ Animator encontrado en el modelo FEMENINO activo.");
        }

        // Si después de buscar en ambos, no se encontró NADA, mostrar error y detener.
        if (currentAnimator == null)
        {
            Debug.LogError("❌ No se encontró un Animator en NINGÚN modelo de avatar activo. Revisa las asignaciones en el Inspector.");
            return;
        }
        
        // El resto del código funciona igual, pero ahora con el Animator correcto.
        if (!currentAnimator.enabled || currentAnimator.runtimeAnimatorController == null)
        {
            Debug.LogError("❌ El Animator encontrado está deshabilitado o no tiene un Animator Controller.");
            return;
        }

        currentAnimator.SetTrigger(triggerName);
        
        Debug.Log($"✅ Trigger '{triggerName}' activado en el Animator correcto.");
    }
}