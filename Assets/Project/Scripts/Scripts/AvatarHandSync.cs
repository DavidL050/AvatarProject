using UnityEngine;

public class AvatarHandSync : MonoBehaviour
{
    [Header("VR Controllers")]
    public Transform leftController;
    public Transform rightController;
    
    [Header("Configuración")]
    public Vector3 handPositionOffset = new Vector3(0, 0, 0.1f);
    public Vector3 handRotationOffset = Vector3.zero;
    
    [Header("Nombres de Huesos")]
    public string leftHandBoneName = "Hand_L";
    public string rightHandBoneName = "Hand_R";
    
    [Header("Debug")]
    public bool showDebug = true;
    
    private Transform leftHandBone;
    private Transform rightHandBone;
    private GameObject currentAvatar;

    void Start()
    {
        FindHandBones();
    }

    void Update()
    {
        GameObject activeAvatar = GetActiveAvatar();
        if (activeAvatar != currentAvatar)
        {
            currentAvatar = activeAvatar;
            FindHandBones();
        }
        
        SyncControllerPositions();
    }

    private GameObject GetActiveAvatar()
    {
        Transform female = transform.Find("Female_Avatar");
        if (female != null && female.gameObject.activeSelf)
            return female.gameObject;
        
        Transform male = transform.Find("Male_Avatar");
        if (male != null && male.gameObject.activeSelf)
            return male.gameObject;
        
        return null;
    }

    private void FindHandBones()
    {
        if (currentAvatar == null)
        {
            if (showDebug)
                Debug.LogWarning("No hay avatar activo");
            return;
        }
        
        leftHandBone = FindBoneRecursive(currentAvatar.transform, leftHandBoneName);
        rightHandBone = FindBoneRecursive(currentAvatar.transform, rightHandBoneName);
        
        if (showDebug)
        {
            if (leftHandBone != null)
                Debug.Log("Mano izquierda encontrada: " + leftHandBone.name);
            else
                Debug.LogWarning("No se encontró Hand_L");
                
            if (rightHandBone != null)
                Debug.Log("Mano derecha encontrada: " + rightHandBone.name);
            else
                Debug.LogWarning("No se encontró Hand_R");
        }
    }

    private Transform FindBoneRecursive(Transform parent, string boneName)
    {
        if (parent.name == boneName)
            return parent;
        
        foreach (Transform child in parent)
        {
            Transform found = FindBoneRecursive(child, boneName);
            if (found != null)
                return found;
        }
        
        return null;
    }

    private void SyncControllerPositions()
    {
        if (leftController != null && leftHandBone != null)
        {
            Vector3 targetPos = leftHandBone.position + leftHandBone.TransformDirection(handPositionOffset);
            Quaternion targetRot = leftHandBone.rotation * Quaternion.Euler(handRotationOffset);
            
            leftController.position = targetPos;
            leftController.rotation = targetRot;
        }
        
        if (rightController != null && rightHandBone != null)
        {
            Vector3 targetPos = rightHandBone.position + rightHandBone.TransformDirection(handPositionOffset);
            Quaternion targetRot = rightHandBone.rotation * Quaternion.Euler(handRotationOffset);
            
            rightController.position = targetPos;
            rightController.rotation = targetRot;
        }
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        
        if (leftHandBone != null && leftController != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(leftHandBone.position, leftController.position);
            Gizmos.DrawWireSphere(leftHandBone.position, 0.03f);
        }
        
        if (rightHandBone != null && rightController != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(rightHandBone.position, rightController.position);
            Gizmos.DrawWireSphere(rightHandBone.position, 0.03f);
        }
    }
}
