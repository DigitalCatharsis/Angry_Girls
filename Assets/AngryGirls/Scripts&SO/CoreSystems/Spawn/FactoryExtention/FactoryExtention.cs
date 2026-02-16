#if (UNITY_EDITOR) 
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Editor tool for extending existing factory classes with new enum values
    /// </summary>
    public class FactoryExtention : MonoBehaviour
    {
        public string enumClassName = "CharacterType";
        public string factoryClassName = "CharacterFactory";
        public string newEnumValue = "YBot_Purple";

        [TextArea]
        public string generatedEnumCode;

        [TextArea]
        public string generatedFactoryCode;

        private string factoryFilePath;

        /// <summary>
        /// Generates and adds new enum value to existing factory
        /// </summary>
        public void GenerateCode()
        {
            if (!TryFindFactoryFile(factoryClassName, out factoryFilePath))
            {
                Debug.LogError($"Factory class file '{factoryClassName}' not found.");
                return;
            }

            generatedEnumCode = GenerateEnumCode(enumClassName, newEnumValue, factoryFilePath);
            generatedFactoryCode = GenerateFactoryCode(enumClassName, newEnumValue, factoryFilePath);
        }

        private bool TryFindFactoryFile(string className, out string filePath)
        {
            filePath = null;

            var guids = AssetDatabase.FindAssets($"t:Script {className}");
            if (guids.Length == 0)
            {
                guids = AssetDatabase.FindAssets($"t:Script {className}.cs");

                if (guids.Length == 0)
                {
                    Debug.LogError($"Script '{className}' not found.");
                    return false;
                }
            }

            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError($"Couldn't get script path for GUID '{guids[0]}'.");
                return false;
            }

            filePath = path;

            return true;
        }

        private string GenerateEnumCode(string enumName, string newEnumValue, string filePath)
        {
            try
            {
                var lines = File.ReadAllLines(filePath).ToList();
                var enumStartIndex = lines.FindIndex(line => line.Contains($"enum {enumName}"));

                if (enumStartIndex == -1)
                {
                    Debug.LogError($"Enum {enumName} not found in file {filePath}");
                    return null;
                }

                var enumEndIndex = lines.FindIndex(enumStartIndex + 1, line => line.Contains("}"));
                if (enumEndIndex == -1)
                {
                    Debug.LogError($"Closing brace '}}' not found in file {filePath}");
                    return null;
                }

                bool enumValueExists = false;
                for (int i = enumStartIndex + 1; i < enumEndIndex; i++)
                {
                    if (lines[i].Trim().StartsWith(newEnumValue))
                    {
                        enumValueExists = true;
                        break;
                    }
                }

                if (enumValueExists)
                {
                    Debug.LogWarning($"Enum value {newEnumValue} already exists, skipping addition");
                    return null;
                }

                lines.Insert(enumEndIndex, $"\t{newEnumValue},");
                File.WriteAllLines(filePath, lines);
                AssetDatabase.Refresh();
                Debug.Log($"Enum {enumName} updated, added {newEnumValue} in {filePath}");

                return string.Join(Environment.NewLine, lines);

            }
            catch (Exception e)
            {
                Debug.LogError($"Error processing Enum: {e.Message}");
                return null;
            }
        }

        private string GenerateFactoryCode(string enumName, string newEnumValue, string filePath)
        {
            try
            {
                var lines = File.ReadAllLines(filePath).ToList();

                int dictionaryStart = lines.FindIndex(line => line.Contains($"Dictionary<{enumName}, Func<GameObject>> Prefabs"));
                if (dictionaryStart == -1)
                {
                    Debug.LogError($"Prefabs dictionary not found in file {filePath}");
                    return null;
                }

                int dictionaryEnd = lines.FindIndex(dictionaryStart, line => line.Contains("};"));
                if (dictionaryEnd == -1)
                {
                    Debug.LogError($"Closing brace '}};' not found in file {filePath}");
                    return null;
                }

                bool prefabValueExists = false;
                for (int i = dictionaryStart + 1; i < dictionaryEnd; i++)
                {
                    if (lines[i].Trim().StartsWith($"{{ {enumName}.{newEnumValue}"))
                    {
                        prefabValueExists = true;
                        break;
                    }
                }
                if (prefabValueExists)
                {
                    Debug.LogWarning($"Prefab value {newEnumValue} already exists in dictionary, skipping addition");
                    return null;
                }

                string comma = "";
                if (dictionaryEnd > dictionaryStart + 1)
                {
                    bool foundExistingEntry = false;
                    for (int i = dictionaryStart + 1; i < dictionaryEnd; i++)
                    {
                        if (lines[i].Trim().StartsWith("{"))
                        {
                            foundExistingEntry = true;
                            break;
                        }
                    }
                    if (foundExistingEntry)
                    {
                        comma = ",";
                    }
                }

                lines.Insert(dictionaryEnd, $"{comma}\n\t{{ {enumName}.{newEnumValue}, () => Resources.Load({enumName}.{newEnumValue}.ToString()) as GameObject }}");
                File.WriteAllLines(filePath, lines);
                AssetDatabase.Refresh();
                Debug.Log($"Prefabs dictionary updated, added {newEnumValue} in {filePath}");

                return string.Join(Environment.NewLine, lines);
            }
            catch (Exception e)
            {
                Debug.LogError($"Error processing factory: {e.Message}");
                return null;
            }
        }
    }
}
#endif