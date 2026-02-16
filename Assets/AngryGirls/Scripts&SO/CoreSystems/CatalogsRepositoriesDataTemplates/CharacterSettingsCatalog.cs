using UnityEngine;
using System.Collections.Generic;

namespace Angry_Girls
{
    /// <summary>
    /// Immutable catalog of character templates stored as ScriptableObject.
    /// Automatically builds dictionary from CharacterSettings.characterType field.
    /// No manual CharacterType assignment required in inspector.
    /// </summary>
    [CreateAssetMenu(fileName = "CharacterSettingsCatalog", menuName = "Angry_Girls/Catalog/CharacterSettingsCatalog")]
    public class CharacterSettingsCatalogSO : ScriptableObject
    {
        [Header("Inspector Setup")]
        [Tooltip("Drag all CharacterSettings assets here. Dictionary will be built automatically from their characterType field.")]
        [SerializeField] private List<CharacterSettings> _characterSettingsList = new();

        // Runtime dictionary built from _characterSettingsList
        private Dictionary<CharacterType, CharacterSettings> _catalog;

        /// <summary>
        /// Build catalog dictionary from list on first access (lazy initialization).
        /// </summary>
        private void EnsureCatalogBuilt()
        {
            if (_catalog != null)
                return;

            _catalog = new Dictionary<CharacterType, CharacterSettings>();

            foreach (var settings in _characterSettingsList)
            {
                if (settings == null)
                {
                    Debug.LogWarning($"CharacterSettingsCatalogSO: Null entry in _characterSettingsList at index {_characterSettingsList.IndexOf(settings)}");
                    continue;
                }

                if (settings.characterType == CharacterType.NULL)
                {
                    Debug.LogWarning($"CharacterSettingsCatalogSO: CharacterSettings '{settings.name}' has NULL characterType. Skipping.");
                    continue;
                }

                if (_catalog.ContainsKey(settings.characterType))
                {
                    Debug.LogWarning($"CharacterSettingsCatalogSO: Duplicate CharacterType {settings.characterType} found for '{settings.name}'. First entry retained.");
                    continue;
                }

                _catalog[settings.characterType] = settings;
            }

            Debug.Log($"CharacterSettingsCatalogSO: Built catalog with {_catalog.Count} valid entries from {_characterSettingsList.Count} assets");
        }

        /// <summary>
        /// Get character settings by type.
        /// </summary>
        public CharacterSettings GetByType(CharacterType type)
        {
            EnsureCatalogBuilt();
            return _catalog.TryGetValue(type, out var settings) ? settings : null;
        }

        /// <summary>
        /// Get all character types for debug UI (sorted by name).
        /// </summary>
        public (CharacterType type, string name)[] GetAllTypesForDebug()
        {
            EnsureCatalogBuilt();

            var list = new List<(CharacterType type, string name)>();
            foreach (var kvp in _catalog)
            {
                if (kvp.Value != null)
                {
                    list.Add((kvp.Key, kvp.Value.name));
                }
            }
            list.Sort((a, b) => a.name.CompareTo(b.name));
            return list.ToArray();
        }
    }
}