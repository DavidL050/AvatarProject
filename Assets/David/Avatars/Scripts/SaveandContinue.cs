using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SaveAndContinue : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameObject avatarRoot;        // Arrastra aquí el root del avatar personalizado
    [SerializeField] private GameObject customizationUI;   // (Opcional) UI de personalización para ocultar

    [Header("Scene")]
    [SerializeField] private string targetSceneName = "Proyecto_General"; // Nombre EXACTO en Build Settings

    [Header("Loading (opcional)")]
    [SerializeField] private CanvasGroup loadingOverlay;   // (Opcional) pantalla de carga (CanvasGroup)

    public void OnSaveAndContinue()
    {
        if (avatarRoot == null)
        {
            Debug.LogError("[SaveAndContinue] Falta asignar avatarRoot.");
            return;
        }

        // Asegura que exista el Carrier
        if (AvatarCarrier.I == null)
        {
            var go = new GameObject("__AvatarCarrier__");
            go.AddComponent<AvatarCarrier>();
        }

        // Adjunta el avatar para que sobreviva al cambio de escena
        AvatarCarrier.I.Attach(avatarRoot);

        // Oculta la UI de personalización (opcional)
        if (customizationUI != null) customizationUI.SetActive(false);

        // Muestra overlay de carga (opcional)
        if (loadingOverlay != null)
        {
            loadingOverlay.alpha = 1f;
            loadingOverlay.blocksRaycasts = true;
            loadingOverlay.interactable = false;
        }

        // Carga la escena destino
        StartCoroutine(LoadTargetScene());
    }

    private IEnumerator LoadTargetScene()
    {
        var op = SceneManager.LoadSceneAsync(targetSceneName, LoadSceneMode.Single);
        while (!op.isDone)
            yield return null;
    }
}
