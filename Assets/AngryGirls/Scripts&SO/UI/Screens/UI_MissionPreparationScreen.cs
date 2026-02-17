using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using System.Collections.Generic;

namespace Angry_Girls
{
    /// <summary>
    /// Main mission preparation screen with tab navigation.
    /// </summary>
    public class UI_MissionPreparationScreen : UI_UIScreen
    {
        [Header("Navigation Tabs")]
        [SerializeField] private Button _charactersTabButton;
        [SerializeField] private Button _missionsTabButton;
        [SerializeField] private Button _shopTabButton;
        [SerializeField] private Button _editTeamButton;

        [Header("Tab Panels")]
        [SerializeField] private GameObject _charactersPanelGameObject;
        [SerializeField] private GameObject _missionsPanelGameObject;
        [SerializeField] private GameObject _shopPanelGameObject;
        [SerializeField] private GameObject _editTeamPanelGameObject;

        [Header("Bottom Buttons")]
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _startMissionButton;

        [Header("Character Selection Components")]
        [SerializeField] private UI_CharacterSelectionPanel _characterSelectionPanel;

        [Header("Mission Selection Components")]
        [SerializeField] private UI_MissionSelectionPanel _missionSelectionPanel;

        [Header("Shop and Inventory Components")]
        [SerializeField] private UI_ShopPanel _shopPanelComponent;

        [Header("Team Equipment")]
        [SerializeField] private UI_TeamEditPanel _editTeamPanel;

        private GameObject _activePanel;
        private bool _isInitialized = false;

        [Header("Button Colors")]
        [SerializeField] private Color _selectedButtonColor = Color.yellow;
        [SerializeField] private Color _defaultButtonColor = Color.white;

        private List<IUIPanel> _UIPanels = new();
        private List<Button> __panelButtons = new();

        public override void Initialize()
        {
            if (_isInitialized) return;

            _UIPanels.Add(_characterSelectionPanel);
            _UIPanels.Add(_missionSelectionPanel);
            _UIPanels.Add(_shopPanelComponent);
            _UIPanels.Add(_editTeamPanel);

            __panelButtons.Add(_charactersTabButton);
            __panelButtons.Add(_shopTabButton);
            __panelButtons.Add(_editTeamButton);
            __panelButtons.Add(_missionsTabButton);

            base.Initialize();
            SetupUI();
            _isInitialized = true;
        }

        private void SetupUI()
        {
            //Init Buttons
            _charactersTabButton.onClick.AddListener(OnCharactersTabPressed);
            _missionsTabButton.onClick.AddListener(OnMissionsTabPressed);
            _shopTabButton.onClick.AddListener(OnShopTabPressed);
            _backButton.onClick.AddListener(OnBackPressed);
            _startMissionButton.onClick.AddListener(OnStartMissionPressed);
            _editTeamButton.onClick.AddListener(OnEditTeamTabPressed);



            //init values
            var assetProvider = CoreManager.Instance.AddressableAssetManager;
            var shopManager = CoreManager.Instance.ShopManager;
            var inventoryManager = CoreManager.Instance.InventoryManager;
            var moneyStorage = CoreManager.Instance.CreditsManager;

            //InitPanels
            foreach(var panel in _UIPanels)
            {
                panel.Initialize(CoreManager.Instance);
            }

            //TODO: show previous or default
            ShowCharactersTab();
        }


        public override void Show()
        {
            base.Show();
            RefreshAllPanels();
        }

        private void RefreshAllPanels()
        {
            foreach (var panel in _UIPanels)
            {
                panel.Refresh();
            }
        }

        private void ShowCharactersTab()
        {
            SwitchToPanel(_charactersPanelGameObject);
            HighlightActiveTab(_charactersTabButton);
        }

        private void ShowEditTeamTab()
        {
            SwitchToPanel(_editTeamPanelGameObject);
            HighlightActiveTab(_editTeamButton);
        }

        private void ShowMissionsTab()
        {
            SwitchToPanel(_missionsPanelGameObject);
            HighlightActiveTab(_missionsTabButton);
        }

        private void ShowShopTab()
        {
            SwitchToPanel(_shopPanelGameObject);
            HighlightActiveTab(_shopTabButton);
        }

        private void SwitchToPanel(GameObject panelToShow)
        {
            if (_activePanel != null)
                _activePanel.SetActive(false);

            if (panelToShow != null)
            {
                panelToShow.SetActive(true);
                _activePanel = panelToShow;
            }
        }

        private void HighlightActiveTab(Button activeButton)
        {
            ResetTabHighlights();

            if (activeButton != null)
            {
                var image = activeButton.GetComponent<Image>();
                if (image != null)
                {
                    image.color = _selectedButtonColor;
                }
            }
        }

        private void ResetTabHighlights()
        {
            foreach (var button in __panelButtons)
            {
                if (button != null)
                {
                    var image = button.GetComponent<Image>();
                    if (image != null)
                    {
                        image.color = _defaultButtonColor;
                    }
                }
            }
        }

        private void OnCharactersTabPressed()
        {
            ShowCharactersTab();
        }

        private void OnMissionsTabPressed()
        {
            ShowMissionsTab();
        }

        private void OnShopTabPressed()
        {
            ShowShopTab();
        }

        private void OnEditTeamTabPressed()
        {
            ShowEditTeamTab();
        }

        private async void OnBackPressed()
        {
            CoreManager.Instance.SaveLoadManager.SaveGame();
            await NavigationManager.NavigateToScene(SceneType.MainMenuScene);
        }

        private async void OnStartMissionPressed()
        {
            if (CanStartMission())
            {
                await StartSelectedMissionAsync();
            }
            else
            {
                UIManager.Instance.ShowNotification("Please select a mission and characters first!", 0.5f);
            }
        }

        private bool CanStartMission()
        {
            if (!_missionSelectionPanel.IsMissionSelected())
            {
                return false;
            }

            if (!_characterSelectionPanel.HasSelectedCharacters())
            {
                return false;
            }

            return true;
        }

        private async UniTask StartSelectedMissionAsync()
        {
            var selectedMission = _missionSelectionPanel.GetSelectedMission();
            if (_missionSelectionPanel.IsMissionSelected())
            {
                CoreManager.Instance.SaveLoadManager.SaveGame();
                NavigationManager.SetLastMission(selectedMission.missionName);
                await NavigationManager.NavigateToScene(selectedMission.missionName);
            }
        }

        private void OnDestroy()
        {
            if (_editTeamButton != null)
                _editTeamButton.onClick.RemoveListener(OnEditTeamTabPressed);

            if (_charactersTabButton != null)
                _charactersTabButton.onClick.RemoveListener(OnCharactersTabPressed);

            if (_missionsTabButton != null)
                _missionsTabButton.onClick.RemoveListener(OnMissionsTabPressed);

            if (_shopTabButton != null)
                _shopTabButton.onClick.RemoveListener(OnShopTabPressed);


            if (_backButton != null)
                _backButton.onClick.RemoveListener(OnBackPressed);

            if (_startMissionButton != null)
                _startMissionButton.onClick.RemoveListener(OnStartMissionPressed);
        }
    }
}