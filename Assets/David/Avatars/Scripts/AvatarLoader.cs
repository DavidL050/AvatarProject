
using UnityEngine;
using System.IO;

public class AvatarLoader : MonoBehaviour
{
    [Header("Referencias de Modelos")]
    [Tooltip("Arrastra aquí el GameObject del avatar masculino (debe ser hijo de este objeto)")]
    public GameObject maleAvatarObject;
    [Tooltip("Arrastra aquí el GameObject del avatar femenino (debe ser hijo de este objeto)")]
    public GameObject femaleAvatarObject;

    [Header("Referencia de Cámara")]
    public Camera playerCamera;

    private GameObject persistentAvatar;

    void Awake()
    {
        
        maleAvatarObject.SetActive(false);
        femaleAvatarObject.SetActive(false);

        // Buscamos si hay un avatar persistente (que vino de la escena de creación)
        GameObject existingAvatar = GameObject.FindWithTag("PlayerAvatar");
        if (existingAvatar != null)
        {
            persistentAvatar = existingAvatar;
            persistentAvatar.SetActive(true);
            SetupCamera();
            Debug.Log("Avatar persistente cargado correctamente.");
            return;
        }

        // Si no hay avatar persistente, usamos el loader clásico con JSON
        LoadAvatarFromJSON();
    }

    private void LoadAvatarFromJSON()
    {
        string path = Path.Combine(Application.persistentDataPath, "avatarData.json");

        if (File.Exists(path))
        {
            string jsonData = File.ReadAllText(path);
            AvatarData loadedData = JsonUtility.FromJson<AvatarData>(jsonData);

            if (loadedData.selectedGender == Gender.Male)
            {
                maleAvatarObject.SetActive(true);
                persistentAvatar = maleAvatarObject;
            }
            else
            {
                femaleAvatarObject.SetActive(true);
                persistentAvatar = femaleAvatarObject;
            }

            // Aplicamos los blendshapes
            SkinnedMeshRenderer targetMesh = persistentAvatar.GetComponentInChildren<SkinnedMeshRenderer>();
            ApplyDataToAvatar(loadedData, targetMesh);

            // Asignamos tag para futuras escenas
            persistentAvatar.tag = "PlayerAvatar";

            SetupCamera();
            Debug.Log("Avatar cargado desde JSON correctamente.");
        }
        else
        {
            Debug.LogWarning("No se encontró archivo de guardado. Cargando avatar femenino por defecto.");
            femaleAvatarObject.SetActive(true);
            persistentAvatar = femaleAvatarObject;
            persistentAvatar.tag = "PlayerAvatar";
            SetupCamera();
        }
    }

    private void ApplyDataToAvatar(AvatarData data, SkinnedMeshRenderer targetMesh)
    {
        targetMesh.SetBlendShapeWeight(0, data.bodyFat * 100f);
        targetMesh.SetBlendShapeWeight(1, data.bodyMuscle * 100f);

        if (data.selectedGender == Gender.Female)
            targetMesh.SetBlendShapeWeight(2, data.breastSize * 100f);
    }

    private void SetupCamera()
    {
        if (playerCamera == null)
        {
            Debug.LogWarning("No se ha asignado la cámara del jugador.");
            return;
        }

        // Colocamos la cámara como hijo del avatar
        playerCamera.transform.SetParent(persistentAvatar.transform);

        // Ajustamos posición y rotación inicial (puedes modificar según tu prefab)
        playerCamera.transform.localPosition = new Vector3(0, 1.6f, 0); // altura promedio ojos
        playerCamera.transform.localRotation = Quaternion.identity;

        // Activamos la cámara
        playerCamera.gameObject.SetActive(true);
    }
}
