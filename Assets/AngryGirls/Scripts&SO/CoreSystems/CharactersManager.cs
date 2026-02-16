using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace Angry_Girls
{
    public class CharactersManager : MonoBehaviour, ISaveReinitManager<DefaultSaveTemplate, CharactersSaveData, CharactersData>
    {
        private CharactersData _charactersData = new CharactersData();
        public CharactersData CharactersData => _charactersData;

        public event Action OnDataChanged;

        public void ResetManagersData()
        {
            _charactersData.ResetData();
            //OnDataChanged?.Invoke();
        }

        public async UniTask ReinitDataFromTemplateAsync(DefaultSaveTemplate template)
        {
            _charactersData.ResetData();

            var catalog = CoreManager.Instance.CharacterSettingsCatalogSO;

            // Spawn selected characters (max 6)
            int slot = 0;
            foreach (var charType in template.selectedCharacters)
            {
                if (slot >= 6 || charType == CharacterType.NULL) continue;

                var settings = catalog.GetByType(charType);
                if (settings != null)
                {
                    var profile = new CharacterProfile(settings);
                    _charactersData.InternalSetSelectedCharacter(slot, profile);
                    slot++;
                    Debug.Log($"CharactersManager: Added '{settings.name}' to selected slot {slot - 1}");
                }
                else
                {
                    Debug.Log($"CharactersManager: Cant add '{charType}' to selected slot {slot - 1}. ChractersSettings for this chartype was not found!");
                }
            }

            // Spawn available characters
            foreach (var charType in template.availableCharacters)
            {
                if (charType == CharacterType.NULL) continue;

                var settings = catalog.GetByType(charType);
                if (settings != null)
                {
                    var profile = new CharacterProfile(settings);
                    _charactersData.InternalAddToAvailable(profile);
                    Debug.Log($"CharactersManager: Added '{settings.name}' to available pool");
                }
            }

            OnDataChanged?.Invoke();
            await UniTask.CompletedTask;
        }

        public async UniTask ReinitDataFromSaveAsync(CharactersSaveData saveData)
        {
            if (_charactersData == null)
            {
                Debug.LogError("CharactersManager: _CharactersData is null");
                _charactersData = new CharactersData();
            }

            await _charactersData.UpdateFromSaveAsync(saveData);
            OnDataChanged?.Invoke();
        }

        public CharactersSaveData ConvertDataForSave()
        {
            return _charactersData.ConvertToSaveData();
        }

        #region Centralized methods of character management
        /// <summary>
        /// Add character from available pool to selected pool.
        /// Returns true if successful.
        /// </summary>
        public bool AddCharacterToSelected(CharacterProfile character)
        {
            if (character == null || _charactersData == null)
                return false;

            // Find empty slot in selected pool
            int emptySlotIndex = -1;
            for (int i = 0; i < _charactersData.SelectedCharactersPool.Count; i++)
            {
                if (_charactersData.SelectedCharactersPool[i] == null)
                {
                    emptySlotIndex = i;
                    break;
                }
            }

            if (emptySlotIndex == -1)
                return false; // Team is full

            // Find character in available pool
            if (!_charactersData.InternalRemoveFromAvailable(character))
                return false; // Character not in available pool

            // Move character
            _charactersData.InternalSetSelectedCharacter(emptySlotIndex, character);

            // Compact selected pool (shift non-null profiles to the left)
            CompactSelectedPool();

            OnDataChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// Remove character from selected pool at specified index and return to available pool.
        /// Returns true if successful.
        /// </summary>
        public bool RemoveCharacterFromSelected(int index)
        {
            if (_charactersData == null ||
                index < 0 ||
                index >= _charactersData.SelectedCharactersPool.Count ||
                _charactersData.SelectedCharactersPool[index] == null)
            {
                return false;
            }

            // Move character to available pool
            var character = _charactersData.SelectedCharactersPool[index];
            _charactersData.InternalAddToAvailable(character);
            _charactersData.InternalSetSelectedCharacter(index, null);

            // Compact selected pool (shift non-null profiles to the left)
            CompactSelectedPool();

            OnDataChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// Compact selected pool: shift all non-null profiles to the left, nulls to the right.
        /// </summary>
        private void CompactSelectedPool()
        {
            var nonNullCharacters = new System.Collections.Generic.List<CharacterProfile>();

            // Collect non-null profiles
            foreach (var profile in _charactersData.SelectedCharactersPool)
            {
                if (profile != null)
                    nonNullCharacters.Add(profile);
            }

            // Rebuild pool: non-null first, then nulls
            for (int i = 0; i < _charactersData.SelectedCharactersPool.Count; i++)
            {
                if (i < nonNullCharacters.Count)
                    _charactersData.InternalSetSelectedCharacter(i, nonNullCharacters[i]);
                else
                    _charactersData.InternalSetSelectedCharacter(i, null);
            }
        }
        #endregion
    }
}