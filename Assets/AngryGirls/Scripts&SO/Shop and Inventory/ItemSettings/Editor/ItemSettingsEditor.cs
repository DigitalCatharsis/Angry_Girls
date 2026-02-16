using UnityEditor;
using UnityEngine;

namespace Angry_Girls
{
#if UNITY_EDITOR
    /// <summary>
    /// Custom editor for ItemSettings to provide a button for renewing the UniqueId.
    /// </summary>
    [CustomEditor(typeof(ItemSettings))]
    public class ItemSettingsEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            ItemSettings itemSettings = (ItemSettings)target;

            GUILayout.Space(10);

            GUIContent renewButtonContent = new GUIContent("Renew UniqueId", "Generates a new unique identifier for this ItemSettings asset. Use this after duplicating an asset via Ctrl+D to ensure uniqueness.");
            if (GUILayout.Button(renewButtonContent))
            {
                System.Reflection.FieldInfo uniqueIdField = typeof(ItemSettings).GetField("_uniqueId", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                if (uniqueIdField != null)
                {
                    string newId = System.Guid.NewGuid().ToString();
                    uniqueIdField.SetValue(itemSettings, newId);
                    EditorUtility.SetDirty(itemSettings);
                    AssetDatabase.SaveAssetIfDirty(itemSettings);

                    Debug.Log($"ItemSettingsEditor: Generated new UniqueId '{newId}' for asset '{itemSettings.name}'.");
                }
                else
                {
                    Debug.LogError($"ItemSettingsEditor: Could not find private field '_uniqueId' in ItemSettings class for asset '{itemSettings.name}'.");
                }
            }

            EditorGUILayout.LabelField("Current UniqueId:", itemSettings.UniqueId);
        }
    }
#endif
}