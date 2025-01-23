using UnityEngine;
using UnityEditor;
using System.IO;

namespace Angry_Girls
{
    public class DynamicFactoryExtention : MonoBehaviour
    {
        public string newFactoryName;      // �������� ����� �������
        public string newEnumName;         // �������� ������ Enum
        public string firstEnumElement;    // ������ ������� ������ Enum

        public void GenerateCode()
        {
            // ��������� ���� ��� ������ Enum
            string enumCode = GenerateEnumCode(newEnumName, firstEnumElement);
            SaveCodeToFile(enumCode, $"{newEnumName}.cs");

            // ��������� ���� ��� ����� �������
            string factoryCode = GenerateFactoryCode(newFactoryName, newEnumName);
            SaveCodeToFile(factoryCode, $"{newFactoryName}.cs");

            // ��������� ������������ ����� (��������, PoolManager � PoolObjectLoader)
            UpdatePoolManager(newEnumName);
            UpdatePoolObjectLoader(newFactoryName, newEnumName);

            // �������� �� ������
            Debug.Log($"����� ������� {newFactoryName} � Enum {newEnumName} ������� �������!");
        }

        private string GenerateEnumCode(string enumName, string firstElement)
        {
            // ������ ��������� ���� ��� ������ Enum
            return $"public enum {enumName} {{ {firstElement} }}";
        }

        private string GenerateFactoryCode(string factoryName, string enumName)
        {
            // ������ ��������� ���� ��� ����� �������
            return $@"public class {factoryName} : BaseFactory<{enumName}>
{{
    protected override Dictionary<{enumName}, Func<GameObject>> Prefabs => new Dictionary<{enumName}, Func<GameObject>>()
    {{
        {{ {enumName}.{firstEnumElement}, () => Resources.Load({enumName}.{firstEnumElement}.ToString()) as GameObject }}
    }};
}}";
        }

        private void SaveCodeToFile(string code, string fileName)
        {
            // ������ ���������� ���� � ����
            string filePath = Application.dataPath + "/" + fileName;
            System.IO.File.WriteAllText(filePath, code);
            AssetDatabase.Refresh();
        }

        private void UpdatePoolManager(string newEnumName)
        {
            // ������ ������� ���� PoolManager
            string poolManagerPath = "Assets/Scripts/PoolManager.cs";
            string poolManagerCode = File.ReadAllText(poolManagerPath);

            // ��������� ����� �������
            string newDictionaryCode = $@"
                [SerializedDictionary(""{newEnumName}Type"", ""Values"")]
                public SerializedDictionary<{newEnumName}, List<PoolObject>> {newEnumName}PoolDictionary = new SerializedDictionary<{newEnumName}, List<PoolObject>>();
            ";

            // ��������� ����� ������� ����� ���������� �������
            int lastDictionaryIndex = poolManagerCode.LastIndexOf("[SerializedDictionary");
            poolManagerCode = poolManagerCode.Insert(lastDictionaryIndex, newDictionaryCode);

            // ��������� ��������� ����
            File.WriteAllText(poolManagerPath, poolManagerCode);
            AssetDatabase.Refresh();
        }

        private void UpdatePoolObjectLoader(string newFactoryName, string newEnumName)
        {
            // ������ ������� ���� PoolObjectLoader
            string poolObjectLoaderPath = "Assets/Scripts/PoolObjectLoader.cs";
            string poolObjectLoaderCode = File.ReadAllText(poolObjectLoaderPath);

            // ��������� ����� ����� ��� ��������������� �������� �� ����� �������
            string newMethodCode = $@"
                private PoolObject Instantiate{newFactoryName}({newEnumName} objType, Vector3 position, Quaternion rotation)
                {{
                    return GameLoader.Instance.spawnManager.{newFactoryName}.SpawnGameobject(objType, position, rotation).GetComponent<PoolObject>();
                }}
            ";

            // ��������� ����� ����� ����� ���������� ������
            int lastMethodIndex = poolObjectLoaderCode.LastIndexOf("private PoolObject");
            poolObjectLoaderCode = poolObjectLoaderCode.Insert(lastMethodIndex, newMethodCode);

            // ��������� ��������� ����
            File.WriteAllText(poolObjectLoaderPath, poolObjectLoaderCode);
            AssetDatabase.Refresh();
        }
    }
}