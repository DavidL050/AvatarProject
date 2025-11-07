using UnityEngine;

/// <summary>
/// Sistema de cámara de tercera persona para VR
/// La cámara sigue al avatar mientras te mueves con los joysticks VR
/// </summary>
public class ThirdPersonVRCamera : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("El avatar que seguirá la cámara (Female_Avatar o Male_Avatar)")]
    public Transform avatarTarget;
    
    [Tooltip("XR Origin - para obtener la posición VR")]
    public Transform xrOrigin;

    [Header("Configuración de Cámara")]
    [Tooltip("Distancia de la cámara detrás del avatar")]
    public float distanceBehind = 3f;
    
    [Tooltip("Altura de la cámara sobre el avatar")]
    public float heightAbove = 2f;
    
    [Tooltip("Suavidad del seguimiento (menor = más suave)")]
    [Range(1f, 20f)]
    public float smoothSpeed = 10f;

    [Header("Rotación")]
    [Tooltip("¿Permitir rotar la cámara con el joystick derecho?")]
    public bool allowCameraRotation = true;
    
    [Tooltip("Sensibilidad de rotación")]
    public float rotationSpeed = 100f;

    [Header("Colisión")]
    [Tooltip("¿Evitar que la cámara atraviese paredes?")]
    public bool avoidWalls = true;
    
    [Tooltip("Capas que bloquean la cámara")]
    public LayerMask collisionMask = -1;

    // Variables privadas
    private float currentRotationY = 0f;
    private Vector3 currentVelocity;

    void Start()
    {
        // Auto-detectar avatar si no está asignado
        if (avatarTarget == null)
        {
            GameObject avatar = GameObject.FindGameObjectWithTag("Player");
            if (avatar != null)
            {
                avatarTarget = avatar.transform;
                Debug.Log($"✓ Avatar auto-detectado: {avatarTarget.name}");
            }
            else
            {
                Debug.LogError("⚠ No se encontró el avatar. Asigna 'avatarTarget' manualmente.");
            }
        }

        // Auto-detectar XR Origin si no está asignado
        if (xrOrigin == null)
        {
            GameObject xr = GameObject.Find("XR Origin (VR)");
            if (xr != null)
            {
                xrOrigin = xr.transform;
                Debug.Log($"✓ XR Origin auto-detectado");
            }
        }

        // Establecer rotación inicial
        if (avatarTarget != null)
        {
            currentRotationY = avatarTarget.eulerAngles.y;
        }
    }

    void LateUpdate()
    {
        if (avatarTarget == null) return;

        // Manejar rotación de cámara con joystick derecho (opcional)
        HandleCameraRotation();

        // Calcular posición deseada de la cámara
        Vector3 desiredPosition = CalculateDesiredPosition();

        // Verificar colisiones
        if (avoidWalls)
        {
            desiredPosition = CheckForWallCollision(desiredPosition);
        }

        // Mover cámara suavemente
        transform.position = Vector3.SmoothDamp(
            transform.position, 
            desiredPosition, 
            ref currentVelocity, 
            1f / smoothSpeed
        );

        // Hacer que la cámara mire al avatar
        transform.LookAt(avatarTarget.position + Vector3.up * heightAbove * 0.5f);
    }

    /// <summary>
    /// Calcula la posición deseada de la cámara detrás del avatar
    /// </summary>
    Vector3 CalculateDesiredPosition()
    {
        // Dirección hacia atrás basada en la rotación actual
        Vector3 directionBack = Quaternion.Euler(0, currentRotationY, 0) * Vector3.back;
        
        // Posición objetivo
        Vector3 targetPosition = avatarTarget.position;
        targetPosition += directionBack * distanceBehind;
        targetPosition += Vector3.up * heightAbove;

        return targetPosition;
    }

    /// <summary>
    /// Verifica si hay una pared entre la cámara y el avatar
    /// </summary>
    Vector3 CheckForWallCollision(Vector3 desiredPosition)
    {
        Vector3 avatarPos = avatarTarget.position + Vector3.up * heightAbove * 0.5f;
        Vector3 direction = desiredPosition - avatarPos;
        float distance = direction.magnitude;

        RaycastHit hit;
        if (Physics.Raycast(avatarPos, direction.normalized, out hit, distance, collisionMask))
        {
            // Hay una pared, acercar la cámara
            return hit.point - direction.normalized * 0.3f; // 0.3f = buffer
        }

        return desiredPosition;
    }

    /// <summary>
    /// Permite rotar la cámara con el joystick derecho del control VR
    /// </summary>
    void HandleCameraRotation()
    {
        if (!allowCameraRotation) return;

        // Input para VR (joystick derecho)
        Vector2 rightStickInput = Vector2.zero;

        // Obtener input del joystick derecho VR
        // Esto depende de tu sistema VR (XR Input o Input System)
        #if UNITY_INPUT_SYSTEM
        // Si usas Input System:
        // rightStickInput = InputSystem.actions.FindAction("Camera Rotate").ReadValue<Vector2>();
        #else
        // Si usas Input Manager clásico:
        rightStickInput.x = Input.GetAxis("Horizontal_RightStick"); // Configura esto en Input Manager
        #endif

        // También permitir rotación con teclado para testing
        if (Input.GetKey(KeyCode.Q)) rightStickInput.x = -1f;
        if (Input.GetKey(KeyCode.E)) rightStickInput.x = 1f;

        // Aplicar rotación
        if (rightStickInput.sqrMagnitude > 0.01f)
        {
            currentRotationY += rightStickInput.x * rotationSpeed * Time.deltaTime;
        }
        else
        {
            // Si no hay input, seguir la rotación del avatar
            currentRotationY = Mathf.LerpAngle(
                currentRotationY, 
                avatarTarget.eulerAngles.y, 
                Time.deltaTime * 2f
            );
        }
    }

    /// <summary>
    /// Dibuja gizmos en el editor para visualizar la configuración
    /// </summary>
    void OnDrawGizmosSelected()
    {
        if (avatarTarget == null) return;

        // Posición del avatar
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(avatarTarget.position, 0.3f);

        // Posición deseada de la cámara
        Vector3 desiredPos = CalculateDesiredPosition();
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(desiredPos, 0.3f);

        // Línea entre avatar y cámara
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(avatarTarget.position, desiredPos);
    }
}