using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Angry_Girls
{
    /// <summary>
    /// Single character entry in reward presentation.
    /// Shows portrait, name, level, and XP bar with fill animation.
    /// </summary>
    public class UI_CharacterRewardEntry : MonoBehaviour
    {
        [SerializeField] private Image _portraitImage;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private Image _xpBarFill;
        [SerializeField] private TextMeshProUGUI _xpText;

        private Sequence _animationSequence;

        /// <summary>
        /// Setup entry with data and start fill animation.
        /// </summary>
        public void Setup(CharacterRewardEntry data, float barFillDuration, float delay)
        {
            _nameText.text = GetDisplayName(data.characterSettings);
            ApplyPortrait(data);

            // Start from the state before the mission, the bar then plays every level up of the reward.
            ShowProgress(data.previousLevel, data.previousXp, data.previousXpForNextLevel, data, false);
            _xpBarFill.fillAmount = GetFill(data.previousXp, data.previousXpForNextLevel);

            _animationSequence = DOTween.Sequence();
            _animationSequence.AppendInterval(delay);

            var levelsGained = Mathf.Max(0, data.currentLevel - data.previousLevel);
            var segmentDuration = barFillDuration / (levelsGained + 1);

            // Every gained level fills the bar to the end, switches the level and starts again from zero.
            for (var step = 1; step <= levelsGained; step++)
            {
                var level = data.previousLevel + step;
                _animationSequence.Append(_xpBarFill.DOFillAmount(1f, segmentDuration).SetEase(Ease.Linear));
                _animationSequence.AppendCallback(() =>
                {
                    if (this == null) return;
                    _xpBarFill.fillAmount = 0f;
                    ShowProgress(level, 0, data.xpForNextLevel, data, true);
                });
            }

            var finalFill = data.xpForNextLevel > 0 ? GetFill(data.currentXp, data.xpForNextLevel) : 1f;
            _animationSequence.Append(_xpBarFill.DOFillAmount(finalFill, segmentDuration).SetEase(Ease.OutQuad));
            _animationSequence.AppendCallback(() =>
            {
                if (this == null) return;
                ShowProgress(data.currentLevel, data.currentXp, data.xpForNextLevel, data, data.LeveledUp);
            });
            _animationSequence.Play();
        }

        private void ShowProgress(int level, int xp, int xpForNext, CharacterRewardEntry data, bool leveledUp)
        {
            // Every text is a single line: the prefab rows have the height of one line.
            // The stat gains (data.healthGain, data.damageGain) do not fit into the single line rows of the prefab,
            // the new stats are shown in the character panels.
            _levelText.text = leveledUp ? $"Lv. {level}  LEVEL UP!" : $"Lv. {level}";

            _xpText.text = xpForNext <= 0
                ? $"MAX LEVEL  (+{data.xpGained} XP)"
                : $"{xp} / {xpForNext} XP  (+{data.xpGained})";
        }

        /// <summary>
        /// Readable name of a character asset, for example "AirToGround Red" instead of the asset name.
        /// </summary>
        private static string GetDisplayName(CharacterSettings settings)
        {
            if (settings == null) return "Unknown";

            return settings.name
                .Replace("SO_CharacterSettings_", string.Empty)
                .Replace("Player_YBot_", string.Empty)
                .Replace("_", " ");
        }

        private static float GetFill(int xp, int xpForNext)
        {
            return xpForNext > 0 ? Mathf.Clamp01((float)xp / xpForNext) : 1f;
        }

        private void ApplyPortrait(CharacterRewardEntry data)
        {
            var sprite = data.characterSettings != null ? data.characterSettings.portrait : null;
            if (sprite == null || _portraitImage == null) return;

            _portraitImage.sprite = sprite;
            _portraitImage.enabled = true;
        }

        private void OnDestroy()
        {
            if (_animationSequence != null && _animationSequence.IsActive())
            {
                _animationSequence.Kill();
            }
        }
    }
}