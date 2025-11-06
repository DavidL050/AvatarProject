using UnityEngine;
using UnityEngine.UI;

public class HealthBarUI : MonoBehaviour
{
    public Slider slider;
    public Health playerHealth;

    void Start()
    {
        if (playerHealth != null)
        {
            slider.maxValue = playerHealth.maxHealth;
            slider.value = playerHealth.CurrentHealth;
        }
    }

    void Update()
    {
        if (playerHealth != null)
        {
            slider.value = playerHealth.CurrentHealth;
        }
    }
}
