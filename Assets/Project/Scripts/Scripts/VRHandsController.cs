using UnityEngine;
using UnityEngine.XR;
using System.Collections.Generic;

public class VRHandController : MonoBehaviour
{
    [Header("Configuración de Mano")]
    [Tooltip("¿Es la mano derecha? (false = izquierda)")]
    public bool isRightHand = true;
    
    [Header("Detección de Objetos")]
    [Tooltip("Radio de búsqueda de objetos agarrables")]
    public float grabRadius = 2.0f;
    
    [Tooltip("Layers que pueden ser agarrados")]
    public LayerMask grabbableLayer = -1; // -1 = Everything
    
    [Tooltip("Máximo número de objetos a detectar")]
    public int maxDetectionCount = 10;
    
    [Header("Punto de Agarre")]
    [Tooltip("Transform donde se posiciona el objeto agarrado")]
    public Transform gripPoint;
    
    [Header("Feedback Visual")]
    [Tooltip("Renderer de la mano para feedback visual")]
    public Renderer handRenderer;
    
    [Tooltip("Color cuando hay objeto cercano")]
    public Color nearObjectColor = Color.yellow;
    
    [Tooltip("Color cuando se agarra objeto")]
    public Color grabbingColor = Color.green;
    
    private Color originalHandColor;
    
    [Header("Vibración")]
    [Tooltip("Intensidad de vibración al agarrar (0-1)")]
    [Range(0f, 1f)]
    public float grabHapticIntensity = 0.3f;
    
    [Tooltip("Duración de vibración al agarrar")]
    public float grabHapticDuration = 0.1f;
    
    [Header("Simulación (Sin VR)")]
    [Tooltip("Tecla para agarrar con mano izquierda")]
    public KeyCode leftGrabKey = KeyCode.G;
    
    [Tooltip("Tecla para agarrar con mano derecha")]
    public KeyCode rightGrabKey = KeyCode.H;
    
    [Tooltip("Activar modo simulación (para testing sin VR)")]
    public bool useSimulationMode = true;
    
    [Header("Debug")]
    [Tooltip("Mostrar mensajes de debug en consola")]
    public bool showDebug = true;
    
    // Estado del controlador VR
    private InputDevice controller;
    private bool controllerInitialized = false;
    
    // Estado de agarre
    private VRGrabbable currentGrabbedObject;
    private VRGrabbable nearestObject;
    private bool gripButtonPressed = false;
    private bool wasGripPressed = false;
    
    // Tracking de velocidad
    private Vector3 previousPosition;
    private Quaternion previousRotation;
    private Vector3 handVelocity;
    private Vector3 handAngularVelocity;
    
    // Buffer para detección
    private Collider[] detectionBuffer;
    
    // Simulación
    private bool isVRActive = false;
    private int debugFrameCounter = 0;

    void Awake()
    {
        detectionBuffer = new Collider[maxDetectionCount];
        
        if (gripPoint == null)
        {
            CreateGripPoint();
        }
        
        if (handRenderer != null)
        {
            originalHandColor = handRenderer.material.color;
        }
    }

    private void CreateGripPoint()
    {
        Transform existingGrip = transform.Find("GripPoint");
        
        if (existingGrip != null)
        {
            gripPoint = existingGrip;
            if (showDebug)
                Debug.Log($"✓ GripPoint encontrado en {(isRightHand ? "Right" : "Left")} Controller");
        }
        else
        {
            GameObject grip = new GameObject("GripPoint");
            grip.transform.SetParent(transform);
            
            // Posición adelante del controller
            grip.transform.localPosition = new Vector3(0, 0, 0.5f);
            grip.transform.localRotation = Quaternion.identity;
            gripPoint = grip.transform;
            
            if (showDebug)
                Debug.Log($"🔧 GripPoint creado para {(isRightHand ? "Right" : "Left")} Controller");
        }
    }

    void Start()
    {
        InitializeController();
        previousPosition = gripPoint.position;
        previousRotation = gripPoint.rotation;
        
        if (showDebug)
        {
            Debug.Log($"🤚 VRHandController inicializado: {(isRightHand ? "Derecha" : "Izquierda")}");
            Debug.Log($"   - Grab Radius: {grabRadius}");
            Debug.Log($"   - Grabbable Layer: {LayerMaskToString(grabbableLayer)}");
        }
        
        if (useSimulationMode && !isVRActive)
        {
            if (showDebug)
                Debug.Log($"⌨️ Modo simulación activado - Tecla: {(isRightHand ? rightGrabKey : leftGrabKey)}");
        }
    }

