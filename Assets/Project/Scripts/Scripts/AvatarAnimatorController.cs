using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AvatarAnimationController : MonoBehaviour
{
    [Header("Configuración del Avatar")]
    
    public Transform avatarParent; 

    [Header("UI del Panel de Animación")]
    public GameObject animationPanel;
    public Transform buttonsContainer;
    public GameObject animationButtonTemplate;

    [System.Serializable]
    public struct AnimationData
    {
        public string animationName;
        public string animatorStateName;
    }

    public AnimationData[] availableAnimations;

    void Start()
    {
        animationPanel.SetActive(false);
        animationButtonTemplate.SetActive(false);
        PopulateAnimationButtons();
    }

    void PopulateAnimationButtons()
    {
        foreach (var animData in availableAnimations)
        {
            GameObject newButtonObj = Instantiate(animationButtonTemplate, buttonsContainer);
            newButtonObj.SetActive(true);

            TMP_Text buttonText = newButtonObj.GetComponentInChildren<TMP_Text>();
            if (buttonText != null)
            {
                buttonText.text = animData.animationName;
            }

            Button buttonComponent = newButtonObj.GetComponent<Button>();
            if (buttonComponent != null)
            {
                buttonComponent.onClick.AddListener(() => PlayAnimation(animData.animatorStateName));
            }
        }
    }

  
    public void PlayAnimation(string stateName)
    {
       
        Animator currentAnimator = avatarParent.GetComponentInChildren<Animator>();

        if (currentAnimator != null)
        {
          
            currentAnimator.Play(stateName);
        }
        else
        {
            Debug.LogError("¡No se encontró ningún Animator activo dentro de 'avatarParent'!");
        }
    }
}