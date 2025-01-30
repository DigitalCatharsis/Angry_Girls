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
        public string enumClassName = "CharacterType"; // Имя enum
        public string factoryClassName = "CharacterFactory"; // Имя класса фабрики
        public string newEnumValue = "YBot_Purple";

        [TextArea]
        public string generatedEnumCode;

        [TextArea]
        public string generatedFactoryCode;

        private string factoryFilePath; // путь до скрипта фабрики

        public void GenerateCode()
        {
            if (!TryFindFactoryFile(factoryClassName, out factoryFilePath))
            {
                Debug.LogError($"Файл для фабричного класса '{factoryClassName}' не найден.");
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
                // если не найдено, поищем по имени файла
                guids = AssetDatabase.FindAssets($"t:Script {className}.cs");

                if (guids.Length == 0)
                {
                    Debug.LogError($"Скрипт с именем '{className}' не найден.");
                    return false;
                }
            }

            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError($"Не удалось получить путь к скрипту для GUID '{guids[0]}'.");
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
                    Debug.LogError($"Enum {enumName} не найден в файле {filePath}");
                    return null;
                }

                var enumEndIndex = lines.FindIndex(enumStartIndex + 1, line => line.Contains("}"));
                if (enumEndIndex == -1)
                {
                    Debug.LogError($"Не найден закрывающийся элемент `}}` в файле {filePath}");
                    return null;
                }

                //Ищем есть ли уже этот элемент
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
                    Debug.LogWarning($"элемент enum {newEnumValue} уже существует, пропускаем добавление");
                    return null;
                }

                lines.Insert(enumEndIndex, $"\t{newEnumValue},");
                File.WriteAllLines(filePath, lines);
                AssetDatabase.Refresh();
                Debug.Log($"Enum {enumName} обновлен, добавлен {newEnumValue} в {filePath}");

                return string.Join(Environment.NewLine, lines);

            }
            catch (Exception e)
            {
                Debug.LogError($"Ошибка при обработке Enum: {e.Message}");
                return null;
            }
        }

        private string GenerateFactoryCode(string enumName, string newEnumValue, string filePath)
        {
            try
            {
                var lines = File.ReadAllLines(filePath).ToList();

                // Находим начало словаря
                int dictionaryStart = lines.FindIndex(line => line.Contains($"Dictionary<{enumName}, Func<GameObject>> Prefabs"));
                if (dictionaryStart == -1)
                {
                    Debug.LogError($"Не найден словарь Prefabs в файле {filePath}");
                    return null;
                }

                // Находим закрытие словаря
                int dictionaryEnd = lines.FindIndex(dictionaryStart, line => line.Contains("};"));
                if (dictionaryEnd == -1)
                {
                    Debug.LogError($"Не найден закрывающийся элемент `}};` в файле {filePath}");
                    return null;
                }
                //ищем есть ли уже этот элемент в словаре
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
                    Debug.LogWarning($"элемент {newEnumValue} в словаре Prefabs уже существует, пропускаем добавление");
                    return null;
                }

                // Проверяем, нужно ли добавлять запятую перед новой записью
                string comma = "";
                if (dictionaryEnd > dictionaryStart + 1) // Если в словаре уже есть записи
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
                Debug.Log($"Словарь Prefabs обновлен, добавлен {newEnumValue} в {filePath}");

                return string.Join(Environment.NewLine, lines);
            }
            catch (Exception e)
            {
                Debug.LogError($"Ошибка при обработке фабрики: {e.Message}");
                return null;
            }
        }
    }
}
#endif