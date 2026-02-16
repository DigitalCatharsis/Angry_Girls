using System.Collections.Generic;
using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Main UI manager that controls screen navigation and notifications.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        public static UIManager Instance { get; private set; }

        [Header("UI Screens")]
        [SerializeField] private UI_MainMenuScreen _mainMenuScreen;
        [SerializeField] private UI_MissionPreparationScreen _missionPreparationScreen;
        [SerializeField] private UI_GameplayScreen _gameplayScreen;

        [Header("Global UI")]
        [SerializeField] private UI_LoadingScreen _loadingScreen;
        [SerializeField] private NotificationManager _notificationManager;

        private UI_UIScreen _currentScreen;
        private Stack<UI_UIScreen> _screenHistory = new Stack<UI_UIScreen>();
        private bool _isInitialized = false;

        [SerializeField] private float _defaultNotificationDuration = 0.7f;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            InitializeScreens();
        }

        private void OnEnable()
        {
            if (!_isInitialized)
            {
                InitializeScreens();
            }
        }

        private void InitializeScreens()
        {
            if (_isInitialized) return;

            if (_mainMenuScreen != null)
            {
                _mainMenuScreen.Initialize();
            }

            if (_missionPreparationScreen != null)
            {
                _missionPreparationScreen.Initialize();
            }

            if (_gameplayScreen != null)
            {
                _gameplayScreen.Initialize();
            }

            _isInitialized = true;
        }

        /// <summary>
        /// Show a specific screen type.
        /// </summary>
        public void ShowScreen<T>(bool addToHistory = true) where T : UI_UIScreen
        {
            UI_UIScreen screenToShow = _mainMenuScreen is T ? _mainMenuScreen
                : _missionPreparationScreen is T ? _missionPreparationScreen
                : _gameplayScreen is T ? _gameplayScreen
                : null;

            if (screenToShow != null)
            {
                if (_currentScreen != null)
                {
                    if (addToHistory)
                        _screenHistory.Push(_currentScreen);
                    _currentScreen.Hide();
                }

                _currentScreen = screenToShow;
                _currentScreen.Show();
            }
            else
            {
                Debug.LogWarning($"Screen of type {typeof(T).Name} is not available in current scene");
            }
        }

        /// <summary>
        /// Go back to the previous screen.
        /// </summary>
        public void GoBack()
        {
            if (_screenHistory.Count > 0 && _currentScreen != null)
            {
                _currentScreen.Hide();
                _currentScreen = _screenHistory.Pop();
                _currentScreen.Show();
            }
        }

        /// <summary>
        /// Get a specific screen component.
        /// </summary>
        public T GetScreen<T>() where T : UI_UIScreen
        {
            if (_mainMenuScreen is T mainMenuScreen) return mainMenuScreen;
            if (_missionPreparationScreen is T missionPreparationScreen) return missionPreparationScreen;
            if (_gameplayScreen is T gameplayScreen) return gameplayScreen;
            return null;
        }

        /// <summary>
        /// Show or hide the loading screen.
        /// </summary>
        public void ShowLoadingScreen(bool show)
        {
            if (_loadingScreen != null)
            {
                if (show)
                    _loadingScreen.Show();
                else
                    _loadingScreen.Hide();
            }
        }

        /// <summary>
        /// Show a notification message.
        /// </summary>
        public void ShowNotification(string message, float duration = -1)
        {
            if (duration == -1)
            {
                duration = _defaultNotificationDuration;
            }

            _notificationManager?.ShowNotification(message, duration);
        }

        /// <summary>
        /// Update screen references when a new scene is loaded.
        /// </summary>
        public void UpdateScreenReferences()
        {
            _isInitialized = false;
            InitializeScreens();
        }
    }
}