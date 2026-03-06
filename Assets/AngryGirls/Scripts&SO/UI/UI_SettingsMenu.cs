using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Angry_Girls
{
    /// <summary>
    /// Settings categories for tab navigation
    /// </summary>
    public enum SettingsCategory
    {
        Audio = 0,
        Camera = 1,
        Graphics = 2,
        Gameplay = 3,
        Controls = 4,
        System = 5
    }

    /// <summary>
    /// Main settings screen with tabbed navigation
    /// </summary>
    public class UI_SettingsScreen : UI_UIScreen
    {
        [Header("Tab Buttons")]
        [SerializeField] private Button[] _tabButtons;
        [SerializeField] private Color _selectedTabColor = new Color(1f, 0.84f, 0f, 1f);
        [SerializeField] private Color _defaultTabColor = Color.white;

        [Header("Category Panels")]
        [SerializeField] private GameObject[] _categoryPanels;

        [Header("Navigation")]
        [SerializeField] private Button _closeButton;
        [SerializeField] private Button _resetAllButton;
        [SerializeField] private Button _saveButton;

        private SettingsCategory _currentCategory = SettingsCategory.Audio;
        private SettingsManager _settingsManager;
        private Dictionary<SettingsCategory, ISettingsCategoryPanel> _categoryPanelControllers;

        public override void Initialize()
        {
            base.Initialize();
            _settingsManager = CoreManager.Instance.SettingsManager;
            InitializeCategoryPanels();
            SetupTabButtons();
            SetupNavigationButtons();
            ShowCategory(SettingsCategory.Audio);
        }

        private void InitializeCategoryPanels()
        {
            _categoryPanelControllers = new Dictionary<SettingsCategory, ISettingsCategoryPanel>();

            foreach (var panel in GetComponentsInChildren<ISettingsCategoryPanel>(true))
            {
                panel.Initialize(_settingsManager);
                _categoryPanelControllers[panel.Category] = panel;
            }
        }

        private void SetupTabButtons()
        {
            for (int i = 0; i < _tabButtons.Length; i++)
            {
                int index = i;
                _tabButtons[i].onClick.RemoveAllListeners();
                _tabButtons[i].onClick.AddListener(() => OnTabClicked((SettingsCategory)index));
            }
        }

        private void SetupNavigationButtons()
        {
            if (_closeButton != null)
                _closeButton.onClick.AddListener(OnClosePressed);

            if (_resetAllButton != null)
                _resetAllButton.onClick.AddListener(OnResetAllPressed);

            if (_saveButton != null)
                _saveButton.onClick.AddListener(OnSavePressed);
        }

        private void OnTabClicked(SettingsCategory category)
        {
            if (_currentCategory == category) return;
            ShowCategory(category);
        }

        private void ShowCategory(SettingsCategory category)
        {
            _currentCategory = category;
            UpdateTabVisuals();
            UpdatePanelVisibility();
            LoadCategoryValues();
        }

        private void UpdateTabVisuals()
        {
            for (int i = 0; i < _tabButtons.Length; i++)
            {
                var image = _tabButtons[i].GetComponent<Image>();
                if (image != null)
                {
                    image.color = (i == (int)_currentCategory) ? _selectedTabColor : _defaultTabColor;
                }
            }
        }

        private void UpdatePanelVisibility()
        {
            for (int i = 0; i < _categoryPanels.Length; i++)
            {
                if (_categoryPanels[i] != null)
                {
                    _categoryPanels[i].SetActive(i == (int)_currentCategory);
                }
            }
        }

        private void LoadCategoryValues()
        {
            if (_categoryPanelControllers.TryGetValue(_currentCategory, out var panel))
            {
                panel.LoadValues();
            }
        }

        private void OnClosePressed()
        {
            _settingsManager.SaveSettings();
            Hide();
        }

        private void OnResetAllPressed()
        {
            _settingsManager.ResetToPlatformDefaults();
            LoadCategoryValues();
            UIManager.Instance?.ShowNotification("All settings reset to defaults", 1f);
        }

        private void OnSavePressed()
        {
            _settingsManager.SaveSettings();
            UIManager.Instance?.ShowNotification("Settings saved", 0.5f);
        }

        public override void Show()
        {
            base.Show();
            LoadCategoryValues();
        }

        private void OnDestroy()
        {
            foreach (var button in _tabButtons)
            {
                if (button != null) button.onClick.RemoveAllListeners();
            }
            if (_closeButton != null) _closeButton.onClick.RemoveAllListeners();
            if (_resetAllButton != null) _resetAllButton.onClick.RemoveAllListeners();
            if (_saveButton != null) _saveButton.onClick.RemoveAllListeners();
        }
    }

    /// <summary>
    /// Interface for individual category panel controllers
    /// </summary>
    public interface ISettingsCategoryPanel
    {
        SettingsCategory Category { get; }
        void Initialize(SettingsManager settingsManager);
        void LoadValues();
        void SaveValues();
    }
}