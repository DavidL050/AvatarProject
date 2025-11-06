// SaveSystem.cs
using UnityEngine;
using System.IO;

public static class SaveSystem
{
    public static string GetSavePath(string profileID)
    {
        return Path.Combine(Application.persistentDataPath, profileID + ".json");
    }

    public static void SaveAvatar(AvatarData data, string profileID)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(GetSavePath(profileID), json);
        Debug.Log("Avatar guardado en: " + GetSavePath(profileID));
    }

    public static AvatarData LoadAvatar(string profileID)
    {
        string path = GetSavePath(profileID);
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            return JsonUtility.FromJson<AvatarData>(json);
        }
        else
        {
            // Si no hay archivo, devuelve datos por defecto
            return new AvatarData(); 
        }
    }
}