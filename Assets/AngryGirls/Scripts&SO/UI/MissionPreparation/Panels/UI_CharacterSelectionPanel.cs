using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Panel for character selection in mission preparation.
    /// All character data modifications MUST go through CharactersManager.
    /// Direct modification of CharactersData pools is prohibited.
    /// </summary>
    public class UI_CharacterSelectionPanel : MonoBehaviour, IUIPanel
    {
        [Header("UI References")]
        [SerializeField] private Transform _availableCharactersContainer;
        [SerializeField] private Transform _selectedCharactersContainer;
        [SerializeField] private GameObject _characterSlotPrefab;
        [SerializeField] private GameObject _previewPanel;
        [SerializeField] private TextMeshProUGUI _characterNameText;
        [SerializeField] private TextMeshProUGUI _characterStatsText;
        [SerializeField] private TextMeshProUGUI _creditsText;

        [Header("Data")]
        [SerializeField] private int _maxSelectedCharacters = 6;

        private List<UI_CharacterSlot> _availableCharacterSlots = new List<UI_CharacterSlot>();
        private List<UI_CharacterSlot> _selectedCharacterSlots = new List<UI_CharacterSlot>();
        private CharactersManager _charactersManager;

        /// <summary>
        /// Initialize the panel and subscribe to data change events.
        /// </summary>
        public void Initialize(CoreManager coreManager)
        {
            _charactersManager = coreManager.CharactersManager;

            // Subscribe to data changes for automatic UI updates
            _charactersManager.OnDataChanged += Refresh;

            CreateSelectedCharacterSlots();
            UpdateCharacterDisplay();
        }

        /// <summary>
        /// Refresh the panel display when character data changes.
        /// </summary>
        public void Refresh()
        {
            UpdateCharacterDisplay();
        }

        private void CreateSelectedCharacterSlots()
        {
            // Clean up existing selected slots
            ClearSlots(_selectedCharacterSlots);

            // Create selected character slots (fixed count = 6)
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
        private void ClearSlots(List<UI_CharacterSlot> slots)
        {
            foreach (var slot in slots)
            {
                if (slot != null && slot.gameObject != null)
                {
                    UnsubscribeSlot(slot);
                    Destroy(slot.gameObject);
                }
            }
            slots.Clear();
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

        private void UpdateCharacterDisplay()
        {
            if (_charactersManager == null) return;

            var charactersData = _charactersManager.CharactersData;
            if (charactersData == null) return;

            // Update available characters (dynamic count)
            UpdateAvailableCharacters(charactersData.AvailableCharacterPool);

            // Update selected characters (compact fill - left-aligned, no gaps)
            var compactSelected = charactersData.SelectedCharactersPool.Where(p => p != null).ToArray();

            //for (int i = _selectedCharacterSlots.Count - 1; i >=0 ; i--)
            for (int i = 0; i < _selectedCharacterSlots.Count; i++)
            {
                if (i < compactSelected.Length)
                    _selectedCharacterSlots[i].SetCharacter(compactSelected[i], CharacterSlotType.Selected).Forget();
                else
                    _selectedCharacterSlots[i].Clear();
            }
        }

        private UI_CharacterSlot CreateSlot(Transform parent)
        {
            var slotGO = Instantiate(_characterSlotPrefab, parent);
            return slotGO.GetComponent<UI_CharacterSlot>();
        }

        private void UpdateAvailableCharacters(IReadOnlyList<CharacterProfile> availableCharacters)
        {
            // Remove excess slots
            while (_availableCharacterSlots.Count > availableCharacters.Count)
            {
                var last = _availableCharacterSlots[_availableCharacterSlots.Count - 1];
                UnsubscribeSlot(last);
                Destroy(last.gameObject);
                _availableCharacterSlots.RemoveAt(_availableCharacterSlots.Count - 1);
            }

            // Add missing slots
            while (_availableCharacterSlots.Count < availableCharacters.Count)
            {
                var slot = CreateSlot(_availableCharactersContainer);
                SubscribeSlot(slot);
                _availableCharacterSlots.Add(slot);
            }


            // Update slot contents
            for (int i = 0; i < _availableCharacterSlots.Count; i++)
            {
                if (i < availableCharacters.Count)
                {
                    _availableCharacterSlots[i].SetCharacter(availableCharacters[i], CharacterSlotType.Available).Forget();
                }
                else
                {
                    _availableCharacterSlots[i].Clear();
                }
            }
        }

        private void OnCharacterSlotClicked(UI_CharacterSlot slot)
        {
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
            UpdatePreview(slot.Character);
        }
        private void OnSlotPointerExit(UI_CharacterSlot slot) => UpdatePreview(null);

        private void UpdatePreview(CharacterProfile character)
        {
            if (character == null || character.CharacterSettings == null)
            {
                _previewPanel?.SetActive(false);
                if (_characterNameText != null) _characterNameText.text = "";
                if (_characterStatsText != null) _characterStatsText.text = "";
                return;
            }

            _previewPanel?.SetActive(true);

            if (_characterNameText != null)
            {
                _characterNameText.text = character.CharacterSettings.name;
            }

            if (_characterStatsText != null)
            {
                if (character.CharacterSettings.characterStats != null)
                {
                    _characterStatsText.text = $"HP: {character.CharacterSettings.characterStats.health}\n" +
                                              $"Attack: {character.CharacterSettings.characterStats.damage}";
                }
                else
                {
                    _characterStatsText.text = "HP: Unknown\nAttack: Unknown";
                }
            }
        }

        /// <summary>
        /// Check if any characters are selected.
        /// </summary>
        public bool HasSelectedCharacters()
        {
            return _selectedCharacterSlots.Any(s => s.Character != null);
        }

        private void OnDestroy()
        {
            if (_charactersManager != null)
                _charactersManager.OnDataChanged -= Refresh;

            ClearSlots(_availableCharacterSlots);
            ClearSlots(_selectedCharacterSlots);
        }
    }
}