using UnityEngine;
using UnityEditor;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;

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

            // 2. Find the insertion point (just before the last "}")
            int insertionPoint = poolObjectLoaderCode.LastIndexOf("}");

            // 3. Check for method duplication
            if (poolObjectLoaderCode.Contains($"Instantiate{newFactoryName}"))
            {
                Debug.LogWarning($"Method Instantiate{newFactoryName} already exists in PoolObjectLoader. Skipping insertion.");
                return;
            }

            // 4. Insert new method
            poolObjectLoaderCode = poolObjectLoaderCode.Insert(insertionPoint, newMethodCode);

            // 5. Find the switch statement and insert new case
            int switchCaseIndex = poolObjectLoaderCode.IndexOf("objType switch");
            if (switchCaseIndex == -1)
            {
                Debug.LogError("objType switch");
                return;
            }

            int switchBodyStartIndex = poolObjectLoaderCode.IndexOf("{", switchCaseIndex) + 1;
            if (switchBodyStartIndex == 0)
            {
                Debug.LogError("Cant find switchBodyStartIndex in " + poolObjectLoaderPath);
                return;
            }


            int caseEndIndex = -1;
            int tempIndex = switchBodyStartIndex;
            while (true)
            {

                tempIndex = poolObjectLoaderCode.IndexOf("case", tempIndex);

                if (tempIndex == -1)
                {
                    break;
                }

                caseEndIndex = poolObjectLoaderCode.IndexOf("=>", tempIndex);
                if (caseEndIndex == -1)
                {
                    tempIndex = poolObjectLoaderCode.IndexOf("case", tempIndex + 1);

                }
                else
                {
                    tempIndex = caseEndIndex + 1;
                }

            }

            if (caseEndIndex == -1)
            {
                caseEndIndex = poolObjectLoaderCode.IndexOf("default", switchBodyStartIndex);

                if (caseEndIndex == -1)
                {
                    caseEndIndex = poolObjectLoaderCode.IndexOf("}", switchBodyStartIndex);
                    if (caseEndIndex == -1)
                    {
                        Debug.LogError("Cant find case end in switch(objType) in PoolObjectLoader");
                        return;
                    }
                }

            }


            string newSwitchCase = $@"
                 {newEnumName} {newEnumName.ToLower()}Type => Instantiate{newFactoryName}({newEnumName.ToLower()}Type, position, rotation),
            ";
            poolObjectLoaderCode = poolObjectLoaderCode.Insert(caseEndIndex, newSwitchCase);

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
