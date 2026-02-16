using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Angry_Girls
{
    [Serializable]
    public class CharactersSaveData
    {
        public CharactersSaveData() { }

        public CharactersSaveData(CharacterProfileSaveData[] selected, List<CharacterProfileSaveData> available)
        {
            if (selected != null && selected.Length > 0)
            {
                int copyCount = Math.Min(selected.Length, selectedCharactersSlotsSaveData.Length);
                Array.Copy(selected, selectedCharactersSlotsSaveData, copyCount);
            }
            if (available != null)
            {
                availableCharacterPoolSaveData = new List<CharacterProfileSaveData>(available);
            }
        }

        public CharacterProfileSaveData[] selectedCharactersSlotsSaveData = new CharacterProfileSaveData[6];
        public List<CharacterProfileSaveData> availableCharacterPoolSaveData = new();
    }

    [Serializable]
    public class CharactersData : ISaveData<CharactersData, CharactersSaveData>
    {
        [SerializeField] private CharacterProfile[] _selectedCharactersPool = new CharacterProfile[6];
        [SerializeField] private List<CharacterProfile> _availableCharacterPool = new List<CharacterProfile>();

        /// <summary>
        /// READ-ONLY access to selected characters pool.
        /// DO NOT MODIFY DIRECTLY — use CharactersManager methods instead.
        /// Direct modification bypasses OnDataChanged event and breaks UI synchronization.
        /// </summary>
        public IReadOnlyList<CharacterProfile> SelectedCharactersPool => Array.AsReadOnly(_selectedCharactersPool);

        /// <summary>
        /// READ-ONLY access to available characters pool.
        /// DO NOT MODIFY DIRECTLY — use CharactersManager methods instead.
        /// </summary>
        public IReadOnlyList<CharacterProfile> AvailableCharacterPool => _availableCharacterPool.AsReadOnly();


        public void ResetData()
        {
            Array.Clear(_selectedCharactersPool, 0, _selectedCharactersPool.Length);
            _availableCharacterPool.Clear();
        }

        public async UniTask UpdateFromSaveAsync(CharactersSaveData saveData)
        {
            try
            {
                if (saveData == null)
                {
                    throw new Exception("CharactersData save is null");
                }

                ResetData();

                if (saveData?.availableCharacterPoolSaveData != null)
                {
                    foreach (var charProfileSaveData in saveData.availableCharacterPoolSaveData)
                    {
                        if (charProfileSaveData == null) continue;

                        var profile = new CharacterProfile();
                        await profile.UpdateFromSaveAsync(charProfileSaveData);

                        if (profile.CharacterSettings != null)
                        {
                            _availableCharacterPool.Add(profile);
                        }
                        else
                        {
                            Debug.LogWarning("CharactersData.UpdateFromSaveAsync: Skipped invalid profile (CharacterSettings is null)");
                        }
                    }
                }

                if (saveData?.selectedCharactersSlotsSaveData != null)
                {
                    for (int i = 0; i < saveData.selectedCharactersSlotsSaveData.Length && i < _selectedCharactersPool.Length; i++)
                    {
                        var charSaveData = saveData.selectedCharactersSlotsSaveData[i];

                        if (charSaveData == null || charSaveData.characterType == CharacterType.NULL)
                        {
                            _selectedCharactersPool[i] = null;
                            continue;
                        }

                        var profile = new CharacterProfile();
                        await profile.UpdateFromSaveAsync(charSaveData);

                        if (profile.CharacterSettings != null)
                        {
                            _selectedCharactersPool[i] = profile;
                        }
                        else
                        {
                            Debug.LogWarning($"CharactersData.UpdateFromSaveAsync: Skipped invalid profile in slot {i} (CharacterSettings is null)");
                            _selectedCharactersPool[i] = null;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning(ex.Message);
                throw ex;
            }
        }

        public CharactersSaveData ConvertToSaveData()
        {
            var selectedCharsSaveData = new CharacterProfileSaveData[_selectedCharactersPool.Length];

            for (int i = 0; i < _selectedCharactersPool.Length; i++)
            {
                var selectedChar = _selectedCharactersPool[i];
                if (selectedChar != null && selectedChar.CharacterSettings != null)
                {
                    selectedCharsSaveData[i] = selectedChar.ConvertToSaveData();
                }
                else
                {
                    selectedCharsSaveData[i] = null;
                }
            }

            var availableCharsDTO = new List<CharacterProfileSaveData>(_availableCharacterPool.Count);
            foreach (var availableChar in _availableCharacterPool)
            {
                if (availableChar != null && availableChar.CharacterSettings != null)
                {
                    availableCharsDTO.Add(availableChar.ConvertToSaveData());
                }
            }

            return new CharactersSaveData(selected: selectedCharsSaveData, available: availableCharsDTO);
        }

        #region internal methoods for CharactersManager
        internal void InternalSetSelectedCharacter(int index, CharacterProfile profile)
        {
            if (index >= 0 && index < _selectedCharactersPool.Length)
                _selectedCharactersPool[index] = profile;
        }

        internal void InternalAddToAvailable(CharacterProfile profile)
        {
            if (profile != null)
                _availableCharacterPool.Add(profile);
        }

        internal bool InternalRemoveFromAvailable(CharacterProfile profile)
        {
            return profile != null && _availableCharacterPool.Remove(profile);
        }

        internal void InternalClear()
        {
            Array.Clear(_selectedCharactersPool, 0, _selectedCharactersPool.Length);
            _availableCharacterPool.Clear();
        }
        #endregion
    }
}