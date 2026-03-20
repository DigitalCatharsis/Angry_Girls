using UnityEditor;
using UnityEngine;

public class FixMissingScripts : EditorWindow
{
    [MenuItem("Tools/Fix Missing Scripts by Name")]
    public static void LookForMissingScripts()
    {
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        foreach (var guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            var components = prefab.GetComponentsInChildren<Component>(true);
            bool changed = false;

            foreach (var comp in components)
            {
                if (comp == null)
                {
                    Debug.LogWarning($"Found Missing in prefab: {path}. Try dragging it manually or using the relink script.");
                    changed = true;
                }
            }
            if (changed) EditorUtility.SetDirty(prefab);
        }
        AssetDatabase.SaveAssets();
        Debug.Log("Search completed. Check console.");
    }
}