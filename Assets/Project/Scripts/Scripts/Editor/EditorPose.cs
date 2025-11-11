using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif


[ExecuteAlways]
public class EditorIdlePreview : MonoBehaviour
{
    [Header("Avatares")]
    public Animator maleAnimator;
    public Animator femaleAnimator;

    [Header("Clip Idle (o el que quieras mostrar)")]
    public AnimationClip previewClip;

    [Header("Opciones")]
    public bool runInEditor = true;
    public bool applyAlsoInPlay = false;
    [Range(0.1f, 3f)] public float speed = 1f;

    void Update()
    {
        if (previewClip == null) return;

        // Solo aplicamos en modo editor (o en Play si lo pides explícitamente)
        if (!Application.isPlaying || applyAlsoInPlay)
        {
            float time = (float)(EditorApplication.timeSinceStartup * speed % previewClip.length);

            if (maleAnimator != null)
                previewClip.SampleAnimation(maleAnimator.gameObject, time);

            if (femaleAnimator != null)
                previewClip.SampleAnimation(femaleAnimator.gameObject, time);

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                if (maleAnimator)   EditorUtility.SetDirty(maleAnimator.gameObject);
                if (femaleAnimator) EditorUtility.SetDirty(femaleAnimator.gameObject);
            }
#endif
        }
    }
}
