using Cysharp.Threading.Tasks;
using DG.Tweening;
using System;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Angry_Girls
{
    /// <summary>
    /// Reward presentation screen with animations and audio.
    /// Inherits from UI_UIScreen for consistent lifecycle management.
    /// </summary>
    public class UI_RewardPresentation : UI_UIScreen
    {
        [Header("UI Components")]
        [SerializeField] private Image _rewardIcon;
        [SerializeField] private TextMeshProUGUI _rewardTitle;
        [SerializeField] private TextMeshProUGUI _rewardDescription;
        [SerializeField] private Button _continueButton;
        [SerializeField] private ParticleSystem _confettiParticles;

        [Header("Animation Settings")]
        [SerializeField] private float _iconScaleDuration = 0.5f;
        [SerializeField] private float _textFadeDuration = 0.3f;
        [SerializeField] private float _delayBeforeContinue = 1.0f;

        [Header("Audio")]
        [SerializeField] private AudioClipData _rewardMusicData;
        [SerializeField] private AudioClipData _rewardSfxData;

        private CancellationTokenSource _cancellationTokenSource;
        private bool _isShowing = false;
        private AudioManager _audioManager;
        private bool _musicPlaying = false;

        private void Awake()
        {
            _audioManager = CoreManager.Instance?.AudioManager;

            if (_continueButton != null)
                _continueButton.onClick.AddListener(OnContinuePressed);

            Hide();
        }

        /// <summary>
        /// Show reward presentation with animation and audio.
        /// </summary>
        public async UniTaskVoid ShowRewardAsync(RewardGrantResult rewardResult)
        {
            if (_isShowing) return;
            _isShowing = true;

            _cancellationTokenSource = new CancellationTokenSource();
            var token = _cancellationTokenSource.Token;

            try
            {
                Show();
                _continueButton.interactable = false;

                // Setup content
                SetupRewardContent(rewardResult);

                // Play audio
                PlayRewardAudio();

                // Play animation sequence
                await PlayRewardAnimation(token);

                // Wait before allowing continue
                await UniTask.Delay((int)(_delayBeforeContinue * 1000), cancellationToken: token);

                _continueButton.interactable = true;

                // Wait for player to press continue
                await UniTask.WaitUntil(() => !_isShowing, cancellationToken: token);
            }
            catch (OperationCanceledException)
            {
                // Panel closed externally
            }
            finally
            {
                _isShowing = false;
                StopRewardAudio();
                Hide();
                _cancellationTokenSource?.Dispose();
            }
        }

        private void SetupRewardContent(RewardGrantResult result)
        {
            _rewardTitle.text = result.isDuplicate ? "DUPLICATE!" : "REWARD EARNED!";
            _rewardTitle.color = result.isDuplicate ? Color.yellow : Color.green;

            _rewardDescription.text = result.message;

            // Load icon based on reward type
            LoadRewardIcon(result).Forget();

            if (_confettiParticles != null && !result.isDuplicate)
                _confettiParticles.Play();
        }

        private async UniTask LoadRewardIcon(RewardGrantResult result)
        {
            if (_rewardIcon == null) return;

            Sprite iconSprite = null;

            switch (result.rewardType)
            {
                case RewardType.Credits:
                    // Load credits icon from resources or assign in inspector
                    iconSprite = Resources.Load<Sprite>("Icons/CreditsIcon");
                    break;

                case RewardType.Item:
                    if (result.itemSettings != null && result.itemSettings.IconReference != null)
                    {
                        iconSprite = await CoreManager.Instance.AddressableAssetManager
                            .LoadSpriteAsync(result.itemSettings.IconReference);
                    }
                    break;

                case RewardType.Character:
                    if (result.characterSettings != null && result.characterSettings.portrait != null)
                    {
                        iconSprite = await CoreManager.Instance.AddressableAssetManager
                            .LoadSpriteAsync(result.characterSettings.portrait);
                    }
                    break;
            }

            if (iconSprite != null)
            {
                _rewardIcon.sprite = iconSprite;
                _rewardIcon.enabled = true;
            }
            else
            {
                _rewardIcon.enabled = false;
            }
        }

        private async UniTaskVoid PlayRewardAnimation(CancellationToken token)
        {
            if (_rewardIcon != null)
            {
                // Icon scale animation
                _rewardIcon.transform.localScale = Vector3.zero;
                await _rewardIcon.transform.DOScale(1f, _iconScaleDuration)
                    .SetEase(Ease.OutBack)
                    .ToUniTask(cancellationToken: token);
            }

            if (_rewardDescription != null)
            {
                // Text fade in
                _rewardDescription.alpha = 0;
                await _rewardDescription.DOFade(1f, _textFadeDuration)
                    .ToUniTask(cancellationToken: token);
            }
        }

        private void PlayRewardAudio()
        {
            if (_audioManager == null) return;

            // Play SFX
            if (_rewardSfxData != null)
            {
                _audioManager.PlayClipData(_rewardSfxData, _rewardSfxData.fallbackCategory, false);
            }

            // Play music (optional, short fanfare)
            if (_rewardMusicData != null)
            {
                _audioManager.PlayMusicAsync(_rewardMusicData, crossfadeDuration: 0.5f).Forget();
                _musicPlaying = true;
            }
        }

        private void StopRewardAudio()
        {
            if (_audioManager == null) return;

            if (_musicPlaying)
            {
                _audioManager.StopMusic();
                _musicPlaying = false;
            }
        }

        private void OnContinuePressed()
        {
            _isShowing = false;
        }

        public override void Hide()
        {
            base.Hide();

            if (_confettiParticles != null)
                _confettiParticles.Stop();
        }

        private void OnDestroy()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            StopRewardAudio();
        }
    }
}