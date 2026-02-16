using UnityEditor;
using UnityEngine;

public class ResaveTool
{
    [MenuItem("Tools/Reimport All Manual")]
    public static void ReimportAll()
    {
        AssetDatabase.ImportAsset("Assets", ImportAssetOptions.ImportRecursive | ImportAssetOptions.ForceUpdate);
        Debug.Log("Принудительный переимпорт завершен!");
    }
}