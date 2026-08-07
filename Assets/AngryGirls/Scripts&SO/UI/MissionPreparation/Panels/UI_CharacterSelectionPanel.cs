using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Angry_Girls
{
    /// <summary>
    /// Panel for selecting characters for the team.
    /// Subscribes to CharactersManager.OnDataChanged and properly unsubscribes on destroy.
    /// </summary>
    public class UI_CharacterSelectionPanel : MonoBehaviour, IUIPanel
    {
        [Header("UI References")]
        [SerializeField] private Transform _availableCharactersContainer;
        [SerializeField] private Transform _selectedCharactersContainer;
        [SerializeField] private GameObject _characterSlotPrefab;
        [SerializeField] private TextMeshProUGUI _statsText;

        [Header("Limits")]
        [SerializeField] private int _maxSelectedCharacters = 6;

        private List<UI_CharacterSlot> _availableCharacterSlots = new List<UI_CharacterSlot>();
        private List<UI_CharacterSlot> _selectedCharacterSlots = new List<UI_CharacterSlot>();
        private CharactersManager _charactersManager;
        private IAssetProvider _assetProvider;

        private bool _isDestroyed = false;

        /// <summary>
        /// Initialize the panel and subscribe to data changes.
        /// </summary>
        public void Initialize(CoreManager coreManager)
        {
            _charactersManager = coreManager.CharactersManager;
            _assetProvider = coreManager.AddressableAssetManager;

            // Subscribe to data changes
            _charactersManager.OnDataChanged += OnCharactersDataChanged;

            CreateSelectedCharacterSlots();
            Refresh();
        }

        /// <summary>
        /// Callback for CharactersManager.OnDataChanged.
        /// Guards against destroyed state.
        /// </summary>
        private void OnCharactersDataChanged()
        {
            if (_isDestroyed) return;
            if (this == null) return;

            Refresh();
        }

        /// <summary>
        /// Refresh the panel display.
        /// </summary>
        public void Refresh()
        {
            if (_isDestroyed) return;
            UpdateCharacterDisplay();
        }

        private void CreateSelectedCharacterSlots()
        {
            // Clear existing selected slots
            ClearSlotList(_selectedCharacterSlots);

            // Create fixed number of selected character slots
            for (int i = 0; i < _maxSelectedCharacters; i++)
            {
                var slotGO = Instantiate(_characterSlotPrefab, _selectedCharactersContainer);
                var slot = slotGO.GetComponent<UI_CharacterSlot>();
                if (slot != null)
                {
                    SubscribeSlot(slot);
                    _selectedCharacterSlots.Add(slot);
                }
            }
        }

        private void UpdateCharacterDisplay()
        {
            if (_isDestroyed) return;
            if (_charactersManager == null) return;

            var charactersData = _charactersManager.CharactersData;
            if (charactersData == null) return;

            UpdateAvailableCharacters(charactersData.AvailableCharacterPool);
            UpdateSelectedCharacters(charactersData.SelectedCharactersPool);
        }

        private void UpdateAvailableCharacters(IReadOnlyList<CharacterProfile> availableCharacters)
        {
            if (_isDestroyed) return;

            // Remove excess slots
            while (_availableCharacterSlots.Count > availableCharacters.Count)
            {
                var last = _availableCharacterSlots[_availableCharacterSlots.Count - 1];
                UnsubscribeSlot(last);
                if (last != null && last.gameObject != null)
                    Destroy(last.gameObject);
                _availableCharacterSlots.RemoveAt(_availableCharacterSlots.Count - 1);
            }

            // Add missing slots
            while (_availableCharacterSlots.Count < availableCharacters.Count)
            {
                var slotGO = Instantiate(_characterSlotPrefab, _availableCharactersContainer);
                var slot = slotGO.GetComponent<UI_CharacterSlot>();
                if (slot != null)
                {
                    SubscribeSlot(slot);
                    _availableCharacterSlots.Add(slot);
                }
            }

            // Update slot contents with null guards
            for (int i = 0; i < _availableCharacterSlots.Count; i++)
            {
                var slot = _availableCharacterSlots[i];
                // Guard against destroyed slots
                if (slot == null || slot.gameObject == null)
                {
                    // Recreate destroyed slot
                    var slotGO = Instantiate(_characterSlotPrefab, _availableCharactersContainer);
                    slot = slotGO.GetComponent<UI_CharacterSlot>();
                    if (slot != null)
                    {
                        SubscribeSlot(slot);
                        _availableCharacterSlots[i] = slot;
                    }
                    else
                    {
                        continue;
                    }
                }

                if (i < availableCharacters.Count)
                {
                    slot.SetCharacter(availableCharacters[i], CharacterSlotType.Available).Forget();
                }
                else
                {
                    slot.Clear();
                }
            }
        }

        private void UpdateSelectedCharacters(IReadOnlyList<CharacterProfile> selectedCharacters)
        {
            if (_isDestroyed) return;

            for (int i = 0; i < _selectedCharacterSlots.Count; i++)
            {
                var slot = _selectedCharacterSlots[i];
                // Guard against destroyed slots
                if (slot == null || slot.gameObject == null)
                {
                    continue;
                }

                if (i < selectedCharacters.Count)
                {
                    slot.SetCharacter(selectedCharacters[i], CharacterSlotType.Selected).Forget();
                }
                else
                {
                    slot.Clear();
                }
            }
        }

        private void SubscribeSlot(UI_CharacterSlot slot)
        {
            if (slot == null) return;
            slot.OnClicked += OnCharacterSlotClicked;
            slot.OnPointerEntered += OnSlotPointerEnter;
            slot.OnPointerExited += OnSlotPointerExit;
        }

        private void UnsubscribeSlot(UI_CharacterSlot slot)
        {
            if (slot == null) return;
            slot.OnClicked -= OnCharacterSlotClicked;
            slot.OnPointerEntered -= OnSlotPointerEnter;
            slot.OnPointerExited -= OnSlotPointerExit;
        }

        private void ClearSlotList(List<UI_CharacterSlot> slots)
        {
            foreach (var slot in slots)
            {
                UnsubscribeSlot(slot);
                if (slot != null && slot.gameObject != null)
                    Destroy(slot.gameObject);
            }
            slots.Clear();
        }

        private void OnCharacterSlotClicked(UI_CharacterSlot slot)
        {
            if (_isDestroyed) return;
            if (slot == null || slot.Character == null) return;

            var character = slot.Character;
            var slotType = slot.SlotType;

            switch (slotType)
            {
                case CharacterSlotType.Available:
                    if (_charactersManager.AddCharacterToSelected(character))
                        UIManager.Instance.ShowNotification("Character added to team!", 0.5f);
                    else
                        UIManager.Instance.ShowNotification("Team is full or character not available!", 0.5f);
                    break;

                case CharacterSlotType.Selected:
                    int selectedIndex = _selectedCharacterSlots.IndexOf(slot);
                    if (selectedIndex >= 0 && _charactersManager.RemoveCharacterFromSelected(selectedIndex))
                        UIManager.Instance.ShowNotification("Character removed from team!", 0.5f);
                    else
                        UIManager.Instance.ShowNotification("Failed to remove character!", 0.5f);
                    break;
            }
        }

        private void OnSlotPointerEnter(UI_CharacterSlot slot)
        {
            if (_isDestroyed || slot == null) return;
            // Show character preview/stats on hover
            if (slot.Character != null && _statsText != null)
            {
                var baseStats = slot.Character.GetSettingsStats;
                var itemsStats = slot.Character.GetItemsStats;
                _statsText.text = CharactersStatsBase.GetColoredText(baseStats, itemsStats);
            }
        }

        private void OnSlotPointerExit(UI_CharacterSlot slot)
        {
            if (_isDestroyed) return;
            // Reset stats display
            if (_statsText != null)
            {
                _statsText.text = CharactersStatsBase.GetEmptyText();
            }
        }

        /// <summary>
        /// Check if enough characters are selected.
        /// </summary>
        public bool HasSelectedCharacters()
        {
            if (_charactersManager == null) return false;
            var data = _charactersManager.CharactersData;
            if (data == null) return false;

            int count = 0;
            foreach (var c in data.SelectedCharactersPool)
            {
                if (c != null) count++;
            }
            return count > 0;
        }

        /// <summary>
        /// Properly unsubscribe from all events on destroy.
        /// </summary>
        private void OnDestroy()
        {
            _isDestroyed = true;

            // Unsubscribe from CharactersManager events
            if (_charactersManager != null)
            {
                _charactersManager.OnDataChanged -= OnCharactersDataChanged;
            }

            // Unsubscribe all slots
            foreach (var slot in _availableCharacterSlots)
            {
                UnsubscribeSlot(slot);
            }
            foreach (var slot in _selectedCharacterSlots)
            {
                UnsubscribeSlot(slot);
            }

            _availableCharacterSlots.Clear();
            _selectedCharacterSlots.Clear();
        }
    }
}