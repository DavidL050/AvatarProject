using UnityEngine;
using Sunbox.Avatars;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using System.Collections.Generic;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class UnifiedPlayerMovement : MonoBehaviour
{
    [Header("Velocidades de Movimiento")]
    [Tooltip("Velocidad de caminata (debe coincidir con threshold de Walk en Blend Tree)")]
    public float walkSpeed = 1f;
    
    [Tooltip("Velocidad de sprint (debe coincidir con threshold de Run en Blend Tree)")]
    public float sprintSpeed = 3f;
    
    [Tooltip("Velocidad de rotación del personaje")]
    public float rotationSpeed = 720f;

    [Header("Configuración de Salto")]
    public float jumpForce = 6f;
    public float gravity = 15f;
    public float jumpCooldown = 0.5f;
    public float groundCheckMargin = 0.1f;

    [Header("Suavizado de Animación")]
    [Tooltip("Velocidad de transición entre animaciones (menor = más suave)")]
    [Range(0.1f, 1f)]
    public float animationSmoothTime = 0.15f;
    
    [Tooltip("Multiplicador de aceleración al iniciar movimiento")]
    [Range(1f, 10f)]
    public float accelerationRate = 5f;
    
    [Tooltip("Multiplicador de desaceleración al detener movimiento")]
    [Range(1f, 10f)]
    public float decelerationRate = 7f;

    [Header("Referencias VR")]
    public Transform xrOrigin;

    [Header("Animación - Nombres de Parámetros")]
    public string speedParameterName = "Speed";
    public string jumpTriggerName = "Jump";
    public string groundedParameterName = "Grounded";

    // Componentes
    private CharacterController characterController;
    private Animator animator;
    private PlayerInput playerInput;
    private Transform cameraTransform;

    // Input
    private Vector2 moveInput;
    private bool sprintInput;
    private InputAction jumpAction;
    private InputAction sprintAction;

    // Estado
    private Vector3 velocity;
    private float lastJumpTime = -999f;
    private bool isGrounded;
    private bool wasGroundedLastFrame = true;
    private bool jumpRequestedThisFrame = false;
    private bool isJumping = false;
    
    // Suavizado de animación
    private float currentAnimSpeed = 0f;
    private float animationVelocity = 0f; // Para SmoothDamp

    // VR
    private UnityEngine.XR.InputDevice leftController;
    private bool vrInitialized = false;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();

        // Configurar acción de Movimiento
        var moveAction = playerInput.actions["Move"];
        if (moveAction != null)
        {
            moveAction.performed += OnMovePerformed;
            moveAction.canceled += OnMoveCanceled;
        }

        // Configurar acción de Sprint (solo guardar referencia, lectura directa en Update)
        sprintAction = playerInput.actions["Sprint"];

        // Configurar acción de Salto
        jumpAction = playerInput.actions["Jump"];
        if (jumpAction != null)
        {
            jumpAction.performed += OnJumpPerformed;
        }
    }

    void Start()
    {
        AvatarCustomization customization = GetComponent<AvatarCustomization>();
        if (customization != null && customization.Animator != null)
        {
            animator = customization.Animator;
            Debug.Log($"✓ Animator encontrado: {animator.runtimeAnimatorController?.name}");
            
            // Verificar parámetros del Animator
            foreach (var param in animator.parameters)
            {
                Debug.Log($"📋 Parámetro Animator: {param.name} (Tipo: {param.type})");
            }
        }
        else
        {
            Debug.LogWarning("⚠ No se encontró AvatarCustomization o Animator");
        }

        if (Camera.main != null)
        {
            cameraTransform = Camera.main.transform;
        }
        else
        {
            Debug.LogError("✗ No se encontró cámara principal");
        }

        if (xrOrigin == null)
        {
            GameObject xr = GameObject.Find("XR Origin (VR)");
            if (xr != null)
            {
                xrOrigin = xr.transform;
                Debug.Log("✓ XR Origin encontrado");
            }
        }

        InitializeVRControllers();
        Debug.Log("========== UnifiedPlayerMovement Inicializado ==========");
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        jumpRequestedThisFrame = true;
    }

    void Update()
    {
        if (!vrInitialized)
            InitializeVRControllers();

        GetVRInput();
        
        // CORRECCIÓN SPRINT: Lectura directa del estado del botón
        if (sprintAction != null)
        {
            sprintInput = sprintAction.IsPressed();
        }
        
        // Detección de suelo
        wasGroundedLastFrame = isGrounded;
        isGrounded = characterController.isGrounded || 
                     (characterController.collisionFlags & CollisionFlags.Below) != 0;

        // Detectar aterrizaje
        if (isGrounded && !wasGroundedLastFrame && isJumping)
        {
            isJumping = false;
            Debug.Log("🛬 Aterrizaje detectado");
        }

        ApplyGravity();
        ProcessJump();
        MoveCharacter();
        UpdateAnimations();
        
        // Resetear solicitud de salto
        jumpRequestedThisFrame = false;
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
    }

    private void InitializeVRControllers()
    {
        var devices = new List<UnityEngine.XR.InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(
            InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller,
            devices
        );

        if (devices.Count > 0)
        {
            leftController = devices[0];
            vrInitialized = true;
            Debug.Log($"✓ VR Controller conectado: {leftController.name}");
        }
    }

    private void GetVRInput()
    {
        if (leftController.isValid &&
            leftController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out Vector2 vrJoystick))
        {
            if (vrJoystick.sqrMagnitude > 0.01f)
                moveInput = vrJoystick;
        }
    }

    private void ApplyGravity()
    {
        if (isGrounded && velocity.y <= 0)
        {
            velocity.y = -2f;
        }
        else
        {
            velocity.y -= gravity * Time.deltaTime;
        }
    }

    private void ProcessJump()
    {
        bool canJump = jumpRequestedThisFrame && 
                       isGrounded && 
                       !isJumping && 
                       (Time.time - lastJumpTime) > jumpCooldown &&
                       velocity.y <= 0.1f;

        if (canJump)
        {
            velocity.y = jumpForce;
            lastJumpTime = Time.time;
            isJumping = true;

            if (animator != null && !string.IsNullOrEmpty(jumpTriggerName))
            {
                animator.ResetTrigger(jumpTriggerName);
                animator.SetTrigger(jumpTriggerName);
            }

            Debug.Log($"🦘 Salto ejecutado - Frame: {Time.frameCount}, Tiempo: {Time.time:F2}");
        }
        else if (jumpRequestedThisFrame)
        {
            Debug.Log($"❌ Salto rechazado - Grounded: {isGrounded}, Jumping: {isJumping}, " +
                     $"Cooldown: {Time.time - lastJumpTime:F2}s/{jumpCooldown}s, VelY: {velocity.y:F2}");
        }
    }

    private void MoveCharacter()
    {
        if (cameraTransform == null) return;

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;
        forward.y = 0;
        right.y = 0;
        forward.Normalize();
        right.Normalize();

        // CORRECCIÓN DIAGONAL: Clamp magnitude del input antes de usarlo
        Vector2 clampedInput = Vector2.ClampMagnitude(moveInput, 1f);
        
        // Guardar magnitud para movimiento analógico (gamepad)
        float inputMagnitude = clampedInput.magnitude;
        
        // Calcular dirección de movimiento
        Vector3 moveDirection = Vector3.zero;
        if (inputMagnitude > 0.1f)
        {
            moveDirection = (forward * clampedInput.y + right * clampedInput.x).normalized;
        }

        // Determinar velocidad objetivo
        float targetSpeed = sprintInput ? sprintSpeed : walkSpeed;
        
        // Multiplicar por magnitud del input para soporte analógico
        float currentSpeed = inputMagnitude * targetSpeed;

        // Aplicar movimiento
        Vector3 finalMovement = (moveDirection * currentSpeed + velocity) * Time.deltaTime;
        characterController.Move(finalMovement);

        // Rotación suave hacia la dirección de movimiento
        if (moveDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRotation,
                rotationSpeed * Time.deltaTime
            );
        }
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        // Calcular velocidad objetivo para el Blend Tree
        float targetAnimSpeed = 0f;
        
        if (moveInput.magnitude > 0.1f)
        {
            // Si hay input, usar la velocidad correspondiente
            targetAnimSpeed = sprintInput ? sprintSpeed : walkSpeed;
        }
        
        // SUAVIZADO: Interpolación suave hacia la velocidad objetivo
        currentAnimSpeed = Mathf.SmoothDamp(
            currentAnimSpeed, 
            targetAnimSpeed, 
            ref animationVelocity, 
            animationSmoothTime
        );

        // Enviar velocidad suavizada al Animator
        if (!string.IsNullOrEmpty(speedParameterName))
        {
            animator.SetFloat(speedParameterName, currentAnimSpeed);
        }

        // Actualizar estado de suelo
        if (!string.IsNullOrEmpty(groundedParameterName))
        {
            animator.SetBool(groundedParameterName, isGrounded);
        }
    }
    
    public void OnLand()
    {
        Debug.Log("📢 Animation Event: OnLand");
        isJumping = false;
        
        if (animator != null && !string.IsNullOrEmpty(jumpTriggerName))
        {
            animator.ResetTrigger(jumpTriggerName);
        }
    }

    public void OnFootstep()
    {
        Debug.Log("👟 Animation Event: OnFootstep");
        // Aquí puedes agregar reproducción de sonidos de pasos
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        // Indicador visual de estado
        if (isJumping)
            Gizmos.color = Color.yellow;
        else if (isGrounded)
            Gizmos.color = Color.green;
        else
            Gizmos.color = Color.red;
            
        Gizmos.DrawWireSphere(transform.position, 0.3f);

        // Dirección de cámara
        if (cameraTransform != null)
        {
            Vector3 forward = cameraTransform.forward;
            forward.y = 0;
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, forward.normalized * 2f);
        }
    }

    void OnDestroy()
    {
        if (playerInput != null)
        {
            var moveAction = playerInput.actions["Move"];
            if (moveAction != null)
            {
                moveAction.performed -= OnMovePerformed;
                moveAction.canceled -= OnMoveCanceled;
            }
        }
        
        if (jumpAction != null)
        {
            jumpAction.performed -= OnJumpPerformed;
        }
    }
}
