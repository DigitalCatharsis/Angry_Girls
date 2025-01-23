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

        public void GenerateCode()
        {
            // Генерация кода для нового Enum
            string enumCode = GenerateEnumCode(newEnumName, firstEnumElement);
            SaveCodeToFile(enumCode, $"{newEnumName}.cs");

            // Генерация кода для новой фабрики
            string factoryCode = GenerateFactoryCode(newFactoryName, newEnumName);
            SaveCodeToFile(factoryCode, $"{newFactoryName}.cs");

            // Обновляем существующие файлы (например, PoolManager и PoolObjectLoader)
            UpdatePoolManager(newEnumName);
            UpdatePoolObjectLoader(newFactoryName, newEnumName);

            // Сообщаем об успехе
            Debug.Log($"Новая фабрика {newFactoryName} и Enum {newEnumName} успешно созданы!");
        }

        private string GenerateEnumCode(string enumName, string firstElement)
        {
            // Логика генерации кода для нового Enum
            return $"public enum {enumName} {{ {firstElement} }}";
        }

        private string GenerateFactoryCode(string factoryName, string enumName)
        {
            // Логика генерации кода для новой фабрики
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
            // Логика сохранения кода в файл
            string filePath = Application.dataPath + "/" + fileName;
            System.IO.File.WriteAllText(filePath, code);
            AssetDatabase.Refresh();
        }

        private void UpdatePoolManager(string newEnumName)
        {
            // Читаем текущий файл PoolManager
            string poolManagerPath = "Assets/Scripts/PoolManager.cs";
            string poolManagerCode = File.ReadAllText(poolManagerPath);

            // Добавляем новый словарь
            string newDictionaryCode = $@"
                [SerializedDictionary(""{newEnumName}Type"", ""Values"")]
                public SerializedDictionary<{newEnumName}, List<PoolObject>> {newEnumName}PoolDictionary = new SerializedDictionary<{newEnumName}, List<PoolObject>>();
            ";

            // Вставляем новый словарь после последнего словаря
            int lastDictionaryIndex = poolManagerCode.LastIndexOf("[SerializedDictionary");
            poolManagerCode = poolManagerCode.Insert(lastDictionaryIndex, newDictionaryCode);

            // Сохраняем изменённый файл
            File.WriteAllText(poolManagerPath, poolManagerCode);
            AssetDatabase.Refresh();
        }

        private void UpdatePoolObjectLoader(string newFactoryName, string newEnumName)
        {
            // Читаем текущий файл PoolObjectLoader
            string poolObjectLoaderPath = "Assets/Scripts/PoolObjectLoader.cs";
            string poolObjectLoaderCode = File.ReadAllText(poolObjectLoaderPath);

            // Добавляем новый метод для инстанцирования объектов из новой фабрики
            string newMethodCode = $@"
                private PoolObject Instantiate{newFactoryName}({newEnumName} objType, Vector3 position, Quaternion rotation)
                {{
                    return GameLoader.Instance.spawnManager.{newFactoryName}.SpawnGameobject(objType, position, rotation).GetComponent<PoolObject>();
                }}
            ";

            // Вставляем новый метод после последнего метода
            int lastMethodIndex = poolObjectLoaderCode.LastIndexOf("private PoolObject");
            poolObjectLoaderCode = poolObjectLoaderCode.Insert(lastMethodIndex, newMethodCode);

            // Сохраняем изменённый файл
            File.WriteAllText(poolObjectLoaderPath, poolObjectLoaderCode);
            AssetDatabase.Refresh();
        }
    }
}