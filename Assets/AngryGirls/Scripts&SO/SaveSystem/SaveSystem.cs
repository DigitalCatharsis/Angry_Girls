using System.IO;
using UnityEngine;

namespace Angry_Girls
{
    public static class SaveSystem
    {
        private static string SavePath => Application.persistentDataPath + "/save.json";

        public static void Save(SaveData data)
        {
            var json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SavePath, json);
        }

        public static SaveData Load()
        {
            if (!File.Exists(SavePath)) return new SaveData();
            var json = File.ReadAllText(SavePath);
            return JsonUtility.FromJson<SaveData>(json);
        }
    }
}