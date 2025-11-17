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
    public float decelerationRate = 5f;

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
    private float currentMovementSpeed = 0f;
    
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
        // MOVIMIENTO - Joystick izquierdo
        if (leftController.isValid &&
            leftController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.primary2DAxis, out Vector2 vrJoystick))
        {
            if (vrJoystick.sqrMagnitude > 0.01f)
                moveInput = vrJoystick;
        }
        
        // SPRINT - Gatillo izquierdo (Left Trigger)
        if (leftController.isValid &&
            leftController.TryGetFeatureValue(UnityEngine.XR.CommonUsages.trigger, out float triggerValue))
        {
            sprintInput = triggerValue > 0.5f; // Presionado si el gatillo está >50%
            
            if (sprintInput)
                Debug.Log($"🏃 Sprint activo (Trigger: {triggerValue:F2})");
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
        if (inputMagnitude > 0.1f) {
            moveDirection = (forward * clampedInput.y + right * clampedInput.x).normalized;
        }
        
        float targetSpeed = 0f;
        if (inputMagnitude > 0.1f) {
            targetSpeed = sprintInput ? sprintSpeed : walkSpeed;
            targetSpeed *= inputMagnitude;
        }
        
        float smoothRate = (targetSpeed > currentMovementSpeed) ? accelerationRate : decelerationRate;
        currentMovementSpeed = Mathf.Lerp(currentMovementSpeed, targetSpeed, Time.deltaTime * smoothRate);
        
        Vector3 horizontalMovement = moveDirection * currentMovementSpeed;
        Vector3 finalMovement = (horizontalMovement + velocity) * Time.deltaTime;
        
        characterController.Move(finalMovement);
        
        if (moveDirection.sqrMagnitude > 0.01f) {
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

        float targetAnimSpeed = 0f;
        float inputMagnitude = Vector2.ClampMagnitude(moveInput, 1f).magnitude;

        if (inputMagnitude > 0.1f)
        {
            targetAnimSpeed = sprintInput ? runAnimSpeed : walkAnimSpeed;
            targetAnimSpeed *= inputMagnitude;
        }

        if (!string.IsNullOrEmpty(speedParameterName))
            animator.SetFloat(speedParameterName, targetAnimSpeed, animationDampTime, Time.deltaTime);

        if (!string.IsNullOrEmpty(groundedParameterName))
            animator.SetBool(groundedParameterName, isGrounded);
        
        Debug.Log($"[AnimDEBUG] Speed: {targetAnimSpeed:F2}, Grounded: {isGrounded}, Jump: {jumpRequestedThisFrame}, Sprint: {sprintInput}");
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
