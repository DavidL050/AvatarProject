// UISliderIdentifier.cs

using UnityEngine;

// Define los posibles "tipos" de sliders que puedes tener.
public enum SliderType { Generic, BodyFat, BodyMuscle, BreastSize } // Añade más si los necesitas

public class UISliderIdentifier : MonoBehaviour
{
    // Esta variable pública nos dirá qué controla este slider.
    public SliderType sliderID;
}