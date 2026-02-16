using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Angry_Girls
{
    /// <summary>
    /// Loading screen with progress bar and text.
    /// </summary>
    public class UI_LoadingScreen : UI_UIScreen
    {
        [Header("Loading UI")]
        [SerializeField] private Slider _progressBar;
        [SerializeField] private TextMeshProUGUI _loadingText;
        [SerializeField] private TextMeshProUGUI _progressText;

        /// <inheritdoc/>
        public override void Initialize()
        {
            base.Initialize();
            Hide();
        }

        /// <summary>
        /// Update the progress bar and text.
        /// </summary>
        public void UpdateProgress(float progress)
        {
            if (_progressBar != null)
            {
                _progressBar.value = progress;
            }

            if (_progressText != null)
            {
                _progressText.text = $"{Mathf.RoundToInt(progress * 100)}%";
            }
        }

        /// <summary>
        /// Set the loading text.
        /// </summary>
        public void SetLoadingText(string text)
        {
            if (_loadingText != null)
            {
                _loadingText.text = text;
            }
        }

        /// <inheritdoc/>
        public override void Show()
        {
            base.Show();
            UpdateProgress(0f);
            SetLoadingText("Loading...");
        }
    }
}