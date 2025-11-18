using UnityEngine;
using Sunbox.Avatars;
using System.Collections;

[RequireComponent(typeof(AvatarCustomization))]
public class AvatarApplier : MonoBehaviour
{
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private bool positionFixed = false;
    
    void Awake()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        Debug.Log($"📍 Posición original guardada: {originalPosition}");
    }
    
    void Start()
    {
        Debug.Log("🎨 AvatarApplier: Iniciando aplicación...");
        
        if (GameManager.Instance == null)
        {
            Debug.LogError("❌ GameManager no encontrado! Inicia desde el Menú Principal.");
            return;
        }

        AvatarData dataToApply = GameManager.Instance.CurrentAvatarData;
        
        if (dataToApply == null)
        {
            Debug.LogError("❌ CurrentAvatarData es nulo en GameManager.");
            return;
        }

        var customizer = GetComponent<AvatarCustomization>();
        
        if (customizer == null)
        {
            Debug.LogError("❌ AvatarCustomization no encontrado en el jugador.");
            return;
        }

        StartCoroutine(ApplyAvatarSequence(customizer, dataToApply));
    }
    
    private IEnumerator ApplyAvatarSequence(AvatarCustomization customizer, AvatarData data)
    {
        // Paso 1: Aplicar los datos del avatar
        customizer.ApplyData(data);
        Debug.Log("✓ ApplyData() ejecutado");
        
        // Paso 2: Esperar 2 frames para que el modelo se genere completamente
        yield return null;
        yield return null;
        
        // Paso 3: Obtener el Animator que acaba de ser generado/actualizado
        Animator animator = customizer.Animator;
        
        if (animator == null)
        {
            Debug.LogError("❌ Animator no encontrado después de ApplyData");
            yield break;
        }
        
        // Paso 4: Configurar el Animator Controller correcto según el género
        ConfigureAnimatorController(animator, data);
        
        // Paso 5: Forzar reconexión del Animator con el esqueleto
        animator.Rebind();
        animator.Update(0f);
        Debug.Log("✓ Animator rebindeado al esqueleto");
        
        // Paso 6: Restaurar posición
        RestorePosition();
        
        // Paso 7: Esperar un frame más y verificar que todo esté correcto
        yield return null;
        
        // Verificación final
        VerifyAnimatorSetup(animator);
    }
    
    private void ConfigureAnimatorController(Animator animator, AvatarData data)
    {
        RuntimeAnimatorController controller = null;
        UnityEngine.Avatar avatarRig = null;
        
        if (data.selectedGender == Gender.Male)
        {
            controller = GameManager.Instance.maleAnimatorOverride;
            avatarRig = GameManager.Instance.maleAvatarAsset;
            Debug.Log("🚹 Configurando Animator para HOMBRE");
        }
        else
        {
            controller = GameManager.Instance.femaleAnimatorOverride;
            avatarRig = GameManager.Instance.femaleAvatarAsset;
            Debug.Log("🚺 Configurando Animator para MUJER");
        }

        // Asignar Animator Controller
        if (controller != null)
        {
            animator.runtimeAnimatorController = controller;
            Debug.Log($"✓ Animator Controller asignado: {controller.name}");
        }
        else
        {
            Debug.LogWarning("⚠️ Animator Controller es NULL en GameManager - Usando el existente");
        }

        // Asignar Avatar Rig
        if (avatarRig != null)
        {
            animator.avatar = avatarRig;
            Debug.Log($"✓ Avatar Rig asignado: {avatarRig.name}");
        }
        else
        {
            Debug.LogWarning("⚠️ Avatar Rig es NULL en GameManager - Usando el generado por Sunbox");
        }
    }
    
    private void VerifyAnimatorSetup(Animator animator)
    {
        Debug.Log("========== VERIFICACIÓN FINAL ANIMATOR ==========");
        Debug.Log($"Animator enabled: {animator.enabled}");
        Debug.Log($"Animator.avatar: {animator.avatar?.name ?? "NULL"}");
        Debug.Log($"RuntimeAnimatorController: {animator.runtimeAnimatorController?.name ?? "NULL"}");
        Debug.Log($"AnimatorController parameters: {animator.parameterCount}");
        
        if (animator.avatar == null)
        {
            Debug.LogError("❌ CRÍTICO: Animator.avatar es NULL - las animaciones NO funcionarán");
        }
        else if (!animator.avatar.isHuman)
        {
            Debug.LogError("❌ CRÍTICO: Avatar rig no es Humanoid - las animaciones NO funcionarán");
        }
        else if (!animator.avatar.isValid)
        {
            Debug.LogError("❌ CRÍTICO: Avatar rig no es válido - las animaciones NO funcionarán");
        }
        else
        {
            Debug.Log($"✅ Avatar COMPLETAMENTE configurado y VÁLIDO");
            Debug.Log($"   - Humanoid: {animator.avatar.isHuman}");
            Debug.Log($"   - Valid: {animator.avatar.isValid}");
        }
        
        Debug.Log("================================================");
    }
    
    private void RestorePosition()
    {
        if (!positionFixed)
        {
            transform.position = originalPosition;
            transform.rotation = originalRotation;
            positionFixed = true;
            Debug.Log("📍 Posición restaurada");
        }
    }
}
