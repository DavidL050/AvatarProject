using UnityEngine;

/// <summary>
/// Script helper para configurar objetos agarrables rápidamente
/// Añade este script a cualquier objeto que quieras hacer agarrable en el Editor
/// </summary>
public class VRGrabbableSetup : MonoBehaviour
{
    [Header("Auto-Setup")]
    [Tooltip("Ejecutar configuración automática al iniciar")]
    public bool autoSetup = true;
    
    [Header("Configuración del Objeto")]
    public bool isGrabbable = true;
    public float mass = 1f;
    public bool useGravity = true;
    
    [Header("Collider (si no existe)")]
    public bool addColliderIfMissing = true;
    public ColliderType colliderType = ColliderType.Box;
    
    public enum ColliderType
    {
        Box,
        Sphere,
        Capsule,
        Mesh
    }

    void Start()
    {
        if (autoSetup)
        {
            SetupGrabbableObject();
        }
    }

    [ContextMenu("Setup Grabbable Object")]
    public void SetupGrabbableObject()
    {
        Debug.Log($"🔧 Configurando objeto agarrable: {gameObject.name}");
        
        // 1. Añadir/configurar Rigidbody
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
            Debug.Log("  ✓ Rigidbody añadido");
        }
        
        rb.mass = mass;
        rb.useGravity = useGravity;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        
        // 2. Añadir Collider si es necesario
        Collider col = GetComponent<Collider>();
        if (col == null && addColliderIfMissing)
        {
            AddCollider();
        }
        
        // 3. Añadir/configurar VRGrabbable
        VRGrabbable grabbable = GetComponent<VRGrabbable>();
        if (grabbable == null)
        {
            grabbable = gameObject.AddComponent<VRGrabbable>();
            Debug.Log("  ✓ VRGrabbable añadido");
        }
        
        grabbable.isGrabbable = isGrabbable;
        grabbable.normalMass = mass;
        grabbable.useGravity = useGravity;
        
        // 4. Asegurar que el layer permita interacción
        if (gameObject.layer == 0) // Default layer
        {
            Debug.LogWarning("  ⚠ Considera cambiar el layer para mejor control de colisiones");
        }
        
        Debug.Log($"✓ Objeto configurado correctamente: {gameObject.name}");
    }

    private void AddCollider()
    {
        switch (colliderType)
        {
            case ColliderType.Box:
                gameObject.AddComponent<BoxCollider>();
                Debug.Log("  ✓ BoxCollider añadido");
                break;
                
            case ColliderType.Sphere:
                gameObject.AddComponent<SphereCollider>();
                Debug.Log("  ✓ SphereCollider añadido");
                break;
                
            case ColliderType.Capsule:
                gameObject.AddComponent<CapsuleCollider>();
                Debug.Log("  ✓ CapsuleCollider añadido");
                break;
                
            case ColliderType.Mesh:
                MeshCollider meshCol = gameObject.AddComponent<MeshCollider>();
                meshCol.convex = true; // Necesario para Rigidbody
                Debug.Log("  ✓ MeshCollider añadido (convex)");
                break;
        }
    }

    void OnValidate()
    {
        if (mass <= 0)
        {
            mass = 0.1f;
        }
    }
}
