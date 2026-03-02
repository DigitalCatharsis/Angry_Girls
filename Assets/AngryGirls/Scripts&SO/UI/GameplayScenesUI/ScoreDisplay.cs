using TMPro;
using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Displays and updates player score from collected coins
    /// </summary>
    public class ScoreDisplay : UI_GameplayManagersComponent
    {
        [SerializeField] private TextMeshProUGUI _scoreText;
        [SerializeField] private string _scoreFormat = "Score: {0}";

        private int _currentScore;

        public override void Initialize()
        {
            base.Initialize();
            //_coinCollector.OnCoinCollected += AddScore;

            _currentScore = 0;
            UpdateDisplay();
        }

        public void AddScore(int value)
        {
            _currentScore += value;
            UpdateDisplay();
        }

        private void UpdateDisplay()
        {
            if (_scoreText != null)
            {
                _scoreText.text = string.Format(_scoreFormat, _currentScore);
            }
        }
    }
}