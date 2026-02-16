#if (UNITY_EDITOR) 
using UnityEngine;
using UnityEditor;
using System.IO;

namespace Angry_Girls
{
    /// <summary>
    /// Editor tool for dynamically generating new factory classes and enums
    /// </summary>
    public class DynamicFactoryExtention : MonoBehaviour
    {
        public string newFactoryName;
        public string newEnumName;
        public string firstEnumElement;
        public string customNamespace;

        /// <summary>
        /// Generates code for new factory and enum
        /// </summary>
        public void GenerateCode()
        {
            string code = GenerateCombinedCode(newFactoryName, newEnumName, firstEnumElement, customNamespace);
            SaveCodeToFile(code, $"{newFactoryName}.cs", customNamespace);

            UpdateSpawnManager(newFactoryName, customNamespace);
            UpdatePoolManager(newEnumName, customNamespace);
            UpdatePoolObjectLoader(newFactoryName, newEnumName, customNamespace);

            Debug.Log($"New factory {newFactoryName} and Enum {newEnumName} created in namespace {customNamespace}!");
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
            string filePath = Application.dataPath + "/" + fileName;
            System.IO.File.WriteAllText(filePath, code);
            AssetDatabase.Refresh();
        }

        private void UpdateSpawnManager(string newFactoryName, string customNamespace)
        {
            string spawnManagerPath = FindScriptPath("SpawnManager");
            if (string.IsNullOrEmpty(spawnManagerPath)) return;

            string spawnManagerCode = File.ReadAllText(spawnManagerPath);

            string newFactoryField = $"        public {newFactoryName} {newFactoryName};\n";
            int classStartIndex = spawnManagerCode.IndexOf("public class SpawnManager");
            if (classStartIndex == -1)
            {
                Debug.LogError("Can't find 'public class SpawnManager' in " + spawnManagerPath);
                return;
            }

            int insertionIndex = spawnManagerCode.IndexOf('{', classStartIndex) + 1;
            spawnManagerCode = spawnManagerCode.Insert(insertionIndex, newFactoryField);

            int awakeMethodIndex = spawnManagerCode.IndexOf("private void Awake()");
            if (awakeMethodIndex == -1)
            {
                Debug.LogError("Can't find 'private void Awake()' in " + spawnManagerPath);
                return;
            }

            int methodBodyStartIndex = spawnManagerCode.IndexOf("{", awakeMethodIndex) + 1;
            if (methodBodyStartIndex == 0)
            {
                Debug.LogError("Can't find methodBodyStartIndex in " + spawnManagerPath);
                return;
            }

            string newFactoryInitializationCode = $@"
            {newFactoryName} = gameObject.AddComponent(typeof({newFactoryName})) as {newFactoryName};
";

            int endOfMethod = spawnManagerCode.IndexOf("}", methodBodyStartIndex);
            if (endOfMethod == -1)
            {
                Debug.LogError("Can't find endOfMethod in " + spawnManagerPath);
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
                    Debug.LogError("Can't find 'public class PoolManager' in " + poolManagerPath);
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

            string newMethodCode = $@"
    private PoolObject Instantiate{newFactoryName}({newEnumName} objType, Vector3 position, Quaternion rotation)
    {{
        return GameLoader.Instance.spawnManager.{newFactoryName}.SpawnGameobject(objType, position, rotation).GetComponent<PoolObject>();
    }}
";

            int classClosingBraceIndex = poolObjectLoaderCode.LastIndexOf("}");
            if (classClosingBraceIndex == -1)
            {
                Debug.LogError("Couldn't find closing brace '}' for the class.");
                return;
            }

            int secondToLastClosingBraceIndex = poolObjectLoaderCode.LastIndexOf("}", classClosingBraceIndex - 1);
            if (secondToLastClosingBraceIndex == -1)
            {
                Debug.LogError("Couldn't find second-to-last closing brace '}' for the class.");
                return;
            }

            poolObjectLoaderCode = poolObjectLoaderCode.Insert(secondToLastClosingBraceIndex, newMethodCode);

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
                    Debug.LogError($"Script '{className}' not found.");
                    return null;
                }
            }

            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError($"Couldn't get script path for GUID '{guids[0]}'.");
                return null;
            }

            return path;
        }
    }
}
#endif