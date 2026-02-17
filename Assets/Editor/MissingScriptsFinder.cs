using UnityEditor;
using UnityEngine;

public class MissingScriptsFinder
{
    [MenuItem("Tools/Find All Missing Scripts")]
    public static void FindAllMissing()
    {
        // 1. Checking PREFABS (on disk) 
        string[] prefabs = AssetDatabase.FindAssets("t:Prefab");
        foreach (string guid in prefabs)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (go == null) continue;

            Component[] components = go.GetComponentsInChildren<Component>(true);
            foreach (Component c in components)
            {
                if (c == null) Debug.LogError($"Missing Script in PREFAB: {path}", go);
            }
        }

        // 2. Checking OBJECTS ON THE SCENE (in the hierarchy)
        GameObject[] sceneObjects = GameObject.FindObjectsOfType<GameObject>(true);
        foreach (GameObject go in sceneObjects)
        {
            Component[] components = go.GetComponents<Component>();
            foreach (Component c in components)
            {
                if (c == null) Debug.LogError($"Missing Script on THE SCENE: {go.name}", go);
            }
        }

        Debug.Log("Global search completed. Check the console for red errors.");
    }
}