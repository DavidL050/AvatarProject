#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System.IO;

public class CampusPrefabBuilder : EditorWindow
{
    // ====== MENÚ PRINCIPAL ======
    [MenuItem("Tools/Campus/Build Campus Prefab (Large)")]
    public static void BuildCampusPrefabMenu()
    {
        var go = BuildCampusGO();                     // Construye el GameObject del campus
        string prefabPath = EnsureFolder("Assets/Prefabs") + "/CampusPrefab.prefab";
        var prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
        GameObject.DestroyImmediate(go);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("Campus", "Prefab creado en:\n" + prefabPath, "OK");
        Selection.activeObject = prefab;
    }

    [MenuItem("Tools/Campus/Create Scene With Campus")]
    public static void CreateSceneWithCampus()
    {
        // Crea (o actualiza) el prefab si no existe
        string prefabsDir = EnsureFolder("Assets/Prefabs");
        string prefabPath = prefabsDir + "/CampusPrefab.prefab";
        if (!File.Exists(prefabPath))
        {
            var tmp = BuildCampusGO();
            PrefabUtility.SaveAsPrefabAsset(tmp, prefabPath);
            GameObject.DestroyImmediate(tmp);
        }

        // Crea nueva escena y coloca el campus
        var newScene = UnityEditor.SceneManagement.EditorSceneManager.NewScene(
            UnityEditor.SceneManagement.NewSceneSetup.DefaultGameObjects);

        var campusPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        var campusInstance = PrefabUtility.InstantiatePrefab(campusPrefab) as GameObject;
        campusInstance.transform.position = Vector3.zero;

        // Coloca la cámara mirando a la plaza
        var cam = Camera.main;
        if (cam != null)
        {
            cam.transform.position = new Vector3(0, 12f, -22f);
            cam.transform.rotation = Quaternion.Euler(20f, 0f, 0f);
        }

        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(newScene);
        EditorUtility.DisplayDialog("Campus", "Escena creada con el Campus.\nGuárdala y dale Play.", "OK");
    }

    // ====== CONSTRUCTOR PRINCIPAL DEL CAMPUS ======
    private static GameObject BuildCampusGO()
    {
        var campus = new GameObject("CampusPrefab");

        // Materiales
        var matsDir = EnsureFolder("Assets/Materials");
        var wallMat   = GetOrCreateMat(matsDir + "/Wall.mat",   new Color(0.78f, 0.78f, 0.78f));
        var roofMat   = GetOrCreateMat(matsDir + "/Roof.mat",   new Color(0.18f, 0.18f, 0.2f));
        var glassMat  = GetOrCreateMat(matsDir + "/Glass.mat",  new Color(0.6f, 0.9f, 1f, 0.35f), true);
        var groundMat = GetOrCreateMat(matsDir + "/Ground.mat", new Color(0.35f, 0.4f, 0.35f));
        var pathMat   = GetOrCreateMat(matsDir + "/Path.mat",   new Color(0.25f, 0.25f, 0.25f));

        // Suelo general
        var ground = CreateCube("Ground", new Vector3(120f, 0.5f, 120f), new Vector3(0f, -0.25f, 0f), groundMat, campus.transform);

        // Plaza central
        var plaza = CreateCube("Plaza", new Vector3(30f, 0.2f, 30f), new Vector3(0f, 0.1f, 0f), pathMat, campus.transform);

        // Caminos (en cruz)
        CreateCube("Path_North", new Vector3(6f, 0.2f, 35f), new Vector3(0f, 0.11f, 25f), pathMat, campus.transform);
        CreateCube("Path_South", new Vector3(6f, 0.2f, 35f), new Vector3(0f, 0.11f, -25f), pathMat, campus.transform);
        CreateCube("Path_East",  new Vector3(35f, 0.2f, 6f), new Vector3(25f, 0.11f, 0f), pathMat, campus.transform);
        CreateCube("Path_West",  new Vector3(35f, 0.2f, 6f), new Vector3(-25f, 0.11f, 0f), pathMat, campus.transform);

        // Edificios (3 rectangulares alrededor de la plaza)
        var A = BuildRectBuilding("Building_A", campus.transform, wallMat, roofMat, glassMat,
                                  footprint: new Vector3(26f, 10f, 16f),
                                  center:    new Vector3(0f, 0f, 45f),
                                  doorSide:  Side.South);

        var B = BuildRectBuilding("Building_B", campus.transform, wallMat, roofMat, glassMat,
                                  footprint: new Vector3(22f, 10f, 18f),
                                  center:    new Vector3(45f, 0f, 0f),
                                  doorSide:  Side.West);

        var C = BuildRectBuilding("Building_C", campus.transform, wallMat, roofMat, glassMat,
                                  footprint: new Vector3(28f, 10f, 14f),
                                  center:    new Vector3(-45f, 0f, 0f),
                                  doorSide:  Side.East);

        // Punto de spawn del avatar (en la plaza)
        var spawn = new GameObject("AvatarSpawnPoint");
        spawn.transform.SetParent(campus.transform);
        spawn.transform.position = new Vector3(0f, 0.2f, -8f);

        // Iluminación
        var sun = new GameObject("Directional Light");
        sun.transform.SetParent(campus.transform);
        var dl = sun.AddComponent<Light>();
        dl.type = LightType.Directional;
        dl.intensity = 1.1f;
        sun.transform.rotation = Quaternion.Euler(45f, 35f, 0f);

        AddPointLight("PlazaLight", campus.transform, new Vector3(0f, 6f, 0f), 6f, 18f);
        AddPointLight("ALight", campus.transform, A.transform.position + Vector3.up * 6f, 5f, 16f);
        AddPointLight("BLight", campus.transform, B.transform.position + Vector3.up * 6f, 5f, 16f);
        AddPointLight("CLight", campus.transform, C.transform.position + Vector3.up * 6f, 5f, 16f);

        return campus;
    }

