// PlayerAvatarAnimator.cs

using UnityEngine;
using UnityEngine.InputSystem; // ¡Importante! Usamos el nuevo Input System

public class PlayerAvatarAnimator : MonoBehaviour
{
    [Header("Input Action")]
    [Tooltip("La acción de input del joystick para el movimiento (normalmente el izquierdo)")]
    public InputActionProperty moveAction;

    [Header("Referencias")]
    [Tooltip("El Animator del avatar activo. AvatarLoader se encargará de asignarlo.")]
    public Animator activeAnimator;

    void Update()
    {
        // Si no tenemos un animator que controlar, no hacemos nada.
        if (activeAnimator == null) return;

        // 1. Leemos el valor del joystick de VR (un Vector2, de -1 a 1)
        Vector2 inputAxis = moveAction.action.ReadValue<Vector2>();

        // 2. Enviamos esos valores a los parámetros "MoveX" y "MoveY" del Animator
        activeAnimator.SetFloat("MoveX", inputAxis.x);
        activeAnimator.SetFloat("MoveY", inputAxis.y);
    }
}