using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Angry_Girls
{
    /// <summary>
    /// Manages tutorial slide sequence and automatic tutorial presentation
    /// for the first mission of a new game.
    /// </summary>
    public class TutorialSystem : UI_GameplayManagersComponent
    {
        [Header("Tutorial UI")]
        [SerializeField] private GameObject _tutorialPanel;
        [SerializeField] private List<GameObject> _slides;

        [Header("Navigation")]
        [SerializeField] private Button _nextButton;
        [SerializeField] private Button _prevButton;
        [SerializeField] private Button _closeButton;

        [Header("First Game Tutorial")]
        [SerializeField] private float _firstGameTutorialWaitTimeout = 10f;

        private StageManager _stageManager;
        private GamePhaseFlowController _gamePhaseFlowController;

        private Coroutine _firstGameTutorialRoutine;

        private int _currentSlideIndex;
        private bool _autoTutorialShown;
        private bool _isInitialized;

        /// <summary>
        /// Initializes tutorial UI and starts first-game tutorial monitoring.
        /// </summary>
        public override void Initialize()
        {
            if (_isInitialized)
                return;

            base.Initialize();

            _isInitialized = true;

            _stageManager =
                GameplayCoreManager.Instance?.StageManager;

            _gamePhaseFlowController =
                GameplayCoreManager.Instance?.GamePhaseFlowController;

            if (_nextButton != null)
            {
                _nextButton.onClick.AddListener(
                    ShowNextSlide);
            }

            if (_prevButton != null)
            {
                _prevButton.onClick.AddListener(
                    ShowPreviousSlide);
            }

            if (_closeButton != null)
            {
                _closeButton.onClick.AddListener(
                    HideShowPanel);
            }

            HideTutorialPanel();

            if (_stageManager != null)
            {
                _stageManager.TheStageIsSet +=
                    HandleStageSet;
            }

            if (ShouldShowFirstGameTutorial())
            {
                _firstGameTutorialRoutine =
                    StartCoroutine(
                        WaitForFirstGameTutorial());
            }
        }

        /// <summary>
        /// Toggles the tutorial panel manually.
        /// </summary>
        public void HideShowPanel()
        {
            if (_tutorialPanel == null)
                return;

            if (_tutorialPanel.activeSelf)
                HideTutorialPanel();
            else
                Show();
        }

        /// <summary>
        /// Shows the tutorial starting from the first slide.
        /// </summary>
        public override void Show()
        {
            base.Show();

            if (_tutorialPanel == null)
                return;

            _currentSlideIndex = 0;

            _tutorialPanel.SetActive(true);

            UpdateSlideVisibility();
        }

        /// <summary>
        /// Hides the tutorial panel.
        /// </summary>
        public override void Hide()
        {
            HideTutorialPanel();
        }

        private void HideTutorialPanel()
        {
            if (_tutorialPanel != null)
                _tutorialPanel.SetActive(false);
        }

        private void HandleStageSet(
            int stageIndex)
        {
            if (_autoTutorialShown ||
                stageIndex != 0 ||
                !ShouldShowFirstGameTutorial())
            {
                return;
            }

            TryShowFirstGameTutorial();
        }

        private IEnumerator WaitForFirstGameTutorial()
        {
            var elapsedTime = 0f;

            while (
                !_autoTutorialShown &&
                elapsedTime < _firstGameTutorialWaitTimeout)
            {
                if (TryShowFirstGameTutorial())
                    yield break;

                elapsedTime += Time.unscaledDeltaTime;

                yield return null;
            }

            _firstGameTutorialRoutine = null;
        }

        private bool TryShowFirstGameTutorial()
        {
            if (_autoTutorialShown ||
                !ShouldShowFirstGameTutorial())
            {
                return false;
            }

            if (_stageManager == null)
            {
                _stageManager =
                    GameplayCoreManager.Instance?.StageManager;
            }

            if (_gamePhaseFlowController == null)
            {
                _gamePhaseFlowController =
                    GameplayCoreManager.Instance?.GamePhaseFlowController;
            }

            if (_stageManager == null ||
                _gamePhaseFlowController == null)
            {
                return false;
            }

            if (_stageManager.CurrentStageIndex != 0)
                return false;

            if (_stageManager.CurrentCharacterLauncher == null)
                return false;

            if (_gamePhaseFlowController.CurrentGamePhaseState !=
                GamePhaseNames.LaunchPhase)
            {
                return false;
            }

            _autoTutorialShown = true;

            GameStateManager.Instance
                ?.ConsumeNewGameTutorial();

            Show();

            return true;
        }

        private bool ShouldShowFirstGameTutorial()
        {
            return GameStateManager.Instance != null &&
                   GameStateManager.Instance.IsNewGameSession;
        }

        private void UpdateSlideVisibility()
        {
            if (_slides == null)
                return;

            for (var i = 0; i < _slides.Count; i++)
            {
                if (_slides[i] != null)
                {
                    _slides[i].SetActive(
                        i == _currentSlideIndex);
                }
            }

            if (_prevButton != null)
            {
                _prevButton.interactable =
                    _currentSlideIndex > 0;
            }

            if (_nextButton != null)
            {
                _nextButton.interactable =
                    _currentSlideIndex <
                    _slides.Count - 1;
            }
        }

        private void ShowNextSlide()
        {
            if (_slides == null ||
                _slides.Count == 0)
            {
                return;
            }

            if (_currentSlideIndex >=
                _slides.Count - 1)
            {
                return;
            }

            _currentSlideIndex++;

            UpdateSlideVisibility();
        }

        private void ShowPreviousSlide()
        {
            if (_currentSlideIndex <= 0)
                return;

            _currentSlideIndex--;

            UpdateSlideVisibility();
        }

        private void OnDestroy()
        {
            if (_firstGameTutorialRoutine != null)
            {
                StopCoroutine(
                    _firstGameTutorialRoutine);

                _firstGameTutorialRoutine = null;
            }

            if (_nextButton != null)
            {
                _nextButton.onClick.RemoveListener(
                    ShowNextSlide);
            }

            if (_prevButton != null)
            {
                _prevButton.onClick.RemoveListener(
                    ShowPreviousSlide);
            }

            if (_closeButton != null)
            {
                _closeButton.onClick.RemoveListener(
                    HideShowPanel);
            }

            if (_stageManager != null)
            {
                _stageManager.TheStageIsSet -=
                    HandleStageSet;
            }
        }
    }
}