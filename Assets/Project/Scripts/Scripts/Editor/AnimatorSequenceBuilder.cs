using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

public class AnimatorSequenceBuilder : EditorWindow
{
    private Animator targetAnimator;
    private AnimationClip idleClip;
    private AnimationClip action1Clip;
    private AnimationClip action2Clip;
    private float idleSeconds = 3f;
    private float crossfade = 0.25f;

    [MenuItem("Tools/Animator Sequence/Create 3-State Controller")]
    public static void ShowWindow()
    {
        GetWindow<AnimatorSequenceBuilder>("Animator Sequence");
    }

    void OnGUI()
    {
        GUILayout.Label("Animator Target", EditorStyles.boldLabel);
        targetAnimator = (Animator)EditorGUILayout.ObjectField("Animator", targetAnimator, typeof(Animator), true);

        GUILayout.Space(10);
        GUILayout.Label("Clips", EditorStyles.boldLabel);
        idleClip    = (AnimationClip)EditorGUILayout.ObjectField("Idle Clip",    idleClip,    typeof(AnimationClip), false);
        action1Clip = (AnimationClip)EditorGUILayout.ObjectField("Action 1 Clip", action1Clip, typeof(AnimationClip), false);
        action2Clip = (AnimationClip)EditorGUILayout.ObjectField("Action 2 Clip", action2Clip, typeof(AnimationClip), false);

        GUILayout.Space(10);
        idleSeconds = EditorGUILayout.FloatField("Idle Seconds (before A1)", idleSeconds);
        crossfade   = EditorGUILayout.FloatField("Crossfade Duration",        crossfade);

        GUILayout.Space(20);
        if (GUILayout.Button("Create Controller + Wire PlaySequence"))
        {
            if (targetAnimator == null || idleClip == null || action1Clip == null || action2Clip == null)
            {
                EditorUtility.DisplayDialog("Error", "Assign Animator and all three clips.", "OK");
                return;
            }

            CreateControllerAndWire(targetAnimator, idleClip, action1Clip, action2Clip, idleSeconds, crossfade);
        }
    }

    static void CreateControllerAndWire(Animator anim, AnimationClip idle, AnimationClip a1, AnimationClip a2, float idleSecs, float crossfade)
    {
        // === Ruta fija solicitada ===
        string root   = "Assets/David";
        string folder = "Animations";

        // Crear carpetas si no existen
        if (!AssetDatabase.IsValidFolder("Assets/David"))
            AssetDatabase.CreateFolder("Assets", "David");
        if (!AssetDatabase.IsValidFolder($"{root}/{folder}"))
            AssetDatabase.CreateFolder(root, folder);

        // Crear controller con nombre único
        string path = AssetDatabase.GenerateUniqueAssetPath($"{root}/{folder}/AvatarController.controller");
        var controller = AnimatorController.CreateAnimatorControllerAtPath(path);

        var sm = controller.layers[0].stateMachine;

        var sIdle = sm.AddState("Idle");
        var sA1   = sm.AddState("Action1");
        var sA2   = sm.AddState("Action2");

        sIdle.motion = idle;
        sA1.motion   = a1;
        sA2.motion   = a2;
        sm.defaultState = sIdle;

        // Transiciones (usa hasFixedDuration en lugar de fixedDuration)
        var t1 = sIdle.AddTransition(sA1);
        t1.hasExitTime = true; t1.exitTime = 0.99f; t1.duration = crossfade; t1.hasFixedDuration = false;

        var t2 = sA1.AddTransition(sA2);
        t2.hasExitTime = true; t2.exitTime = 0.99f; t2.duration = crossfade; t2.hasFixedDuration = false;

        var t3 = sA2.AddTransition(sIdle);
        t3.hasExitTime = true; t3.exitTime = 0.99f; t3.duration = crossfade; t3.hasFixedDuration = false;

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        // Asignar al Animator y conectar PlaySequence
        if (anim != null)
        {
            anim.runtimeAnimatorController = controller;

            var ps = anim.gameObject.GetComponent<PlaySequence>();
            if (!ps) ps = anim.gameObject.AddComponent<PlaySequence>();

            ps.animator      = anim;
            ps.idleState     = "Idle";
            ps.action1State  = "Action1";
            ps.action2State  = "Action2";
            ps.idleDuration  = idleSecs;

            EditorUtility.SetDirty(anim.gameObject);
        }

        EditorUtility.DisplayDialog("Done",
            $"Controller creado en:\n{path}\n\nAsignado al Animator y PlaySequence configurado.",
            "OK");
    }
}

