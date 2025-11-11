
using UnityEngine;
using Sunbox.Avatars;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    public float moveSpeed = 5f;
    public float rotationSpeed = 720f; 
    public float gravity = -20f;

    private CharacterController characterController;
    private Animator animator;
    private PlayerInput playerInput;
    private Vector3 velocity;
    private Vector2 moveInput;

   
    private Transform cameraMainTransform; 

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();

        var moveAction = playerInput.actions["Move"];
        moveAction.performed += OnMovePerformed;
        moveAction.canceled += OnMoveCanceled;
    }

    void Start()
    {
      
        AvatarCustomization customization = GetComponent<AvatarCustomization>();
        if (customization != null && customization.Animator != null)
        {
            animator = customization.Animator;
        }
        else
        {
            Debug.LogError("No se pudo encontrar el Animator activo a través de AvatarCustomization.");
        }
        
      
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

        if (characterController.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }


        Vector3 forward = cameraMainTransform.forward;
        Vector3 right = cameraMainTransform.right;
        

        forward.y = 0;
        right.y = 0;
        
        forward.Normalize();
        right.Normalize();

        Vector3 desiredMoveDirection = forward * moveInput.y + right * moveInput.x;

        characterController.Move(desiredMoveDirection.normalized * moveSpeed * Time.deltaTime);
        velocity.y += gravity * Time.deltaTime;
        characterController.Move(velocity * Time.deltaTime);

    
        if (desiredMoveDirection != Vector3.zero)
        {
       
            Quaternion toRotation = Quaternion.LookRotation(desiredMoveDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, rotationSpeed * Time.deltaTime);
        }

       
        float normalizedSpeed = desiredMoveDirection.magnitude;
        animator.SetFloat("Forward", normalizedSpeed);
    }
}