

using UnityEngine;

public class CameraFocusController : MonoBehaviour 
{
    [Header("Objetivos de Enfoque")]
    [Tooltip("La posición por defecto a la que volverá la cámara.")]
    [SerializeField] private Transform defaultTarget;

    [Tooltip("El punto de vista para enfocar la cara.")]
    [SerializeField] private Transform faceTarget;

    [Tooltip("El punto de vista para enfocar el cuerpo.")]
    [SerializeField] private Transform bodyTarget;

    [Header("Velocidad de Movimiento")]
    [Tooltip("Qué tan rápido se mueve la cámara a la nueva posición.")]
    [SerializeField] private float moveSpeed = 5f;

    [Tooltip("Qué tan rápido rota la cámara a la nueva orientación.")]
    [SerializeField] private float rotateSpeed = 5f;

    // El transform del destino actual al que nos estamos moviendo.
    private Transform currentTarget;

    void Start()
    {
       
        if (defaultTarget != null)
        {
            transform.position = defaultTarget.position;
            transform.rotation = defaultTarget.rotation;
            currentTarget = defaultTarget;
        }
    }

    void Update()
    {
       
        if (currentTarget == null) return;

        
        transform.position = Vector3.Lerp(transform.position, currentTarget.position, Time.deltaTime * moveSpeed);

        
        transform.rotation = Quaternion.Slerp(transform.rotation, currentTarget.rotation, Time.deltaTime * rotateSpeed);
    }


    public void FocusOnFace()
    {
        Debug.Log("Cambiando enfoque a la CARA.");
        currentTarget = faceTarget;
    }

   
    public void FocusOnBody()
    {
        Debug.Log("Cambiando enfoque al CUERPO.");
        currentTarget = bodyTarget;
    }

   
    public void ResetFocus()
    {
        Debug.Log("Restableciendo enfoque a la vista por defecto.");
        currentTarget = defaultTarget;
    }
}