using UnityEngine;

// Versión Corregida con Snap Inicial
public class ThirdPersonVRCamera : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Arrastra aquí el objeto del avatar que la cámara debe seguir.")]
    public Transform target;

    [Header("Configuración de Seguimiento")]
    [Tooltip("Qué tan rápido la cámara se pone al día con el movimiento del avatar. Valores más altos son más rápidos.")]
    [Range(1f, 20f)]
    public float followSmoothness = 8f;

    [Header("Configuración de Rotación Orbital")]
    [Tooltip("Qué tan rápido orbita la cámara alrededor del avatar con el joystick.")]
    public float rotationSpeed = 100f;

    // --- ¡NUEVO MÉTODO AÑADIDO! ---
    void Start()
    {
        // Esto soluciona el problema del inicio.
        // Mueve instantáneamente el rig de la cámara a la posición del avatar
        // en el primer frame, para que empiecen en el mismo sitio.
        if (target != null)
        {
            transform.position = target.position;
        }
    }

    void LateUpdate()
    {
        if (target == null)
        {
            Debug.LogError("¡ERROR! El 'Target' del script de la cámara no está asignado.");
            return;
        }

        // --- 1. SEGUIMIENTO SUAVE DE LA POSICIÓN ---
        // (Esta parte sigue igual y funcionará perfectamente después del Start)
        transform.position = Vector3.Lerp(transform.position, target.position, Time.deltaTime * followSmoothness);

        // --- 2. ROTACIÓN CON EL JOYSTICK DERECHO ---
        // (Esta parte no cambia)
        float horizontalInput = Input.GetAxis("Horizontal_RightStick");

        if (Mathf.Abs(horizontalInput) > 0.1f)
        {
            transform.Rotate(0, horizontalInput * rotationSpeed * Time.deltaTime, 0);
        }
    }
}