using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Angry_Girls
{
    /// <summary>
    /// Manages tutorial slide sequence with navigation
    /// </summary>
    public class TutorialSystem : UI_GameplayManagersComponent
    {
        [SerializeField] private GameObject _tutorialPanel;
        [SerializeField] private List<GameObject> _slides;
        [SerializeField] private Button _nextButton;
        [SerializeField] private Button _prevButton;
        [SerializeField] private Button _closeButton;

        private int _currentSlideIndex = 0;

        public override void Initialize()
        {
            base.Initialize();

            if (_nextButton != null) _nextButton.onClick.AddListener(ShowNextSlide);
            if (_prevButton != null) _prevButton.onClick.AddListener(ShowPreviousSlide);
            if (_closeButton != null) _closeButton.onClick.AddListener(HideShowPanel);

            //Hide();
        }

        public void HideShowPanel()
        {
            if (_tutorialPanel.activeSelf)
            {
                _tutorialPanel.SetActive(false);
            }
            else
            {
                _tutorialPanel.SetActive(true);
                Show();
            }
        }

        public override void Show()
        {
            base.Show();
            _currentSlideIndex = 0;
            _tutorialPanel.SetActive(true);
            UpdateSlideVisibility();
        }

        private void UpdateSlideVisibility()
        {
            for (int i = 0; i < _slides.Count; i++)
            {
                _slides[i].SetActive(i == _currentSlideIndex);
            }

            if (_prevButton != null) _prevButton.interactable = _currentSlideIndex > 0;
            if (_nextButton != null) _nextButton.interactable = _currentSlideIndex < _slides.Count - 1;
        }

        private void ShowNextSlide()
        {
            if (_currentSlideIndex < _slides.Count - 1)
            {
                _currentSlideIndex++;
                UpdateSlideVisibility();
            }
        }

        private void ShowPreviousSlide()
        {
            if (_currentSlideIndex > 0)
            {
                _currentSlideIndex--;
                UpdateSlideVisibility();
            }
        }
    }
}