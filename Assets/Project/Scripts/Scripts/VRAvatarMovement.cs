using UnityEngine;
using UnityEngine.XR;

/// <summary>
/// Movimiento del avatar en tercera persona usando controles VR
/// El avatar se mueve con el joystick izquierdo
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class VRAvatarMovement : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [Tooltip("Velocidad de movimiento del avatar")]
    public float moveSpeed = 3f;
    
    [Tooltip("Velocidad de rotación del avatar")]
    public float rotationSpeed = 100f;
    
    [Tooltip("Gravedad aplicada al avatar")]
    public float gravity = 9.81f;

    [Header("Referencias VR")]
    [Tooltip("XR Origin - se usa como referencia de dirección")]
    public Transform xrOrigin;
    
    [Tooltip("Cámara VR - para calcular dirección de movimiento")]
    public Transform vrCamera;

    [Header("Animación (Opcional)")]
    [Tooltip("Animator del avatar para animaciones de caminar")]
    public Animator avatarAnimator;
    
    [Tooltip("Nombre del parámetro de velocidad en el Animator")]
    public string speedParameterName = "Speed";

    // Componentes
    private CharacterController characterController;
    private Vector3 velocity;
    
    // Input VR
    private InputDevice leftController;
    private bool controllersInitialized = false;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        
        // Auto-detectar XR Origin
        if (xrOrigin == null)
        {
            GameObject xr = GameObject.Find("XR Origin (VR)");
            if (xr != null) xrOrigin = xr.transform;
        }

        // Auto-detectar cámara VR
        if (vrCamera == null && xrOrigin != null)
        {
            vrCamera = xrOrigin.Find("Camera Offset/Main Camera");
            if (vrCamera == null)
            {
                vrCamera = Camera.main?.transform;
            }
        }

        // Auto-detectar Animator
        if (avatarAnimator == null)
        {
            avatarAnimator = GetComponentInChildren<Animator>();
        }

        Debug.Log("✓ VRAvatarMovement inicializado");
    }

    void Update()
    {
        // Inicializar controladores VR
        if (!controllersInitialized)
        {
            InitializeControllers();
        }

        // Obtener input de movimiento
        Vector2 moveInput = GetMoveInput();

        // Mover avatar
        MoveAvatar(moveInput);

        // Actualizar animaciones
        UpdateAnimations(moveInput.magnitude);

        // Aplicar gravedad
        ApplyGravity();
    }

    /// <summary>
    /// Inicializa los controladores VR
    /// </summary>
    void InitializeControllers()
    {
        var devices = new System.Collections.Generic.List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(
            InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller,
            devices
        );

        if (devices.Count > 0)
        {
            leftController = devices[0];
            controllersInitialized = true;
            Debug.Log($"✓ Control VR izquierdo conectado: {leftController.name}");
        }
    }

    /// <summary>
    /// Obtiene el input del joystick izquierdo VR
    /// </summary>
    Vector2 GetMoveInput()
    {
        Vector2 input = Vector2.zero;

        // Input desde control VR
        if (leftController.isValid)
        {
            if (leftController.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 joystickValue))
            {
                input = joystickValue;
            }
        }

        // Fallback: Input desde teclado para testing
        if (input.sqrMagnitude < 0.01f)
        {
            input.x = Input.GetAxis("Horizontal");
            input.y = Input.GetAxis("Vertical");
        }

        return input;
    }

    /// <summary>
    /// Mueve el avatar basado en el input
    /// </summary>
    void MoveAvatar(Vector2 input)
    {
        if (input.sqrMagnitude < 0.01f) return;

        // Calcular dirección de movimiento basada en la cámara VR
        Vector3 cameraForward = vrCamera != null ? vrCamera.forward : transform.forward;
        cameraForward.y = 0; // Mantener movimiento en plano horizontal
        cameraForward.Normalize();

        Vector3 cameraRight = vrCamera != null ? vrCamera.right : transform.right;
        cameraRight.y = 0;
        cameraRight.Normalize();

        // Dirección de movimiento
        Vector3 moveDirection = (cameraForward * input.y + cameraRight * input.x).normalized;

        // Mover el avatar
        characterController.Move(moveDirection * moveSpeed * Time.deltaTime);

        // Rotar el avatar hacia la dirección de movimiento
        if (moveDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, 
                targetRotation, 
                rotationSpeed * Time.deltaTime
            );
        }
    }

    /// <summary>
    /// Aplica gravedad al avatar
    /// </summary>
    void ApplyGravity()
    {
        if (characterController.isGrounded)
        {
            velocity.y = -2f; // Pequeño valor para mantener al avatar pegado al suelo
        }
        else
        {
            velocity.y -= gravity * Time.deltaTime;
        }

        characterController.Move(velocity * Time.deltaTime);
    }

    /// <summary>
    /// Actualiza las animaciones del avatar
    /// </summary>
    void UpdateAnimations(float speed)
    {
        if (avatarAnimator != null && !string.IsNullOrEmpty(speedParameterName))
        {
            avatarAnimator.SetFloat(speedParameterName, speed * moveSpeed);
        }
    }

    /// <summary>
    /// Dibuja gizmos para debugging
    /// </summary>
    void OnDrawGizmos()
    {
        // Dirección de movimiento
        if (vrCamera != null)
        {
            Gizmos.color = Color.blue;
            Vector3 forward = vrCamera.forward;
            forward.y = 0;
            Gizmos.DrawRay(transform.position, forward.normalized * 2f);
        }
    }
}