    void Update()
    {
        debugFrameCounter++;
        
        if (!controllerInitialized && useSimulationMode)
            InitializeController();
        
        // Usar VR o teclado según disponibilidad
        if (isVRActive)
        {
            UpdateInput();
        }
        else if (useSimulationMode)
        {
            UpdateSimulationInput();
        }
        
        DetectNearbyObjects();
        HandleGrabRelease();
        UpdateHandVelocity();
        UpdateVisualFeedback();
    }

    private void InitializeController()
    {
        InputDeviceCharacteristics characteristics = InputDeviceCharacteristics.Controller;
        characteristics |= isRightHand ? InputDeviceCharacteristics.Right : InputDeviceCharacteristics.Left;
        
        var devices = new List<InputDevice>();
        InputDevices.GetDevicesWithCharacteristics(characteristics, devices);
        
        if (devices.Count > 0)
        {
            controller = devices[0];
            controllerInitialized = true;
            isVRActive = true;
            if (showDebug)
                Debug.Log($"✓ Controlador {(isRightHand ? "derecho" : "izquierdo")} conectado: {controller.name}");
        }
        else
        {
            isVRActive = false;
        }
    }

    private void UpdateInput()
    {
        if (!controller.isValid) return;
        
        wasGripPressed = gripButtonPressed;
        controller.TryGetFeatureValue(CommonUsages.gripButton, out gripButtonPressed);
    }

    private void UpdateSimulationInput()
    {
        wasGripPressed = gripButtonPressed;
        
        KeyCode grabKey = isRightHand ? rightGrabKey : leftGrabKey;
        gripButtonPressed = Input.GetKey(grabKey);
        
        if (gripButtonPressed && !wasGripPressed && showDebug)
        {
            Debug.Log($"⌨️ Tecla {grabKey} presionada - {(isRightHand ? "Derecha" : "Izquierda")}");
        }
    }

    private void DetectNearbyObjects()
    {
        if (currentGrabbedObject != null) return;
        
        int hitCount = Physics.OverlapSphereNonAlloc(
            gripPoint.position,
            grabRadius,
            detectionBuffer,
            grabbableLayer
        );
        
        // Debug periódico cada 2 segundos
        if (showDebug && debugFrameCounter % 120 == 0)
        {
            Debug.Log($"🔍 {(isRightHand ? "Derecha" : "Izquierda")} - Radio: {grabRadius}m - Detectados: {hitCount} objetos");
            Debug.Log($"   Posición GripPoint: {gripPoint.position}");
        }
        
        VRGrabbable closest = null;
        float closestDistance = float.MaxValue;
        
        for (int i = 0; i < hitCount; i++)
        {
            VRGrabbable grabbable = detectionBuffer[i].GetComponent<VRGrabbable>();
            
            if (grabbable != null)
            {
                if (showDebug && debugFrameCounter % 120 == 0)
                {
                    Debug.Log($"   - Objeto encontrado: {detectionBuffer[i].gameObject.name} | isGrabbable: {grabbable.isGrabbable} | IsGrabbed: {grabbable.IsGrabbed}");
                }
                
                if (grabbable.isGrabbable && !grabbable.IsGrabbed)
                {
                    float distance = Vector3.Distance(gripPoint.position, grabbable.transform.position);
                    
                    if (distance < closestDistance)
                    {
                        closest = grabbable;
                        closestDistance = distance;
                    }
                }
            }
            else
            {
                // El objeto no tiene VRGrabbable
                if (showDebug && debugFrameCounter % 120 == 0)
                {
                    Debug.Log($"   - Objeto sin VRGrabbable: {detectionBuffer[i].gameObject.name}");
                }
            }
        }
        
        if (closest != null && nearestObject != closest && showDebug)
        {
            Debug.Log($"✨ {(isRightHand ? "Derecha" : "Izquierda")} detectó objeto agarrable: {closest.gameObject.name} a {closestDistance:F2}m");
        }
        
        nearestObject = closest;
    }

