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
    [Tooltip("Velocidad de caminata del personaje")]
    public float walkSpeed = 1.3f;
    [Tooltip("Velocidad de sprint del personaje")]
    public float sprintSpeed = 3f;
    [Tooltip("Velocidad de rotación del personaje")]
    public float rotationSpeed = 540f;

    [Header("Valores para el Animator")]
    [Tooltip("Valor enviado al Animator para la animación de caminar")]
    public float walkAnimSpeed = 0.5f;
    [Tooltip("Valor enviado al Animator para la animación de correr")]
    public float runAnimSpeed = 0.8f;

    [Header("Configuración de Salto")]
    public float jumpForce = 6f;
    public float gravity = 15f;
    public float jumpCooldown = 0.5f;
    public float groundCheckMargin = 0.1f;

    [Header("Suavizado de Animación")]
    [Tooltip("Tiempo de transición entre animaciones (mayor = más suave)")]
    [Range(0.05f, 0.5f)]
    public float animationDampTime = 0.15f;
    [Tooltip("Velocidad de aceleración del movimiento")]
    [Range(1f, 10f)]
    public float accelerationRate = 3.81f;
    [Tooltip("Velocidad de desaceleración del movimiento")]
    [Range(1f, 10f)]
    public float decelerationRate = 10f;
    
    [Header("Control de Detención")]
    [Tooltip("Umbral mínimo de input para considerar que hay movimiento")]
    [Range(0.01f, 0.3f)]
    public float inputThreshold = 0.102f;
    [Tooltip("Detener instantáneamente al soltar el joystick (recomendado: activado)")]
    public bool instantStop = true;

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
    private Vector2 rawMoveInput; // Input sin procesar para detección precisa
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
    private float currentMovementSpeed = 0f;
    private float currentAnimSpeed = 0f; // Para controlar la animación directamente
    
    // Controladores VR
    private UnityEngine.XR.InputDevice leftController;
    private UnityEngine.XR.InputDevice rightController;
    private bool vrInitialized = false;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();
        
        var moveAction = playerInput.actions["Move"];
        if (moveAction != null) {
            moveAction.performed += OnMovePerformed;
            moveAction.canceled += OnMoveCanceled;
        }
        
        sprintAction = playerInput.actions["Sprint"];
        jumpAction = playerInput.actions["Jump"];
        
        if (jumpAction != null)
            jumpAction.performed += OnJumpPerformed;
    }

    void Start()
    {
        AvatarCustomization customization = GetComponent<AvatarCustomization>();
        if (customization != null && customization.Animator != null) {
            animator = customization.Animator;
            Debug.Log($"✓ Animator encontrado: {animator.runtimeAnimatorController?.name}");
            foreach (var param in animator.parameters)
                Debug.Log($"📋 Parámetro Animator: {param.name} (Tipo: {param.type})");
        } else {
            Debug.LogWarning("⚠ No se encontró AvatarCustomization o Animator");
        }
        
        if (Camera.main != null) {
            cameraTransform = Camera.main.transform;
        } else {
            Debug.LogError("✗ No se encontró cámara principal");
        }
        
        if (xrOrigin == null) {
            GameObject xr = GameObject.Find("XR Origin (VR)");
            if (xr != null) {
                xrOrigin = xr.transform;
                Debug.Log("✓ XR Origin encontrado");
            }
        }
        
        InitializeVRControllers();
        Debug.Log("========== UnifiedPlayerMovement Inicializado ==========");
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        Debug.Log("!!! JUMP ACTION DETECTED BY INPUT SYSTEM !!!");
        jumpRequestedThisFrame = true;
    }

    void Update()
    {
        if (!vrInitialized)
            InitializeVRControllers();
        
        GetVRInput();
        
        if (sprintAction != null) {
            sprintInput = sprintInput || sprintAction.IsPressed();
        }
        
        wasGroundedLastFrame = isGrounded;
        isGrounded = characterController.isGrounded ||
                     Physics.Raycast(transform.position, Vector3.down, (characterController.height / 2f) + groundCheckMargin);
        
        if (isGrounded && !wasGroundedLastFrame && isJumping) {
            isJumping = false;
            Debug.Log("🛬 Aterrizaje detectado");
        }
        
        ApplyGravity();
        ProcessJump();
        MoveCharacter();
        UpdateAnimations();
        
        jumpRequestedThisFrame = false;
    }

    public void TriggerGrabAnimation()
    {
        if (animator != null)
        {
            // Usamos el nombre del trigger que creaste en tu Animator
            animator.SetTrigger("GrabObjectTrigger");
            Debug.Log("🏃‍♂️ UnifiedPlayerMovement activó el trigger: GrabObjectTrigger");
        }
        else
        {
            Debug.LogWarning("⚠️ Se intentó activar la animación de agarre, pero el Animator es nulo.");
        }
    }
    
    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        rawMoveInput = context.ReadValue<Vector2>();
        moveInput = rawMoveInput;
    }
    
    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        rawMoveInput = Vector2.zero;
        moveInput = Vector2.zero;
        
        // Detención TOTAL inmediata
        if (instantStop) {
            currentMovementSpeed = 0f;
            currentAnimSpeed = 0f;
            
            // Forzar el parámetro Speed a 0 inmediatamente sin suavizado
            if (animator != null && !string.IsNullOrEmpty(speedParameterName)) {
                animator.SetFloat(speedParameterName, 0f);
            }
            
            Debug.Log("🛑 Input cancelado - DETENCIÓN TOTAL");
        }
    }
    
    private void InitializeVRControllers()
    {
        var devices = new List<UnityEngine.XR.InputDevice>();
        
        // Controlador izquierdo
        InputDevices.GetDevicesWithCharacteristics(
            InputDeviceCharacteristics.Left | InputDeviceCharacteristics.Controller,
            devices
        );
        if (devices.Count > 0) {
            leftController = devices[0];
            Debug.Log($"✓ Left Controller conectado: {leftController.name}");
        }
        
        // Controlador derecho
        devices.Clear();
        InputDevices.GetDevicesWithCharacteristics(
            InputDeviceCharacteristics.Right | InputDeviceCharacteristics.Controller,
            devices
        );
        if (devices.Count > 0) {
            rightController = devices[0];
            vrInitialized = true;
            Debug.Log($"✓ Right Controller conectado: {rightController.name}");
        }
    }
    
    private void GetVRInput()
    {
        bool hadPreviousInput = rawMoveInput.sqrMagnitude > 0.01f;
        
        // MOVIMIENTO - Joystick izquierdo
        if (leftController.isValid &&
            leftController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out Vector2 vrJoystick))
        {
            rawMoveInput = vrJoystick;
            
            if (vrJoystick.sqrMagnitude > 0.01f) {
                moveInput = vrJoystick;
            } else {
                moveInput = Vector2.zero;
                
                // Si acabamos de soltar el joystick, forzar detención
                if (hadPreviousInput && instantStop) {
                    currentMovementSpeed = 0f;
                    currentAnimSpeed = 0f;
                    
                    if (animator != null && !string.IsNullOrEmpty(speedParameterName)) {
                        animator.SetFloat(speedParameterName, 0f);
                    }
                    
                    Debug.Log("🛑 VR Joystick soltado - DETENCIÓN TOTAL");
                }
            }
        }
        
        // SPRINT - Gatillo izquierdo (Left Trigger)
        if (leftController.isValid &&
            leftController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out float triggerValue))
        {
            sprintInput = triggerValue > 0.5f;
        }
        
        // SALTO - Botón A del controlador derecho (Primary Button)
        if (rightController.isValid &&
            rightController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primaryButton, out bool jumpPressed))
        {
            if (jumpPressed && !jumpRequestedThisFrame)
            {
                jumpRequestedThisFrame = true;
                Debug.Log($"🎮 VR Jump pressed! Frame: {Time.frameCount}");
            }
        }
    }
    
    private void ApplyGravity()
    {
        if (isGrounded && velocity.y <= 0) {
            velocity.y = -2f;
        } else {
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
            
            if (animator != null && !string.IsNullOrEmpty(jumpTriggerName)) {
                animator.ResetTrigger(jumpTriggerName);
                animator.SetTrigger(jumpTriggerName);
            }
            
            Debug.Log($"🦘 Salto ejecutado - Frame: {Time.frameCount}, Tiempo: {Time.time:F2}");
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
        
        Vector2 clampedInput = Vector2.ClampMagnitude(moveInput, 1f);
        float inputMagnitude = clampedInput.magnitude;
        
        Vector3 moveDirection = Vector3.zero;
        if (inputMagnitude > inputThreshold) {
            moveDirection = (forward * clampedInput.y + right * clampedInput.x).normalized;
        }
        
        float targetSpeed = 0f;
        if (inputMagnitude > inputThreshold) {
            targetSpeed = sprintInput ? sprintSpeed : walkSpeed;
            targetSpeed *= inputMagnitude;
        }
        
        // DETENCIÓN ULTRA AGRESIVA
        if (instantStop && inputMagnitude < inputThreshold) {
            // Forzar velocidad a cero INMEDIATAMENTE sin Lerp
            currentMovementSpeed = 0f;
        } else if (inputMagnitude < inputThreshold) {
            // Desaceleración muy rápida
            currentMovementSpeed = Mathf.Lerp(currentMovementSpeed, 0f, Time.deltaTime * decelerationRate * 2f);
            
            // Umbral de corte para evitar micro-movimientos
            if (currentMovementSpeed < 0.05f) {
                currentMovementSpeed = 0f;
            }
        } else {
            // Aceleración/desaceleración normal cuando hay input
            float smoothRate = (targetSpeed > currentMovementSpeed) ? accelerationRate : decelerationRate;
            currentMovementSpeed = Mathf.Lerp(currentMovementSpeed, targetSpeed, Time.deltaTime * smoothRate);
        }
        
        // Solo mover si la velocidad es significativa
        if (currentMovementSpeed > 0.01f) {
            Vector3 horizontalMovement = moveDirection * currentMovementSpeed;
            Vector3 finalMovement = (horizontalMovement + velocity) * Time.deltaTime;
            characterController.Move(finalMovement);
            
            // Rotar solo si se está moviendo
            if (moveDirection.sqrMagnitude > 0.01f) {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    rotationSpeed * Time.deltaTime
                );
            }
        } else {
            // Si no hay movimiento, solo aplicar gravedad
            Vector3 finalMovement = velocity * Time.deltaTime;
            characterController.Move(finalMovement);
        }
    }

    private void UpdateAnimations()
    {
        if (animator == null) return;

        float inputMagnitude = Vector2.ClampMagnitude(moveInput, 1f).magnitude;
        float targetAnimSpeed = 0f;

        if (inputMagnitude > inputThreshold)
        {
            targetAnimSpeed = sprintInput ? runAnimSpeed : walkAnimSpeed;
            targetAnimSpeed *= inputMagnitude;
        }

        // CONTROL DIRECTO DEL ANIMATOR - SIN SUAVIZADO cuando se detiene
        if (instantStop && inputMagnitude < inputThreshold) {
            // Forzar a 0 INMEDIATAMENTE sin ningún suavizado
            currentAnimSpeed = 0f;
            if (!string.IsNullOrEmpty(speedParameterName)) {
                animator.SetFloat(speedParameterName, 0f); // Sin damp time
            }
        } else {
            // Suavizado normal solo cuando hay movimiento
            float dampTime = (targetAnimSpeed > currentAnimSpeed) ? animationDampTime : animationDampTime * 0.5f;
            currentAnimSpeed = Mathf.Lerp(currentAnimSpeed, targetAnimSpeed, Time.deltaTime / Mathf.Max(dampTime, 0.01f));
            
            if (!string.IsNullOrEmpty(speedParameterName)) {
                animator.SetFloat(speedParameterName, currentAnimSpeed);
            }
        }

        if (!string.IsNullOrEmpty(groundedParameterName))
            animator.SetBool(groundedParameterName, isGrounded);
        
        Debug.Log($"[AnimDEBUG] InputMag: {inputMagnitude:F3}, TargetAnim: {targetAnimSpeed:F2}, CurrentAnim: {currentAnimSpeed:F2}, MovementSpeed: {currentMovementSpeed:F2}");
    }

    public void OnLand()
    {
        Debug.Log("📢 Animation Event: OnLand");
        isJumping = false;
        if (animator != null && !string.IsNullOrEmpty(jumpTriggerName)) {
            animator.ResetTrigger(jumpTriggerName);
        }
    }
    
    public void OnFootstep() { }
    
    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        
        if (isJumping)
            Gizmos.color = Color.yellow;
        else if (isGrounded)
            Gizmos.color = Color.green;
        else
            Gizmos.color = Color.red;
        
        Gizmos.DrawWireSphere(transform.position, 0.3f);
        
        if (cameraTransform != null) {
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
            jumpAction.performed -= OnJumpPerformed;
    }
}
