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
            // Set static data
            _nameText.text = data.characterSettings?.name ?? "Unknown";
            _levelText.text = $"Lv. {data.currentLevel}";
            _xpText.text = $"{data.currentXp} / {data.xpForNextLevel} XP";

            LoadPortraitAsync(data).Forget();

            // Set bar fill to 0 initially
            _xpBarFill.fillAmount = 0f;

            // Calculate fill target
            float fillTarget = data.xpForNextLevel > 0
                ? (float)data.currentXp / data.xpForNextLevel
                : 0f;

            // Animate bar fill after delay
            _animationSequence = DOTween.Sequence();
            _animationSequence.AppendInterval(delay);
            _animationSequence.Append(
                _xpBarFill.DOFillAmount(fillTarget, barFillDuration)
                    .SetEase(Ease.OutQuad)
            );
            _animationSequence.Play();
        }

        private async UniTaskVoid LoadPortraitAsync (CharacterRewardEntry data)
        {
            try
            {
                var sprite = await CoreManager.Instance.AddressableAssetManager.LoadSpriteAsync(data.characterSettings.portrait);

                if (sprite != null)
                {
                    _portraitImage.sprite = sprite;
                    _portraitImage.enabled = true;
                }
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"UI_CharacterRewardEntry: Failed to load sprite: { ex.Message}");
            }
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