// PlayerMovement.cs (Versión Final con Movimiento Relativo a la Cámara)
using UnityEngine;
using Sunbox.Avatars;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 720f; // Aumentamos la velocidad de rotación para que sea más responsiva
    public float gravity = -20f;

    private CharacterController characterController;
    private Animator animator;
    private PlayerInput playerInput;
    private Vector3 velocity;
    private Vector2 moveInput;

    // --- NUEVA VARIABLE ---
    private Transform cameraMainTransform; // Guardaremos la referencia a la cámara aquí

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();

        // Nos subscribimos a los eventos de input
        var moveAction = playerInput.actions["Move"];
        moveAction.performed += OnMovePerformed;
        moveAction.canceled += OnMoveCanceled;
    }

    void Start()
    {
        // Encontramos el Animator activo
        AvatarCustomization customization = GetComponent<AvatarCustomization>();
        if (customization != null && customization.Animator != null)
        {
            animator = customization.Animator;
        }
        else
        {
            Debug.LogError("No se pudo encontrar el Animator activo a través de AvatarCustomization.");
        }
        
        // --- NUEVA LÍNEA ---
        // Guardamos la referencia a la transform de la cámara principal al empezar
        if (Camera.main != null)
        {
            cameraMainTransform = Camera.main.transform;
        }
        else
        {
            Debug.LogError("No se encontró una cámara principal en la escena. El movimiento no será relativo a la cámara.");
        }
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        moveInput = Vector2.zero;
    }

    void Update()
    {
        if (animator == null || cameraMainTransform == null) return;

        // --- LÓGICA DE GRAVEDAD (Sin cambios) ---
        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // --- CÁLCULO DE DIRECCIÓN RELATIVO A LA CÁMARA (¡LA GRAN MEJORA!) ---
        Vector3 forward = cameraMainTransform.forward;
        Vector3 right = cameraMainTransform.right;
        
        // Nos aseguramos de que no nos movamos hacia arriba/abajo basándonos en la inclinación de la cámara
        forward.y = 0;
        right.y = 0;
        
        forward.Normalize();
        right.Normalize();

        // Calculamos la dirección de movimiento deseada
        Vector3 desiredMoveDirection = forward * moveInput.y + right * moveInput.x;

        // --- MOVIMIENTO (Ahora usa la nueva dirección) ---
        characterController.Move(desiredMoveDirection.normalized * moveSpeed * Time.deltaTime);
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);

        // --- ROTACIÓN (Ahora rota hacia la nueva dirección) ---
        if (desiredMoveDirection != Vector3.zero)
        {
            // Hacemos que el personaje rote para mirar en la dirección que se mueve
            Quaternion toRotation = Quaternion.LookRotation(desiredMoveDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }

        // --- ANIMACIÓN (Sin cambios, pero ahora reflejará el movimiento correcto) ---
        float normalizedSpeed = desiredMoveDirection.magnitude;
        animator.SetFloat("Forward", normalizedSpeed);
    }
}