    private void HandleGrabRelease()
    {
        bool gripJustPressed = gripButtonPressed && !wasGripPressed;
        bool gripJustReleased = !gripButtonPressed && wasGripPressed;
        
        if (gripJustPressed)
        {
            if (currentGrabbedObject == null && nearestObject != null)
            {
                GrabObject(nearestObject);
            }
            else if (showDebug)
            {
                if (currentGrabbedObject != null)
                    Debug.Log($"⚠️ Ya hay un objeto agarrado: {currentGrabbedObject.gameObject.name}");
                else if (nearestObject == null)
                    Debug.Log($"⚠️ No hay objetos cercanos para agarrar");
            }
        }
        
        if (gripJustReleased && currentGrabbedObject != null)
        {
            ReleaseObject();
        }
    }

    private void GrabObject(VRGrabbable grabbable)
    {
        currentGrabbedObject = grabbable;
        currentGrabbedObject.OnGrab(gripPoint);
        
        if (isVRActive)
        {
            SendHapticImpulse(grabHapticIntensity, grabHapticDuration);
        }
        
        if (showDebug)
            Debug.Log($"✋ Objeto agarrado por {(isRightHand ? "DERECHA" : "IZQUIERDA")}: {grabbable.gameObject.name}");
    }

    private void ReleaseObject()
    {
        if (currentGrabbedObject == null) return;
        
        currentGrabbedObject.OnRelease(handVelocity, handAngularVelocity);
        
        if (isVRActive)
        {
            SendHapticImpulse(grabHapticIntensity * 0.5f, grabHapticDuration * 0.5f);
        }
        
        if (showDebug)
            Debug.Log($"🎯 Objeto lanzado por {(isRightHand ? "DERECHA" : "IZQUIERDA")}: {currentGrabbedObject.gameObject.name}");
        
        currentGrabbedObject = null;
    }

    private void UpdateHandVelocity()
    {
        handVelocity = (gripPoint.position - previousPosition) / Time.deltaTime;
        
        Quaternion deltaRotation = gripPoint.rotation * Quaternion.Inverse(previousRotation);
        deltaRotation.ToAngleAxis(out float angle, out Vector3 axis);
        angle *= Mathf.Deg2Rad;
        handAngularVelocity = axis * (angle / Time.deltaTime);
        
        previousPosition = gripPoint.position;
        previousRotation = gripPoint.rotation;
    }

    private void UpdateVisualFeedback()
    {
        if (handRenderer == null) return;
        
        Color targetColor = originalHandColor;
        
        if (currentGrabbedObject != null)
            targetColor = grabbingColor;
        else if (nearestObject != null)
            targetColor = nearObjectColor;
        
        handRenderer.material.color = Color.Lerp(
            handRenderer.material.color,
            targetColor,
            Time.deltaTime * 10f
        );
    }

    private void SendHapticImpulse(float amplitude, float duration)
    {
        if (controller.isValid)
        {
            controller.SendHapticImpulse(0, amplitude, duration);
        }
    }

    private string LayerMaskToString(LayerMask mask)
    {
        if (mask.value == -1) return "Everything";
        
        List<string> layers = new List<string>();
        for (int i = 0; i < 32; i++)
        {
            if ((mask.value & (1 << i)) != 0)
            {
                string layerName = LayerMask.LayerToName(i);
                if (!string.IsNullOrEmpty(layerName))
                    layers.Add(layerName);
            }
        }
        return layers.Count > 0 ? string.Join(", ", layers) : "Nothing";
    }

    public bool IsGrabbing => currentGrabbedObject != null;
    public VRGrabbable GrabbedObject => currentGrabbedObject;
    public bool HasNearbyObject => nearestObject != null;

    void OnDrawGizmos()
    {
        if (gripPoint == null) return;
        
        // Radio de detección con colores
        if (currentGrabbedObject != null)
            Gizmos.color = Color.green;
        else if (nearestObject != null)
            Gizmos.color = Color.yellow;
        else
            Gizmos.color = new Color(1, 0, 0, 0.3f); // Rojo semi-transparente
        
        Gizmos.DrawWireSphere(gripPoint.position, grabRadius);
        
        // Punto central del grip
        Gizmos.color = Color.white;
        Gizmos.DrawSphere(gripPoint.position, 0.05f);
        
        // Línea hacia objeto más cercano
        if (Application.isPlaying && nearestObject != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(gripPoint.position, nearestObject.transform.position);
        }
        
        // Velocidad si hay objeto agarrado
        if (Application.isPlaying && currentGrabbedObject != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(gripPoint.position, handVelocity * 0.1f);
        }
    }
}
