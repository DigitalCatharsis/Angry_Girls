using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;

namespace Angry_Girls
{
    public static class Repository
    {
        private const string _gameStateKey = "SaveSlot_0";
        private static Dictionary<string, string> _currentState = new Dictionary<string, string>();

        public static bool LoadState()
        {
            if (PlayerPrefs.HasKey(_gameStateKey))
            {
                try
                {
                    var serializedState = PlayerPrefs.GetString(_gameStateKey);
                    _currentState = JsonConvert.DeserializeObject<Dictionary<string, string>>(serializedState)
                        ?? new Dictionary<string, string>();
                    Debug.Log($"Repository.LoadState: Loaded {_currentState.Count} entries from PlayerPrefs");
                    return true;
                }
                catch (JsonException e)
                {
                    Debug.LogError("Repository: Failed to load game state: " + e.Message);
                    _currentState = new Dictionary<string, string>();
                    return false;
                }
            }
            else
            {
                _currentState = new Dictionary<string, string>();
                Debug.Log("Repository.LoadState: No save file found (new game)");
                return false;
            }
        }

        public static void SaveState()
        {
            try
            {
                var serializedState = JsonConvert.SerializeObject(_currentState);
                PlayerPrefs.SetString(_gameStateKey, serializedState);
                PlayerPrefs.Save();
                Debug.Log($"Repository.SaveState: Saved {_currentState.Count} entries to PlayerPrefs");
            }
            catch (JsonException e)
            {
                Debug.LogError("Repository: Failed to save game state: " + e.Message);
            }
        }

        public static T GetData<T>()
        {
            var key = typeof(T).Name;
            if (!_currentState.TryGetValue(key, out var serializedData) || string.IsNullOrEmpty(serializedData))
            {
                Debug.Log($"Repository.GetData<{key}>: No data found");
                return default;
            }

            try
            {
                Debug.Log($"Repository.GetData<{key}>: Starting deserialization of {serializedData.Length} chars");

                var result = JsonConvert.DeserializeObject<T>(serializedData);
                Debug.Log($"Repository.GetData<{key}>: Successfully loaded");
                return result;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Repository.GetData<{key}> FAILED: {e.Message}\n{e.StackTrace}\nSerialized data preview: {serializedData.Substring(0, Mathf.Min(500, serializedData.Length))}");
                return default;
            }
        }

        public static void SetData<T>(T value) where T : class
        {
            var key = typeof(T).Name;
            try
            {
                var serializedData = JsonConvert.SerializeObject(value);
                _currentState[key] = serializedData;
                Debug.Log($"Repository.SetData<{key}>: Saved data (size: {serializedData.Length} chars)");
            }
            catch (JsonException e)
            {
                Debug.LogError($"Repository: Failed to serialize {key}: {e.Message}");
            }
        }

        public static void ResetState()
        {
            _currentState.Clear();
            PlayerPrefs.DeleteKey(_gameStateKey);
            PlayerPrefs.Save();
            Debug.Log("Repository.ResetState: Game state reset");
        }
    }
}