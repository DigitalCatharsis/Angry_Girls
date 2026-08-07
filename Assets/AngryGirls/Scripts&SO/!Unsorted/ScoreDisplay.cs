using Angry_Girls;
using TMPro;

using UnityEngine;

/// <summary>
/// Displays and updates player score from collected coins.
/// </summary>
public class ScoreDisplay : UI_GameplayManagersComponent
{
    [SerializeField] private TextMeshProUGUI _scoreText;
    [SerializeField] private string _scoreFormat = "Score: {0}";
    private int _currentScore;

    public override void Initialize()
    {
        base.Initialize();
        _currentScore = 0;
        UpdateDisplay();

        // Subscribe to coin collection events
        GameplayCoreManager.Instance.OnCoinCollected += AddScore;
    }

    /// <summary>
    /// Add score from collected coin.
    /// </summary>
    public void AddScore(int value)
    {
        _currentScore += value;
        UpdateDisplay();
    }

    /// <summary>
    /// Get current score for conversion to credits.
    /// </summary>
    public int GetScore() => _currentScore;

    private void UpdateDisplay()
    {
        if (_scoreText != null)
        {
            _scoreText.text = string.Format(_scoreFormat, _currentScore);
        }
    }

    private void OnDestroy()
    {
        if (GameplayCoreManager.Instance != null)
        {
            GameplayCoreManager.Instance.OnCoinCollected -= AddScore;
        }
    }
}