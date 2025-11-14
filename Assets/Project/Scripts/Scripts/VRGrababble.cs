using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class VRGrabbable : MonoBehaviour
{
    [Header("Configuración de Agarre")]
    [Tooltip("¿Se puede agarrar este objeto?")]
    public bool isGrabbable = true;
    
    [Tooltip("Distancia máxima para detectar el agarre")]
    public float grabDistance = 0.3f;
    
    [Header("Física")]
    [Tooltip("Masa del objeto cuando NO está agarrado")]
    public float normalMass = 1f;
    
    [Tooltip("¿Usar gravedad cuando está suelto?")]
    public bool useGravity = true;
    
    [Header("Lanzamiento")]
    [Tooltip("Multiplicador de velocidad al lanzar")]
    public float throwForceMultiplier = 1.5f;
    
    [Tooltip("Velocidad angular al soltar")]
    public float angularVelocityMultiplier = 1f;
    
    [Tooltip("Número de muestras de velocidad para suavizado")]
    public int velocitySampleCount = 5;
    
    [Header("Audio (Opcional)")]
    public AudioClip grabSound;
    public AudioClip releaseSound;
    
    [Header("Debug")]
    public bool showDebug = true;
    
    // Estado interno
    private Rigidbody rb;
    private AudioSource audioSource;
    private bool isBeingGrabbed = false;
    private Transform grabber;
    
    // Tracking mejorado de velocidad
    private Vector3[] velocitySamples;
    private Vector3[] angularVelocitySamples;
    private int currentSampleIndex = 0;
    private bool samplesInitialized = false;
    
    // Offset relativo al grip point
    private Vector3 grabPositionOffset;
    private Quaternion grabRotationOffset;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
        
        // Configurar Rigidbody inicial
        rb.mass = normalMass;
        rb.useGravity = useGravity;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        
        // Inicializar arrays de muestreo
        velocitySamples = new Vector3[velocitySampleCount];
        angularVelocitySamples = new Vector3[velocitySampleCount];
        
        if (showDebug)
            Debug.Log($"🎯 VRGrabbable inicializado en: {gameObject.name}");
    }

    void FixedUpdate()
    {
        if (isBeingGrabbed && grabber != null)
        {
            // Calcular posición y rotación objetivo manteniendo offset
            Vector3 targetPosition = grabber.TransformPoint(grabPositionOffset);
            Quaternion targetRotation = grabber.rotation * grabRotationOffset;
            
            // Mover con física correcta
            rb.MovePosition(targetPosition);
            rb.MoveRotation(targetRotation);
            
            // Capturar velocidad real del Rigidbody
            Vector3 velocity = rb.linearVelocity;
            Vector3 angularVelocity = rb.angularVelocity;
            
            // Guardar muestra para promedio
            velocitySamples[currentSampleIndex] = velocity;
            angularVelocitySamples[currentSampleIndex] = angularVelocity;
            currentSampleIndex = (currentSampleIndex + 1) % velocitySampleCount;
            samplesInitialized = true;
        }
    }

    public void OnGrab(Transform grabberTransform)
    {
        if (!isGrabbable)
        {
            if (showDebug)
                Debug.LogWarning($"⚠️ Intento de agarrar objeto no agarrable: {gameObject.name}");
            return;
        }
        
        isBeingGrabbed = true;
        grabber = grabberTransform;
        
        // Calcular offset relativo al grip point
        grabPositionOffset = grabber.InverseTransformPoint(transform.position);
        grabRotationOffset = Quaternion.Inverse(grabber.rotation) * transform.rotation;
        
        // Configurar física durante el agarre
        rb.useGravity = false;
        rb.constraints = RigidbodyConstraints.None;
        
        // Reproducir sonido
        PlaySound(grabSound);
        
        // Resetear muestras
        currentSampleIndex = 0;
        samplesInitialized = false;
        
        if (showDebug)
            Debug.Log($"✋ VRGrabbable.OnGrab() llamado en: {gameObject.name}");
    }

    public void OnRelease(Vector3 handVelocity, Vector3 handAngularVelocity)
    {
        if (!isBeingGrabbed)
        {
            if (showDebug)
                Debug.LogWarning($"⚠️ Intento de soltar objeto que no estaba agarrado: {gameObject.name}");
            return;
        }
        
        isBeingGrabbed = false;
        
        // Restaurar física
        rb.useGravity = useGravity;
        rb.constraints = RigidbodyConstraints.None;
        
        // Calcular velocidad promedio de las muestras
        Vector3 avgVelocity = CalculateAverageVelocity();
        Vector3 avgAngularVelocity = CalculateAverageAngularVelocity();
        
        // Aplicar velocidades con multiplicadores
        rb.linearVelocity = avgVelocity * throwForceMultiplier;
        rb.angularVelocity = avgAngularVelocity * angularVelocityMultiplier;
        
        // Reproducir sonido
        PlaySound(releaseSound);
        
        if (showDebug)
            Debug.Log($"🎯 VRGrabbable.OnRelease() llamado en: {gameObject.name} | Vel: {rb.linearVelocity.magnitude:F2} m/s");
        
        grabber = null;
    }

    private Vector3 CalculateAverageVelocity()
    {
        if (!samplesInitialized) return Vector3.zero;
        
        Vector3 sum = Vector3.zero;
        for (int i = 0; i < velocitySamples.Length; i++)
        {
            sum += velocitySamples[i];
        }
        
        return sum / velocitySamples.Length;
    }

    private Vector3 CalculateAverageAngularVelocity()
    {
        if (!samplesInitialized) return Vector3.zero;
        
        Vector3 sum = Vector3.zero;
        for (int i = 0; i < angularVelocitySamples.Length; i++)
        {
            sum += angularVelocitySamples[i];
        }
        
        return sum / angularVelocitySamples.Length;
    }

    public bool IsInRange(Vector3 position)
    {
        return Vector3.Distance(transform.position, position) <= grabDistance;
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    public bool IsGrabbed => isBeingGrabbed;
    public Vector3 CurrentVelocity => CalculateAverageVelocity();
    public Vector3 CurrentAngularVelocity => CalculateAverageAngularVelocity();

    void OnDrawGizmosSelected()
    {
        Gizmos.color = isGrabbable ? Color.green : Color.red;
        Gizmos.DrawWireSphere(transform.position, grabDistance);
        
        if (isBeingGrabbed && Application.isPlaying && samplesInitialized)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, CalculateAverageVelocity());
        }
    }
}
