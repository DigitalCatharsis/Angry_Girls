#if (UNITY_EDITOR) 

using UnityEngine;
using UnityEditor;
using System.IO;

namespace Angry_Girls
{
    public class DynamicFactoryExtention : MonoBehaviour
    {
        public string newFactoryName;      // Название новой фабрики
        public string newEnumName;         // Название нового Enum
        public string firstEnumElement;    // Первый элемент нового Enum
        public string customNamespace;      // Пространство имен для нового кода

        public void GenerateCode()
        {
            // Генерация кода для нового Enum и фабрики в одном файле
            string code = GenerateCombinedCode(newFactoryName, newEnumName, firstEnumElement, customNamespace);
            SaveCodeToFile(code, $"{newFactoryName}.cs", customNamespace);

            // Обновляем существующие файлы (например, SpawnManager, PoolManager и PoolObjectLoader)
            UpdateSpawnManager(newFactoryName, customNamespace);
            UpdatePoolManager(newEnumName, customNamespace);
            UpdatePoolObjectLoader(newFactoryName, newEnumName, customNamespace);

            // Сообщаем об успехе
            Debug.Log($"Новая фабрика {newFactoryName} и Enum {newEnumName} успешно созданы в пространстве имен {customNamespace}!");
        }

        private string GenerateCombinedCode(string factoryName, string enumName, string firstElement, string customNamespace)
        {
            string namespaceStart = string.IsNullOrEmpty(customNamespace) ? "" : $"namespace {customNamespace}\n{{";
            string namespaceEnd = string.IsNullOrEmpty(customNamespace) ? "" : "}";

            return $@"
using System.Collections.Generic;
using System;
using UnityEngine;

{namespaceStart}
    public enum {enumName} 
    {{ 
        {firstElement} 
    }}

    public class {factoryName} : BaseFactory<{enumName}>
    {{
        protected override Dictionary<{enumName}, Func<GameObject>> Prefabs => new Dictionary<{enumName}, Func<GameObject>>()
        {{
            {{ {enumName}.{firstElement}, () => Resources.Load({enumName}.{firstElement}.ToString()) as GameObject }}
        }};
    }}
{namespaceEnd}
";
        }

        private void SaveCodeToFile(string code, string fileName, string customNamespace)
        {
            // Логика сохранения кода в файл
            string filePath = Application.dataPath + "/" + fileName;
            System.IO.File.WriteAllText(filePath, code);
            AssetDatabase.Refresh();
        }

        private void UpdateSpawnManager(string newFactoryName, string customNamespace)
        {
            string spawnManagerPath = FindScriptPath("SpawnManager");
            if (string.IsNullOrEmpty(spawnManagerPath)) return;

            string spawnManagerCode = File.ReadAllText(spawnManagerPath);

            // Добавляем поле для новой фабрики
            string newFactoryField = $"        public {newFactoryName} {newFactoryName};\n";
            int classStartIndex = spawnManagerCode.IndexOf("public class SpawnManager");
            if (classStartIndex == -1)
            {
                Debug.LogError("Cant find 'public class SpawnManager' in " + spawnManagerPath);
                return;
            }

            int insertionIndex = spawnManagerCode.IndexOf('{', classStartIndex) + 1;
            spawnManagerCode = spawnManagerCode.Insert(insertionIndex, newFactoryField);

            // Находим метод Awake
            int awakeMethodIndex = spawnManagerCode.IndexOf("private void Awake()");
            if (awakeMethodIndex == -1)
            {
                Debug.LogError("Cant find 'private void Awake()' in " + spawnManagerPath);
                return;
            }
            // Находим начало тела метода Awake
            int methodBodyStartIndex = spawnManagerCode.IndexOf("{", awakeMethodIndex) + 1;
            if (methodBodyStartIndex == 0)
            {
                Debug.LogError("Cant find methodBodyStartIndex in " + spawnManagerPath);
                return;
            }

            string newFactoryInitializationCode = $@"
            {newFactoryName} = gameObject.AddComponent(typeof({newFactoryName})) as {newFactoryName};
";
            // Ищем закрывающую скобку для вставки
            int endOfMethod = spawnManagerCode.IndexOf("}", methodBodyStartIndex);
            if (endOfMethod == -1)
            {
                Debug.LogError("Cant find endOfMethod in " + spawnManagerPath);
                return;
            }
            spawnManagerCode = spawnManagerCode.Insert(endOfMethod, newFactoryInitializationCode);
            File.WriteAllText(spawnManagerPath, spawnManagerCode);
            AssetDatabase.Refresh();
        }

