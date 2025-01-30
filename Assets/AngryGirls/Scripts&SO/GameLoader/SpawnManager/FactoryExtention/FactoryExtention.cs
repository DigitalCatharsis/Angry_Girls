#if (UNITY_EDITOR) 
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Angry_Girls
{
    public class FactoryExtention : MonoBehaviour
    {
        public string enumClassName = "CharacterType"; // ��� enum
        public string factoryClassName = "CharacterFactory"; // ��� ������ �������
        public string newEnumValue = "YBot_Purple";

        [TextArea]
        public string generatedEnumCode;

        [TextArea]
        public string generatedFactoryCode;

        private string factoryFilePath; // ���� �� ������� �������

        public void GenerateCode()
        {
            if (!TryFindFactoryFile(factoryClassName, out factoryFilePath))
            {
                Debug.LogError($"���� ��� ���������� ������ '{factoryClassName}' �� ������.");
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
                // ���� �� �������, ������ �� ����� �����
                guids = AssetDatabase.FindAssets($"t:Script {className}.cs");

                if (guids.Length == 0)
                {
                    Debug.LogError($"������ � ������ '{className}' �� ������.");
                    return false;
                }
            }

            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError($"�� ������� �������� ���� � ������� ��� GUID '{guids[0]}'.");
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
                    Debug.LogError($"Enum {enumName} �� ������ � ����� {filePath}");
                    return null;
                }

                var enumEndIndex = lines.FindIndex(enumStartIndex + 1, line => line.Contains("}"));
                if (enumEndIndex == -1)
                {
                    Debug.LogError($"�� ������ ������������� ������� `}}` � ����� {filePath}");
                    return null;
                }

                //���� ���� �� ��� ���� �������
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
                    Debug.LogWarning($"������� enum {newEnumValue} ��� ����������, ���������� ����������");
                    return null;
                }

                lines.Insert(enumEndIndex, $"\t{newEnumValue},");
                File.WriteAllLines(filePath, lines);
                AssetDatabase.Refresh();
                Debug.Log($"Enum {enumName} ��������, �������� {newEnumValue} � {filePath}");

                return string.Join(Environment.NewLine, lines);

            }
            catch (Exception e)
            {
                Debug.LogError($"������ ��� ��������� Enum: {e.Message}");
                return null;
            }
        }

        private string GenerateFactoryCode(string enumName, string newEnumValue, string filePath)
        {
            try
            {
                var lines = File.ReadAllLines(filePath).ToList();

                // ������� ������ �������
                int dictionaryStart = lines.FindIndex(line => line.Contains($"Dictionary<{enumName}, Func<GameObject>> Prefabs"));
                if (dictionaryStart == -1)
                {
                    Debug.LogError($"�� ������ ������� Prefabs � ����� {filePath}");
                    return null;
                }

                // ������� �������� �������
                int dictionaryEnd = lines.FindIndex(dictionaryStart, line => line.Contains("};"));
                if (dictionaryEnd == -1)
                {
                    Debug.LogError($"�� ������ ������������� ������� `}};` � ����� {filePath}");
                    return null;
                }
                //���� ���� �� ��� ���� ������� � �������
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
                    Debug.LogWarning($"������� {newEnumValue} � ������� Prefabs ��� ����������, ���������� ����������");
                    return null;
                }

                // ���������, ����� �� ��������� ������� ����� ����� �������
                string comma = "";
                if (dictionaryEnd > dictionaryStart + 1) // ���� � ������� ��� ���� ������
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
                Debug.Log($"������� Prefabs ��������, �������� {newEnumValue} � {filePath}");

                return string.Join(Environment.NewLine, lines);
            }
            catch (Exception e)
            {
                Debug.LogError($"������ ��� ��������� �������: {e.Message}");
                return null;
            }
        }
    }
}
#endif