    // ====== BUILDING GENERATOR ======
    private enum Side { North, South, East, West }

    private static GameObject BuildRectBuilding(
        string name, Transform parent, Material wallMat, Material roofMat, Material glassMat,
        Vector3 footprint, Vector3 center, Side doorSide)
    {
        // footprint.x = largo X, footprint.y = alto Y, footprint.z = ancho Z
        var root = new GameObject(name);
        root.transform.SetParent(parent);
        root.transform.position = center;

        float wX = footprint.x;
        float hY = footprint.y;
        float wZ = footprint.z;

        // Piso
        CreateCube("Floor", new Vector3(wX, 0.2f, wZ), new Vector3(0f, 0.1f, 0f), wallMat, root.transform);

        // Paredes (huecas)
        // Front (mirando +Z)
        var front = CreateCube("Wall_Front", new Vector3(wX, hY, 0.2f), new Vector3(0f, hY * 0.5f, wZ * 0.5f), wallMat, root.transform);
        // Back (-Z)
        CreateCube("Wall_Back", new Vector3(wX, hY, 0.2f), new Vector3(0f, hY * 0.5f, -wZ * 0.5f), wallMat, root.transform);
        // Left (-X)
        CreateCube("Wall_Left", new Vector3(0.2f, hY, wZ), new Vector3(-wX * 0.5f, hY * 0.5f, 0f), wallMat, root.transform);
        // Right (+X)
        CreateCube("Wall_Right", new Vector3(0.2f, hY, wZ), new Vector3(wX * 0.5f, hY * 0.5f, 0f), wallMat, root.transform);

        // Techo
        CreateCube("Roof", new Vector3(wX, 0.4f, wZ), new Vector3(0f, hY + 0.25f, 0f), roofMat, root.transform);

        // Hueco de puerta: desactivamos un "panel" pequeño en la pared elegida
        Vector3 doorPos = Vector3.zero;
        switch (doorSide)
        {
            case Side.North: doorPos = new Vector3(0f, 2f,  wZ * 0.5f + 0.11f); break;
            case Side.South: doorPos = new Vector3(0f, 2f, -wZ * 0.5f - 0.11f); break;
            case Side.East:  doorPos = new Vector3( wX * 0.5f + 0.11f, 2f, 0f); break;
            case Side.West:  doorPos = new Vector3(-wX * 0.5f - 0.11f, 2f, 0f); break;
        }
        var door = CreateCube("DoorFrame", new Vector3(2.2f, 4f, 0.2f), doorPos, roofMat, root.transform);
        // Quita el render si quieres hueco abierto
        var mr = door.GetComponent<MeshRenderer>(); if (mr) mr.enabled = false;

        // Ventanas (frente) — 3 paneles de “vidrio”
        float gap = wX / 4f;
        for (int i = -1; i <= 1; i++)
        {
            var win = CreateCube($"Window_{i+2}", new Vector3(2.2f, 2.2f, 0.1f),
                                 new Vector3(i * gap, 5f, wZ * 0.5f + 0.06f),
                                 glassMat, root.transform);
            var r = win.GetComponent<MeshRenderer>();
            if (r != null) r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }

        return root;
    }

    // ====== HELPERS ======
    private static GameObject CreateCube(string name, Vector3 scale, Vector3 pos, Material mat, Transform parent)
    {
        var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
        go.name = name;
        go.transform.SetParent(parent);
        go.transform.localScale = scale;
        go.transform.localPosition = pos;
        if (mat != null)
        {
            var mr = go.GetComponent<MeshRenderer>();
            if (mr != null) mr.sharedMaterial = mat;
        }
        return go;
    }

    private static void AddPointLight(string name, Transform parent, Vector3 pos, float intensity, float range)
    {
        var l = new GameObject(name);
        l.transform.SetParent(parent);
        l.transform.position = pos;
        var light = l.AddComponent<Light>();
        light.type = LightType.Point;
        light.intensity = intensity;
        light.range = range;
        light.shadows = LightShadows.Soft;
    }

    private static string EnsureFolder(string path)
    {
        // path tipo "Assets/Prefabs"
        var parts = path.Split('/');
        string cur = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            string next = cur + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(cur, parts[i]);
            cur = next;
        }
        return path;
    }

    private static Material GetOrCreateMat(string path, Color color, bool transparent = false)
    {
        var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (mat != null) return mat;

        mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
        if (mat.shader == null) mat.shader = Shader.Find("Standard");

        mat.color = color;

        if (transparent)
        {
            // Para URP Lit
            if (mat.shader.name.Contains("Universal"))
            {
                mat.SetFloat("_Surface", 1f); // Transparent
                mat.SetOverrideTag("RenderType", "Transparent");
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            }
            else
            {
                // Standard fallback
                mat.SetFloat("_Mode", 3f); // Transparent
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;
            }
            var c = mat.color; c.a = color.a; mat.color = c;
        }

        AssetDatabase.CreateAsset(mat, path);
        return mat;
    }
}
#endif
