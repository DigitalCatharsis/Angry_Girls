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
        private CoinCollector _coinCollector;

        public override void Initialize()
        {
            base.Initialize();

            _coinCollector = GameplayCoreManager.Instance.gameObject.AddComponent<CoinCollector>();
            _coinCollector.OnCoinCollected += AddScore;

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

    /// <summary>
    /// Helper component to collect coin pickup events
    /// </summary>
    public class CoinCollector : MonoBehaviour
    {
        public event System.Action<int> OnCoinCollected;

        private void OnEnable()
        {
            GameplayCoreManager.Instance.OnInitialized += RegisterCoinHandlers;
        }

        private void OnDisable()
        {
            GameplayCoreManager.Instance.OnInitialized -= RegisterCoinHandlers;
        }

        private void RegisterCoinHandlers()
        {
            //TODO:
            // Subscribe to coin pickup events via InteractionManager
            // This is a simplified version - actual implementation would hook into IPickable.OnPickUp
        }
    }
}