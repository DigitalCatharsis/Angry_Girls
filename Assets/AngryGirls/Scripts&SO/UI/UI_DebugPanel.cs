using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Angry_Girls
{
    /// <summary>
    /// Debug panel for development diagnostics. Accessible via configurable hotkey.
    /// All UI created dynamically with inspector-configurable parameters.
    /// </summary>
    public class DebugPanel : MonoBehaviour
    {
        public static DebugPanel Instance { get; private set; }

        // === Inspector-configurable UI parameters ===
        [Header("Panel Appearance")]
        [SerializeField] private Vector2 _panelSize = new Vector2(420f, 600f);
        [SerializeField] private Vector2 _panelPosition = new Vector2(10f, -10f); // Offset from top-left corner
        [SerializeField] private Color _panelBackgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.85f);
        [SerializeField] private Color _buttonColor = new Color(0.25f, 0.25f, 0.25f, 1f);
        [SerializeField] private Color _buttonHighlightColor = new Color(0.4f, 0.4f, 0.6f, 1f);

        [Header("Button Layout")]
        [SerializeField] private Vector2 _buttonSize = new Vector2(180f, 30f);
        [SerializeField] private Vector2 _buttonSpacing = new Vector2(10f, 5f);
        [SerializeField] private int _buttonColumns = 2;

        [Header("Character Popup")]
        [SerializeField] private Vector2 _popupSize = new Vector2(350f, 480f);
        [SerializeField] private Color _popupBackgroundColor = new Color(0.15f, 0.15f, 0.15f, 0.95f);
        [SerializeField] private float _characterButtonHeight = 25f;
        [SerializeField] private float _characterButtonSpacing = 6f;
        [SerializeField] private Color _playerCharacterColor = new Color(0.2f, 0.5f, 0.2f, 1f);
        [SerializeField] private Color _enemyCharacterColor = new Color(0.5f, 0.2f, 0.2f, 1f);

        [Header("Behavior")]
        [SerializeField] private KeyCode _toggleKey = KeyCode.F1;
        [SerializeField] private float _updateInterval = 0.5f;
        [SerializeField] private Vector2 _referenceResolution = new Vector2(1280f, 720f);

        // === Runtime fields ===
        private GameObject _rootPanel;
        private TextMeshProUGUI _infoBox;
        private GameObject _buttonsContainer;
        private bool _isVisible = false;
        private Canvas _canvas;

        // Popup for character selection
        private GameObject _popupPanel;
        private Canvas _popupCanvas;
        private List<(CharacterType type, string name)> _characterTypesForPopup;

        private float _lastUpdate = 0f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void Update()
        {
            if (Input.GetKeyDown(_toggleKey))
            {
                TogglePanel();
            }

            if (_isVisible && Time.time - _lastUpdate > _updateInterval)
            {
                UpdateDebugInfo();
                _lastUpdate = Time.time;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (_isVisible)
            {
                UpdateDebugInfo();
            }
        }

        private void TogglePanel()
        {
            _isVisible = !_isVisible;

            if (_isVisible)
            {
                // Recreate the panel EVERY time it is opened to apply changes from the inspector
                if (_rootPanel != null)
                {
                    Destroy(_rootPanel);
                    _rootPanel = null;
                }
                CreateUI();
                UpdateDebugInfo();
            }
            else
            {
                if (_rootPanel != null)
                    _rootPanel.SetActive(false);

                // Close the popup when the main panel is closed
                if (_popupPanel != null)
                {
                    Destroy(_popupCanvas.gameObject);
                    _popupCanvas = null;
                    _popupPanel = null;
                    _characterTypesForPopup = null;
                }
            }
        }

        private void CreateUI()
        {
            _canvas = FindObjectOfType<Canvas>();
            if (_canvas == null)
            {
                var canvasObj = new GameObject("DebugPanel_Canvas");
                _canvas = canvasObj.AddComponent<Canvas>();
                _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                _canvas.sortingOrder = 10000;
                canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                canvasObj.AddComponent<GraphicRaycaster>();
                DontDestroyOnLoad(canvasObj);
            }

            _rootPanel = new GameObject("DebugPanel_Root");
            var rootRect = _rootPanel.AddComponent<RectTransform>();
            rootRect.SetParent(_canvas.transform, false);
            rootRect.anchorMin = new Vector2(0f, 1f);
            rootRect.anchorMax = new Vector2(0f, 1f);
            rootRect.pivot = new Vector2(0f, 1f);
            rootRect.anchoredPosition = _panelPosition;
            rootRect.sizeDelta = _panelSize;

            var panel = _rootPanel.AddComponent<Image>();
            panel.color = _panelBackgroundColor;
            panel.raycastTarget = false;
            _rootPanel.AddComponent<VerticalLayoutGroup>().padding = new RectOffset(10, 10, 10, 10);
            _rootPanel.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var infoObj = new GameObject("DebugPanel_InfoBox");
            var infoRect = infoObj.AddComponent<RectTransform>();
            infoObj.transform.SetParent(_rootPanel.transform, false);
            _infoBox = infoObj.AddComponent<TextMeshProUGUI>();
            _infoBox.fontSize = 14;
            _infoBox.color = Color.white;
            _infoBox.enableWordWrapping = false;
            _infoBox.raycastTarget = false;
            infoRect.sizeDelta = new Vector2(_panelSize.x - 20f, 250f);

            _buttonsContainer = new GameObject("DebugPanel_Buttons");
            var containerRect = _buttonsContainer.AddComponent<RectTransform>();
            containerRect.SetParent(_rootPanel.transform, false);
            containerRect.sizeDelta = new Vector2(_panelSize.x - 20f, _panelSize.y - 280f);
            containerRect.anchorMin = Vector2.zero;
            containerRect.anchorMax = Vector2.one;

            var gridLayout = _buttonsContainer.AddComponent<GridLayoutGroup>();
            gridLayout.cellSize = _buttonSize;
            gridLayout.spacing = _buttonSpacing;
            gridLayout.startCorner = GridLayoutGroup.Corner.UpperLeft;
            gridLayout.startAxis = GridLayoutGroup.Axis.Horizontal;
            gridLayout.childAlignment = TextAnchor.UpperLeft;
            gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            gridLayout.constraintCount = _buttonColumns;

            CreateButtons();
            _rootPanel.SetActive(true);
        }

        private void CreateButtons()
        {
            CreateButton("Save Game", SaveCurrentData);
            CreateButton("Load Save", () => LoadSaveData().Forget());
            CreateButton("Reset Save", () => ResetSaveData().Forget());
            CreateButton("Add 100", () => AddCredits(100));
            CreateButton("Remove 100", () => RemoveCredits(100));
            CreateButton("Add 1000", () => AddCredits(1000));
            CreateButton("Remove 1000", () => RemoveCredits(1000));
            CreateButton("Add Char", ShowCharacterSelectionPopup);
            CreateButton("Unlock Shops", UnlockAllShopCollections);
            CreateButton("Refresh Shop", () => RefreshShopFree().Forget());
            CreateButton("Complete Mission", CompleteCurrentMission);
            CreateButton("Close", TogglePanel);
        }

        private void CreateButton(string text, System.Action onClick)
        {
            var btnObj = new GameObject($"Btn_{text.Replace(" ", "_")}");
            var rect = btnObj.AddComponent<RectTransform>();
            btnObj.transform.SetParent(_buttonsContainer.transform, false);

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.sizeDelta = Vector2.zero;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            var btn = btnObj.AddComponent<Button>();
            btn.targetGraphic = btnObj.AddComponent<Image>();
            btn.targetGraphic.color = _buttonColor;

            var txtObj = new GameObject("Text");
            txtObj.transform.SetParent(btnObj.transform, false);
            var txtRect = txtObj.AddComponent<RectTransform>();
            txtRect.anchorMin = Vector2.zero;
            txtRect.anchorMax = Vector2.one;
            txtRect.offsetMin = new Vector2(5f, 0f);
            txtRect.offsetMax = new Vector2(-5f, 0f);

            var txt = txtObj.AddComponent<TextMeshProUGUI>();
            txt.text = text;
            txt.fontSize = 14;
            txt.color = Color.white;
            txt.alignment = TextAlignmentOptions.Center;
            txt.enableWordWrapping = false;

            btn.onClick.AddListener(() =>
            {
                btn.targetGraphic.color = _buttonHighlightColor;
                onClick?.Invoke();
                btn.targetGraphic.color = _buttonColor;
            });
        }

        private void UpdateDebugInfo()
        {
            if (_infoBox == null) return;

            var sb = new StringBuilder();
            sb.AppendLine("<b>=== DEBUG PANEL ===</b>\n");

            sb.AppendLine("<b>RUNTIME</b>");
            AppendRuntimeState(sb);
            sb.AppendLine("\n<b>---</b>\n");

            sb.AppendLine("<b>SAVED</b>");
            AppendSavedState(sb);
            sb.AppendLine($"\n<b>Scene:</b> {SceneManager.GetActiveScene().name}");

            _infoBox.text = sb.ToString();
        }

        private void AppendRuntimeState(StringBuilder sb)
        {
            var moneyStorage = CoreManager.Instance?.CreditsManager;
            int money = moneyStorage?.GetCredits() ?? 0;
            sb.AppendLine($"Credits: {money}");

            var charactersManager = CoreManager.Instance?.CharactersManager;
            var data = charactersManager?.CharactersData;
            if (data == null)
            {
                sb.AppendLine("\nCharacters: <color=yellow>Not initialized</color>");
                return;
            }

            int selectedCount = data.SelectedCharactersPool.Count(c => c != null);
            sb.AppendLine($"\nSelected ({selectedCount}/6):");
            for (int i = 0; i < data.SelectedCharactersPool.Count; i++)
            {
                var p = data.SelectedCharactersPool[i];
                sb.AppendLine($"  [{i}] {(p?.CharacterSettings?.name ?? "<empty>")}");
            }

            sb.AppendLine($"\nAvailable: {data.AvailableCharacterPool.Count}");
            for (int i = 0; i < Mathf.Min(4, data.AvailableCharacterPool.Count); i++)
            {
                var p = data.AvailableCharacterPool[i];
                sb.AppendLine($"  [{i}] {p?.CharacterSettings?.name ?? "NULL"}");
            }
            if (data.AvailableCharacterPool.Count > 4)
                sb.AppendLine($"  ... +{data.AvailableCharacterPool.Count - 4} more");
        }

        private void AppendSavedState(StringBuilder sb)
        {
            bool hasSave = Repository.LoadState();
            if (!hasSave)
            {
                sb.AppendLine("No save file");
                return;
            }

            var data = Repository.GetData<CreditsData>();
            if (data != null)
            {
                sb.AppendLine($"Credits: {data.Credits}");
            }

            try
            {
                var savedCharacters = Repository.GetData<CharactersSaveData>();
                if (savedCharacters == null)
                {
                    sb.AppendLine("\nCharacters: <color=yellow>No data saved</color>");
                    return;
                }

                int selectedCount = 0;
                if (savedCharacters.selectedCharactersSlotsSaveData != null)
                {
                    foreach (var dto in savedCharacters.selectedCharactersSlotsSaveData)
                    {
                        if (dto != null && dto.characterType != CharacterType.NULL)
                            selectedCount++;
                    }
                }

                int availableCount = 0;
                if (savedCharacters.availableCharacterPoolSaveData != null)
                {
                    foreach (var dto in savedCharacters.availableCharacterPoolSaveData)
                    {
                        if (dto != null && dto.characterType != CharacterType.NULL)
                            availableCount++;
                    }
                }

                sb.AppendLine($"\nSelected: {selectedCount}/6");
                sb.AppendLine($"Available: {availableCount}");
            }
            catch (System.Exception ex)
            {
                sb.AppendLine($"<color=red>Chars load error</color>");
                Debug.LogError($"DebugPanel saved chars error: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void ShowCharacterSelectionPopup()
        {
            if (_popupPanel != null)
            {
                Destroy(_popupCanvas.gameObject);
                _popupCanvas = null;
                _popupPanel = null;
                _characterTypesForPopup = null;
            }

            var catalog = CoreManager.Instance?.CharacterSettingsCatalogSO;
            if (catalog == null)
            {
                UIManager.Instance?.ShowNotification("CharacterSettingsCatalog not ready", 0.5f);
                return;
            }

            _characterTypesForPopup = catalog.GetAllTypesForDebug().ToList();
            if (_characterTypesForPopup.Count == 0)
            {
                UIManager.Instance?.ShowNotification("No character types available", 0.5f);
                return;
            }

            // Создаём ОТДЕЛЬНЫЙ канвас для попапа
            var popupCanvasObj = new GameObject("CharacterSelectionPopup_Canvas");
            _popupCanvas = popupCanvasObj.AddComponent<Canvas>();
            _popupCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _popupCanvas.sortingOrder = 11000;
            var scaler = popupCanvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = _referenceResolution;
            scaler.matchWidthOrHeight = 0.5f;
            popupCanvasObj.AddComponent<GraphicRaycaster>();
            DontDestroyOnLoad(popupCanvasObj);

            // Попап панель
            _popupPanel = new GameObject("CharacterSelectionPopup");
            var popupRect = _popupPanel.AddComponent<RectTransform>();
            popupRect.SetParent(_popupCanvas.transform, false);
            popupRect.anchorMin = new Vector2(0.5f, 0.5f);
            popupRect.anchorMax = new Vector2(0.5f, 0.5f);
            popupRect.pivot = new Vector2(0.5f, 0.5f);
            popupRect.sizeDelta = _popupSize;
            popupRect.anchoredPosition = Vector2.zero;

            // Фон попапа
            var bg = _popupPanel.AddComponent<Image>();
            bg.color = _popupBackgroundColor;
            bg.raycastTarget = true;

            // Заголовок
            var titleObj = new GameObject("Title");
            titleObj.transform.SetParent(_popupPanel.transform, false);
            var titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0f, 1f);
            titleRect.anchorMax = new Vector2(1f, 1f);
            titleRect.pivot = new Vector2(0.5f, 1f);
            titleRect.sizeDelta = new Vector2(0f, 40f);
            titleRect.anchoredPosition = new Vector2(0f, -10f);

            var titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "Select Character to Spawn";
            titleText.fontSize = 18;
            titleText.color = Color.white;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.fontStyle = FontStyles.Bold;

            // Scroll view
            var scrollObj = new GameObject("Scroll View");
            scrollObj.transform.SetParent(_popupPanel.transform, false);
            var scrollRect = scrollObj.AddComponent<RectTransform>();
            scrollRect.anchorMin = new Vector2(0f, 0f);
            scrollRect.anchorMax = new Vector2(1f, 1f);
            scrollRect.offsetMin = new Vector2(10f, 50f);
            scrollRect.offsetMax = new Vector2(-30f, -70f);

            var scroll = scrollObj.AddComponent<ScrollRect>();
            scroll.scrollSensitivity = 30f;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.vertical = true;
            scroll.horizontal = false;
            scroll.inertia = true;
            scroll.decelerationRate = 0.135f;

            // Viewport
            var viewport = new GameObject("Viewport").AddComponent<RectTransform>();
            viewport.SetParent(scrollObj.transform, false);
            viewport.anchorMin = Vector2.zero;
            viewport.anchorMax = Vector2.one;
            viewport.sizeDelta = Vector2.zero;
            viewport.gameObject.AddComponent<Mask>();

            // Scrollbar
            var scrollbarObj = new GameObject("Scrollbar");
            scrollbarObj.transform.SetParent(_popupPanel.transform, false);
            var scrollbarRect = scrollbarObj.AddComponent<RectTransform>();
            scrollbarRect.anchorMin = new Vector2(1f, 0f);
            scrollbarRect.anchorMax = new Vector2(1f, 1f);
            scrollbarRect.pivot = new Vector2(1f, 1f);
            scrollbarRect.sizeDelta = new Vector2(20f, -120f);
            scrollbarRect.anchoredPosition = new Vector2(-10f, -10f);

            var scrollbar = scrollbarObj.AddComponent<Scrollbar>();
            scrollbar.direction = Scrollbar.Direction.BottomToTop;
            scrollbar.value = 1f;

            var trackObj = new GameObject("Track");
            trackObj.transform.SetParent(scrollbarObj.transform, false);
            var trackRect = trackObj.AddComponent<RectTransform>();
            trackRect.anchorMin = Vector2.zero;
            trackRect.anchorMax = Vector2.one;
            trackRect.sizeDelta = Vector2.zero;
            var trackImage = trackObj.AddComponent<Image>();
            trackImage.color = new Color(0.3f, 0.3f, 0.3f, 1f);

            var handleObj = new GameObject("Handle");
            handleObj.transform.SetParent(trackObj.transform, false);
            var handleRect = handleObj.AddComponent<RectTransform>();
            handleRect.anchorMin = new Vector2(0f, 0f);
            handleRect.anchorMax = new Vector2(1f, 0.2f);
            handleRect.sizeDelta = Vector2.zero;
            var handleImage = handleObj.AddComponent<Image>();
            handleImage.color = new Color(0.6f, 0.6f, 0.6f, 1f);

            scrollbar.handleRect = handleRect;
            scroll.verticalScrollbar = scrollbar;

            // Content
            var content = new GameObject("Content");
            content.transform.SetParent(viewport, false);
            var contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = Vector2.zero;
            contentRect.anchorMax = Vector2.one;
            contentRect.sizeDelta = new Vector2(0f, _characterTypesForPopup.Count * (_characterButtonHeight + _characterButtonSpacing) + 20f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.anchoredPosition = new Vector2(0f, 0f);

            var layout = content.AddComponent<VerticalLayoutGroup>();
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.spacing = _characterButtonSpacing;
            layout.padding = new RectOffset(5, 5, 5, 5);
            content.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scroll.viewport = viewport;
            scroll.content = contentRect;

            // Character buttons
            for (int i = 0; i < _characterTypesForPopup.Count; i++)
            {
                var (charType, name) = _characterTypesForPopup[i];
                var isPlayer = charType.ToString().StartsWith("Player_");
                var bgColor = isPlayer ? _playerCharacterColor : _enemyCharacterColor;
                var textColor = isPlayer ? new Color(0.9f, 1f, 0.9f) : new Color(1f, 0.9f, 0.9f);

                var btnObj = new GameObject($"CharBtn_{i}");
                btnObj.transform.SetParent(content.transform, false);
                var btnRect = btnObj.AddComponent<RectTransform>();
                btnRect.sizeDelta = new Vector2(_popupSize.x - 60f, _characterButtonHeight);

                var layoutElement = btnObj.AddComponent<LayoutElement>();
                layoutElement.minHeight = _characterButtonHeight;
                layoutElement.preferredHeight = _characterButtonHeight;

                var btn = btnObj.AddComponent<Button>();
                var btnBg = btnObj.AddComponent<Image>();
                btnBg.color = bgColor;
                btnBg.raycastTarget = true;

                var btnText = new GameObject("Text").AddComponent<TextMeshProUGUI>();
                btnText.transform.SetParent(btnObj.transform, false);
                btnText.text = name;
                btnText.fontSize = 14f;
                btnText.color = textColor;
                btnText.alignment = TextAlignmentOptions.Left;
                btnText.enableWordWrapping = false;
                var textRect = btnText.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = new Vector2(10f, 0f);
                textRect.offsetMax = new Vector2(-10f, 0f);

                int index = i;
                btn.onClick.AddListener(() =>
                {
                    if (_characterTypesForPopup == null || index >= _characterTypesForPopup.Count)
                        return;

                    SpawnCharacterByType(index);
                    Destroy(_popupCanvas.gameObject);
                    _popupCanvas = null;
                    _popupPanel = null;
                    _characterTypesForPopup = null;
                });
            }

            // Cancel button
            var cancelObj = new GameObject("CancelButton");
            cancelObj.transform.SetParent(_popupPanel.transform, false);
            var cancelRect = cancelObj.AddComponent<RectTransform>();
            cancelRect.anchorMin = new Vector2(0.5f, 0f);
            cancelRect.anchorMax = new Vector2(0.5f, 0f);
            cancelRect.pivot = new Vector2(0.5f, 0.5f);
            cancelRect.sizeDelta = new Vector2(120f, 35f);
            cancelRect.anchoredPosition = new Vector2(0f, 20f);

            var cancelBtn = cancelObj.AddComponent<Button>();
            var cancelBg = cancelObj.AddComponent<Image>();
            cancelBg.color = new Color(0.4f, 0.4f, 0.4f, 1f);

            var cancelText = new GameObject("Text").AddComponent<TextMeshProUGUI>();
            cancelText.transform.SetParent(cancelObj.transform, false);
            cancelText.text = "Cancel";
            cancelText.fontSize = 16;
            cancelText.color = Color.white;
            cancelText.alignment = TextAlignmentOptions.Center;
            var cancelTextRect = cancelText.GetComponent<RectTransform>();
            cancelTextRect.anchorMin = Vector2.zero;
            cancelTextRect.anchorMax = Vector2.one;
            cancelTextRect.offsetMin = Vector2.zero;
            cancelTextRect.offsetMax = Vector2.zero;

            cancelBtn.onClick.AddListener(() =>
            {
                Destroy(_popupCanvas.gameObject);
                _popupCanvas = null;
                _popupPanel = null;
                _characterTypesForPopup = null;
            });
        }

        public void SaveCurrentData()
        {
            if (GameStateManager.Instance != null)
            {
                CoreManager.Instance.SaveLoadManager.SaveGame();
                UIManager.Instance?.ShowNotification("Game saved!", 0.5f);
                UpdateDebugInfo();
            }
            else
            {
                UIManager.Instance?.ShowNotification("GameStateManager null", 0.5f);
            }
        }

        public async UniTask LoadSaveData()
        {
            if (CoreManager.Instance != null && CoreManager.Instance.SaveLoadManager != null)
            {
                await CoreManager.Instance.SaveLoadManager.LoadGameAsync();
                UIManager.Instance?.ShowNotification("Save loaded!", 0.5f);
                UpdateDebugInfo();
            }
            else
            {
                UIManager.Instance?.ShowNotification("SaveLoadManager unavailable", 0.5f);
            }
        }

        public async UniTask ResetSaveData()
        {
            Repository.ResetState();
            Debug.Log("DebugPanel: Save reset via Repository.ResetState()");

            if (CoreManager.Instance != null && CoreManager.Instance.SaveLoadManager != null)
                await CoreManager.Instance.SaveLoadManager.LoadGameAsync();

            var mainMenu = UIManager.Instance?.GetScreen<UI_MainMenuScreen>();
            mainMenu?.UpdateContinueButtonState();

            UIManager.Instance?.ShowNotification("Save reset!", 0.5f);
            UpdateDebugInfo();
        }

        public void AddCredits(int amount)
        {
            var moneyStorage = CoreManager.Instance?.CreditsManager;
            if (moneyStorage == null)
            {
                UIManager.Instance?.ShowNotification("MoneyStorage unavailable", 0.5f);
                return;
            }

            moneyStorage.SetCredits(amount);
            UIManager.Instance?.ShowNotification($"Added {amount} credits", 0.5f);
            UpdateDebugInfo();

            var shopPanel = FindObjectOfType<UI_ShopPanel>();
            if (shopPanel != null && shopPanel.gameObject.activeInHierarchy)
            {
                shopPanel.Refresh();
            }
        }

        public void RemoveCredits(int amount)
        {
            var moneyStorage = CoreManager.Instance?.CreditsManager;
            if (moneyStorage == null)
            {
                UIManager.Instance?.ShowNotification("MoneyStorage unavailable", 0.5f);
                return;
            }

            int current = moneyStorage.GetCredits();
            int remove = Mathf.Min(amount, current);
            moneyStorage.SetCredits(-remove);
            UIManager.Instance?.ShowNotification($"Removed {remove} credits", 0.5f);
            UpdateDebugInfo();

            var shopPanel = FindObjectOfType<UI_ShopPanel>();
            if (shopPanel != null && shopPanel.gameObject.activeInHierarchy)
            {
                shopPanel.Refresh();
            }
        }

        public void SpawnCharacterByType(int typeIndex)
        {
            var catalog = CoreManager.Instance?.CharacterSettingsCatalogSO;
            var charactersManager = CoreManager.Instance?.CharactersManager;

            if (catalog == null || charactersManager == null || charactersManager.CharactersData == null)
            {
                UIManager.Instance?.ShowNotification("Catalog/Manager not ready", 0.5f);
                return;
            }

            if (typeIndex < 0 || typeIndex >= _characterTypesForPopup?.Count)
            {
                UIManager.Instance?.ShowNotification("Invalid type index", 0.5f);
                return;
            }

            var charType = _characterTypesForPopup[typeIndex].type;
            var settings = catalog.GetByType(charType);

            if (settings == null)
            {
                UIManager.Instance?.ShowNotification($"No settings for type {charType}", 0.5f);
                return;
            }

            var newProfile = new CharacterProfile();
            newProfile.UpdateCharactersSettingsManually(settings);
            charactersManager.CharactersData.InternalAddToAvailable(newProfile);

            UpdateDebugInfo();

            var charPanel = FindObjectOfType<UI_CharacterSelectionPanel>();
            if (charPanel != null && charPanel.gameObject.activeInHierarchy)
            {
                charPanel.Refresh();
            }

            UIManager.Instance?.ShowNotification($"Spawned: {settings.name}", 0.5f);
        }

        public void UnlockAllShopCollections()
        {
            var shopManager = CoreManager.Instance?.ShopManager;
            if (shopManager == null)
            {
                UIManager.Instance?.ShowNotification("ShopManager unavailable", 0.5f);
                return;
            }

            var field = shopManager.GetType().GetField("_collectionAvailability",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (field?.GetValue(shopManager) is Dictionary<ShopAvailability, bool> dict)
            {
                foreach (var key in new[] { ShopAvailability.Easy, ShopAvailability.Normal, ShopAvailability.Hard })
                    if (dict.ContainsKey(key)) dict[key] = true;

                UIManager.Instance?.ShowNotification("All shops unlocked!", 0.5f);
                UpdateDebugInfo();

                var shopPanel = FindObjectOfType<UI_ShopPanel>();
                if (shopPanel != null && shopPanel.gameObject.activeInHierarchy)
                {
                    shopPanel.Refresh();
                }
            }
            else
            {
                UIManager.Instance?.ShowNotification("Collection field not found", 0.5f);
            }
        }

        public async UniTask RefreshShopFree()
        {
            var shopManager = CoreManager.Instance?.ShopManager;
            if (shopManager == null)
            {
                UIManager.Instance?.ShowNotification("ShopManager unavailable", 0.5f);
                return;
            }

            var tempListField = shopManager.GetType().GetField("_temporarilyRemovedFromAssortmentSinceLastRefresh",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            (tempListField?.GetValue(shopManager) as HashSet<string>)?.Clear();

            await shopManager.RefreshAssortmentAsync();

            var shopPanel = FindObjectOfType<UI_ShopPanel>();
            if (shopPanel != null && shopPanel.gameObject.activeInHierarchy)
            {
                shopPanel.Refresh();
            }

            UIManager.Instance?.ShowNotification("Shop refreshed (free)!", 0.5f);
            UpdateDebugInfo();
        }

        public void CompleteCurrentMission()
        {
            var stageMgr = GameplayCoreManager.Instance?.StageManager;
            var missionsManager = CoreManager.Instance?.MissionsManager;

            if (stageMgr == null || missionsManager == null)
            {
                UIManager.Instance?.ShowNotification("Stage/Mission manager null", 0.5f);
                return;
            }

            int stageIndex = stageMgr.CurrentStageIndex;
            if (stageIndex < missionsManager.GetMissionCount())
            {
                missionsManager.CompleteMission(stageIndex, MissionDifficulty.Easy);
                UIManager.Instance?.ShowNotification($"Mission {stageIndex} completed!", 0.5f);
                UpdateDebugInfo();
            }
            else
            {
                UIManager.Instance?.ShowNotification("No mission to complete", 0.5f);
            }
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (_popupCanvas != null)
                Destroy(_popupCanvas.gameObject);
        }
    }
}