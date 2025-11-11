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

    
    void Start()
    {
   
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

        transform.position = Vector3.Lerp(transform.position, target.position, Time.deltaTime * followSmoothness);

    
        float horizontalInput = Input.GetAxis("Horizontal_RightStick");

        if (Mathf.Abs(horizontalInput) > 0.1f)
        {
            transform.Rotate(0, horizontalInput * rotationSpeed * Time.deltaTime, 0);
        }
    }
}