        private void UpdatePoolManager(string newEnumName, string customNamespace)
        {
            string poolManagerPath = FindScriptPath("PoolManager");
            if (string.IsNullOrEmpty(poolManagerPath)) return;

            string poolManagerCode = File.ReadAllText(poolManagerPath);

            string newDictionaryCode = $@"
        [SerializedDictionary(""{newEnumName}Type"", ""Values"")]
        public SerializedDictionary<{newEnumName}, List<PoolObject>> {newEnumName}PoolDictionary = new SerializedDictionary<{newEnumName}, List<PoolObject>>();
            ";

            int insertionIndex = poolManagerCode.LastIndexOf("[SerializedDictionary");

            if (insertionIndex == -1)
            {
                int classStartIndex = poolManagerCode.IndexOf("public class PoolManager");
                if (classStartIndex == -1)
                {
                    Debug.LogError("Cant find 'public class PoolManager' in " + poolManagerPath);
                    return;
                }

                insertionIndex = poolManagerCode.IndexOf('{', classStartIndex) + 1;
            }
            else
            {

                insertionIndex = poolManagerCode.IndexOf(";", insertionIndex) + 1;
            }

            poolManagerCode = poolManagerCode.Insert(insertionIndex, newDictionaryCode);

            File.WriteAllText(poolManagerPath, poolManagerCode);
            AssetDatabase.Refresh();
        }

        private void UpdatePoolObjectLoader(string newFactoryName, string newEnumName, string customNamespace)
        {
            string poolObjectLoaderPath = FindScriptPath("PoolObjectLoader");
            if (string.IsNullOrEmpty(poolObjectLoaderPath)) return;

            string poolObjectLoaderCode = File.ReadAllText(poolObjectLoaderPath);

            // 1. Generate code for new method
            string newMethodCode = $@"
    private PoolObject Instantiate{newFactoryName}({newEnumName} objType, Vector3 position, Quaternion rotation)
    {{
        return GameLoader.Instance.spawnManager.{newFactoryName}.SpawnGameobject(objType, position, rotation).GetComponent<PoolObject>();
    }}
";

            // 2. Find the insertion point for the new method (before the last "}")
            int classClosingBraceIndex = poolObjectLoaderCode.LastIndexOf("}");
            if (classClosingBraceIndex == -1)
            {
                Debug.LogError("Couldn't find closing brace '}' for the class.");
                return;
            }

            // Find the second-to-last closing brace (assuming it's the one before the class closing brace)
            int secondToLastClosingBraceIndex = poolObjectLoaderCode.LastIndexOf("}", classClosingBraceIndex - 1);
            if (secondToLastClosingBraceIndex == -1)
            {
                Debug.LogError("Couldn't find second-to-last closing brace '}' for the class.");
                return;
            }

            poolObjectLoaderCode = poolObjectLoaderCode.Insert(secondToLastClosingBraceIndex, newMethodCode);

            // 3. Find the switch statement and insert new case (before "_ => throw")
            int switchDefaultIndex = poolObjectLoaderCode.IndexOf("_ => throw");
            if (switchDefaultIndex == -1)
            {
                Debug.LogError("'_ => throw' not found in switch statement.");
                return;
            }

            string newSwitchCase = $@"
        {newEnumName} {newEnumName.ToLower()}Type => Instantiate{newFactoryName}({newEnumName.ToLower()}Type, position, rotation),
    ";

            poolObjectLoaderCode = poolObjectLoaderCode.Insert(switchDefaultIndex, newSwitchCase);

            File.WriteAllText(poolObjectLoaderPath, poolObjectLoaderCode);
            AssetDatabase.Refresh();
        }


        private string FindScriptPath(string className)
        {
            var guids = AssetDatabase.FindAssets($"t:Script {className}");
            if (guids.Length == 0)
            {
                guids = AssetDatabase.FindAssets($"t:Script {className}.cs");
                if (guids.Length == 0)
                {
                    Debug.LogError($"Скрипт с именем '{className}' не найден.");
                    return null;
                }
            }

            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError($"Не удалось получить путь к скрипту для GUID '{guids[0]}'.");
                return null;
            }

            return path;
        }
    }
}
#endif