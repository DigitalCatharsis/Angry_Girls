using UnityEditor;
using UnityEngine;

public class FixMissingScripts : EditorWindow
{
    [MenuItem("Tools/Fix Missing Scripts by Name")]
    public static void LookForMissingScripts()
    {
        // Находим все префабы в проекте
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab");
        foreach (var guid in prefabGuids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            // Проверяем компоненты
            var components = prefab.GetComponentsInChildren<Component>(true);
            bool changed = false;

            foreach (var comp in components)
            {
                if (comp == null) // Вот он, Missing Script!
                {
                    Debug.LogWarning($"Нашел Missing в префабе: {path}. Попробуйте перетащить вручную или использовать скрипт восстановления связей.");
                    changed = true;
                }
            }
            if (changed) EditorUtility.SetDirty(prefab);
        }
        AssetDatabase.SaveAssets();
        Debug.Log("Поиск завершен. Проверьте консоль.");
    }
}