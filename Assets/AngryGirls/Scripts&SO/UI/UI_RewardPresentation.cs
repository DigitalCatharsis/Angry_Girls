using Cysharp.Threading.Tasks;
using DG.Tweening;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Angry_Girls
{
    /// <summary>
    /// Fullscreen reward presentation shown after mission victory.
    /// Displays reward, character XP progress, and collected coins.
    /// </summary>
    public class UI_RewardPresentation : UI_GameplayManagersComponent
    {
        [Header("Background")]
        [SerializeField] private CanvasGroup _backgroundGroup;
        [SerializeField] private float _backgroundFadeDuration = 0.4f;

        [Header("Title")]
        [SerializeField] private TextMeshProUGUI _titleText;
        [SerializeField] private string _victoryTitle = "MISSION COMPLETE!";

        [Header("Reward Section")]
        [SerializeField] private Image _rewardIcon;
        [SerializeField] private TextMeshProUGUI _rewardNameText;
        [SerializeField] private Sprite _defaultCreditsIcon;
        [SerializeField] private Sprite _defaultItemIcon;

        [Header("Reward Received Overlay")]
        [SerializeField] private GameObject _rewardReceivedCheckmark;
        [SerializeField] private Image _rewardReceivedBackground;

        [Header("Coins Section")]
        [SerializeField] private TextMeshProUGUI _coinsCollectedText;
        [SerializeField] private Image _coinIcon;

        [Header("Characters Section")]
        [SerializeField] private Transform _charactersContainer;
        [SerializeField] private GameObject _characterEntryPrefab;

        [Header("Continue Button")]
        [SerializeField] private Button _continueButton;
        [SerializeField] private float _delayBeforeContinue = 1.5f;

        [Header("Particles")]
        [SerializeField] private ParticleSystem _confettiParticles;

        [Header("Audio")]
        [SerializeField] private AudioClipData _rewardMusicData;
        [SerializeField] private AudioClipData _rewardReceiveSfxData;
        [SerializeField] private AudioClipData _coinSoundData;

        [Header("Animation Timing")]
        [SerializeField] private float _coinCountDuration = 0.8f;
        [SerializeField] private float _rewardPopDuration = 0.5f;
        [SerializeField] private float _barFillDuration = 0.8f;
        [SerializeField] private float _barDelay = 0.3f;

        private IAssetProvider _assetProvider;
        private MissionsManager _missionsManager;
        private Sequence _animationSequence;
        private RewardService _rewardService;

        /// <summary>
        /// Initialize with asset provider for loading icons.
        /// </summary>
        public override void Initialize()
        {
            _assetProvider = CoreManager.Instance.AddressableAssetManager;
            _missionsManager = CoreManager.Instance.MissionsManager;

            if (_rewardService == null)
            {
                _rewardService = new RewardService(
                    CoreManager.Instance.InventoryManager,
                    CoreManager.Instance.CharactersManager,
                    CoreManager.Instance.CreditsManager,
                    CoreManager.Instance.ItemSettingsRepository,
                    CoreManager.Instance.CharacterSettingsCatalogSO
                );
            }

            if (_continueButton != null)
            {
                _continueButton.onClick.RemoveAllListeners();
                _continueButton.onClick.AddListener(() => OnContinuePressed().Forget());
            }

            base.Initialize();
        }

        /// <summary>
        /// Main entry point. Grants reward and plays full presentation sequence.
        /// </summary>
        public async UniTask ShowAndGrantRewardAsync(int coins)
        {
            var missionData = _missionsManager.GetMissionData(
                _missionsManager.CurrentMission,
                _missionsManager.CurrentDifficulty);

            var rewarddata = missionData.rewardData;
            var alreadyReceived = missionData.isRewardReceived;

            RewardGrantResult rewardGrantResult = await _rewardService.GrantRewardAsync(rewarddata, coins, alreadyReceived);
            _missionsManager.CompleteCurrentMission();

            var presentationData = BuildPresentationData(rewardGrantResult, coins);

            gameObject.SetActive(true);
            if (_continueButton != null) _continueButton.interactable = false;

            ResetVisuals();
            PlayVictoryMusic();

            _animationSequence = DOTween.Sequence();

            // 1. Fade in background
            _animationSequence.Append(_backgroundGroup.DOFade(1f, _backgroundFadeDuration));

            // 2. Title pop
            _animationSequence.Append(_titleText.transform.DOScale(1.2f, 0.3f).SetEase(Ease.OutBack))
                              .AppendCallback(() => _titleText.text = _victoryTitle);

            // 3. Show characters and reward icon immediately (static, no pop animation yet)
            _animationSequence.AppendCallback(() =>
            {
                ShowCharacterEntries(presentationData.characterEntries);
                ShowRewardStatic(presentationData, alreadyReceived);
            });

            // 4. Start confetti
            _animationSequence.AppendCallback(() =>
            {
                if (_confettiParticles != null)
                    _confettiParticles.Play();
            });

            // 5. Animate coins
            _animationSequence.AppendCallback(() =>
            {
                AnimateCoinCountWithSound(presentationData.collectedCoinsScore);
            });

            // Wait for coins to finish counting
            _animationSequence.AppendInterval(_coinCountDuration);

            // 6. After coins, play reward animation and SFX
            _animationSequence.AppendCallback(() =>
            {
                PlayRewardRevealAnimation(alreadyReceived);
            });

            // Wait for reward animation
            float rewardAnimDuration = alreadyReceived ? 0.3f : _rewardPopDuration * 2f + 0.3f;
            _animationSequence.AppendInterval(rewardAnimDuration);

            // 7. Enable continue button
            _animationSequence.AppendInterval(_delayBeforeContinue);
            _animationSequence.AppendCallback(() =>
            {
                if (_continueButton != null) _continueButton.interactable = true;
            });

            _animationSequence.Play();

            // Wait for user to press continue or object destruction.
            // Using 'this == null' prevents MissingReferenceException when the object is destroyed during scene transition.
            var cts = this.GetCancellationTokenOnDestroy();
            await UniTask.WaitUntil(() => this == null || !gameObject.activeSelf, cancellationToken: cts);
        }

        /// <summary>
        /// Builds the data package for reward presentation display.
        /// </summary>
        private RewardPresentationData BuildPresentationData(RewardGrantResult rewardResult, int collectedCoins)
        {
            var data = new RewardPresentationData
            {
                rewardResult = rewardResult,
                collectedCoinsScore = collectedCoins,
                characterEntries = new List<CharacterRewardEntry>()
            };

            var charactersData = CoreManager.Instance.CharactersManager.CharactersData;
            if (charactersData != null)
            {
                foreach (var character in charactersData.SelectedCharactersPool)
                {
                    if (character?.CharacterSettings == null) continue;

                    data.characterEntries.Add(new CharacterRewardEntry
                    {
                        characterSettings = character.CharacterSettings,
                        xpGained = UnityEngine.Random.Range(50, 200),
                        currentLevel = UnityEngine.Random.Range(1, 10),
                        currentXp = UnityEngine.Random.Range(100, 500),
                        xpForNextLevel = 1000
                    });
                }
            }

            return data;
        }

        /// <summary>
        /// Called when continue button is pressed.
        /// </summary>
        public async UniTaskVoid OnContinuePressed()
        {
            if (GameplayCoreManager.Instance?.GameLogic != null)
            {
                await GameplayCoreManager.Instance.GameLogic.ExecuteRewardRecieved();
            }
        }

        public override void Hide()
        {
            if (_animationSequence != null && _animationSequence.IsActive())
                _animationSequence.Kill();

            if (_backgroundGroup != null)
                _backgroundGroup.alpha = 0f;

            base.Hide();
        }

        private void ResetVisuals()
        {
            if (_backgroundGroup != null) _backgroundGroup.alpha = 0f;
            if (_titleText != null)
            {
                _titleText.text = "";
                _titleText.transform.localScale = Vector3.one;
            }

            if (_coinsCollectedText != null) _coinsCollectedText.text = "Coins: 0";

            if (_rewardIcon != null)
            {
                _rewardIcon.sprite = null;
                _rewardIcon.enabled = false;
                _rewardIcon.transform.localScale = Vector3.one;
            }

            if (_rewardNameText != null) _rewardNameText.text = "";

            if (_rewardReceivedCheckmark != null)
                _rewardReceivedCheckmark.SetActive(false);

            if (_rewardReceivedBackground != null)
                _rewardReceivedBackground.enabled = false;

            if (_confettiParticles != null)
                _confettiParticles.Stop();

            if (_charactersContainer != null)
            {
                foreach (Transform child in _charactersContainer)
                    Destroy(child.gameObject);
            }
        }

        private void PlayVictoryMusic()
        {
            if (_rewardMusicData != null && CoreManager.Instance?.AudioManager != null)
            {
                CoreManager.Instance.AudioManager.PlayClipData(
                    _rewardMusicData, AudioCategory.Music, false);
            }
        }

        private void PlaySound(AudioClipData data)
        {
            if (data == null || CoreManager.Instance?.AudioManager == null) return;
            CoreManager.Instance.AudioManager.PlayClipData(
                data, data.fallbackCategory, false);
        }

        /// <summary>
        /// Animates coin counter with sound played on each tick.
        /// </summary>
        private void AnimateCoinCountWithSound(int targetCount)
        {
            if (_coinsCollectedText == null) return;

            if (targetCount <= 0)
            {
                _coinsCollectedText.text = "Coins: 0";
                return;
            }

            int lastSoundValue = -1;
            int soundInterval = Mathf.Max(1, targetCount / 8);

            DOTween.To(
                () => 0,
                x =>
                {
                    if (this == null || _coinsCollectedText == null) return;

                    int intValue = Mathf.FloorToInt(x);
                    _coinsCollectedText.text = $"Coins: {intValue}";

                    if (intValue - lastSoundValue >= soundInterval)
                    {
                        lastSoundValue = intValue;
                        PlaySound(_coinSoundData);
                    }
                },
                targetCount,
                _coinCountDuration
            ).SetEase(Ease.OutQuad);
        }

        /// <summary>
        /// Shows reward icon and text immediately without pop animation.
        /// </summary>
        private void ShowRewardStatic(RewardPresentationData data, bool alreadyReceived)
        {
            var result = data.rewardResult;
            if (result == null) return;

            switch (result.rewardType)
            {
                case RewardType.Credits:
                    _rewardNameText.text = $"+{result.creditsAmount} Credits";
                    _rewardIcon.sprite = _defaultCreditsIcon;
                    _rewardIcon.enabled = true;
                    break;

                case RewardType.Item:
                    _rewardNameText.text = result.itemSettings?.ItemName ?? "Item";
                    _rewardIcon.sprite = _defaultItemIcon;
                    _rewardIcon.enabled = true;
                    LoadItemIcon(result.itemSettings).Forget();
                    break;

                case RewardType.Character:
                    _rewardNameText.text = result.characterSettings?.name ?? "Character";
                    _rewardIcon.sprite = _defaultItemIcon;
                    _rewardIcon.enabled = true;
                    LoadCharacterIcon(result.characterSettings).Forget();
                    break;

                default:
                    _rewardNameText.text = "No Reward";
                    _rewardIcon.enabled = false;
                    break;
            }

            _rewardIcon.transform.localScale = Vector3.one;

            if (alreadyReceived)
            {
                ShowReceivedOverlay();
            }
        }

        /// <summary>
        /// Plays reward pop animation and SFX after coins are counted.
        /// </summary>
        private void PlayRewardRevealAnimation(bool alreadyReceived)
        {
            if (alreadyReceived) return;

            PlaySound(_rewardReceiveSfxData);

            _rewardIcon.transform.localScale = Vector3.zero;

            var rewardSeq = DOTween.Sequence();
            rewardSeq.Append(
                _rewardIcon.transform.DOScale(1.3f, _rewardPopDuration)
                    .SetEase(Ease.OutBack)
            );
            rewardSeq.Append(
                _rewardIcon.transform.DOScale(1f, _rewardPopDuration)
                    .SetEase(Ease.InOutQuad)
            );
            rewardSeq.AppendCallback(() =>
            {
                ShowReceivedOverlay();
            });
        }

        /// <summary>
        /// Shows checkmark and green transparent background over reward icon.
        /// </summary>
        private void ShowReceivedOverlay()
        {
            if (_rewardReceivedCheckmark != null)
                _rewardReceivedCheckmark.SetActive(true);

            if (_rewardReceivedBackground != null)
                _rewardReceivedBackground.enabled = true;
        }

        private async UniTaskVoid LoadItemIcon(ItemSettings settings)
        {
            if (settings?.IconReference == null) return;
            if (string.IsNullOrEmpty(settings.IconReference.AssetGUID)) return;

            try
            {
                var sprite = await _assetProvider.LoadSpriteAsync(settings.IconReference);
                if (sprite != null && this != null && _rewardIcon != null)
                    _rewardIcon.sprite = sprite;
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"UI_RewardPresentation: Failed to load item icon: {ex.Message}");
            }
        }

        private async UniTaskVoid LoadCharacterIcon(CharacterSettings settings)
        {
            if (settings?.portrait == null) return;
            if (string.IsNullOrEmpty(settings.portrait.AssetGUID)) return;

            try
            {
                var sprite = await _assetProvider.LoadSpriteAsync(settings.portrait);
                if (sprite != null && this != null && _rewardIcon != null)
                    _rewardIcon.sprite = sprite;
            }
            catch (System.Exception ex)
            {
                Debug.LogWarning($"UI_RewardPresentation: Failed to load character icon: {ex.Message}");
            }
        }

        private void ShowCharacterEntries(List<CharacterRewardEntry> entries)
        {
            if (_charactersContainer == null) return;

            foreach (Transform child in _charactersContainer)
                Destroy(child.gameObject);

            if (entries == null || entries.Count == 0) return;

            foreach (var entry in entries)
            {
                if (entry?.characterSettings == null) continue;

                var entryGO = Instantiate(_characterEntryPrefab, _charactersContainer);
                var entryComponent = entryGO.GetComponent<UI_CharacterRewardEntry>();
                if (entryComponent != null)
                {
                    entryComponent.Setup(entry, _barFillDuration, _barDelay);
                }
            }
        }

        private void OnDestroy()
        {
            if (_animationSequence != null && _animationSequence.IsActive())
                _animationSequence.Kill();

            if (_continueButton != null)
                _continueButton.onClick.RemoveAllListeners();
        }
    }
}