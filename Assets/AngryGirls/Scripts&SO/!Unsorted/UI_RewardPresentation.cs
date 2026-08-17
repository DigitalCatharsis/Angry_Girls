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
        [SerializeField] private GameObject _rewardReceivedCheckmark;
        [SerializeField] private Sprite _defaultCreditsIcon;
        [SerializeField] private Sprite _defaultItemIcon;

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
        [SerializeField] private AudioClipData _rewardSoundData;
        [SerializeField] private AudioClipData _rewardMusicData;
        [SerializeField] private AudioClipData _coinSoundData;

        [Header("Animation Timing")]
        [SerializeField] private float _elementDelay = 0.3f;
        [SerializeField] private float _barFillDuration = 0.8f;
        [SerializeField] private float _rewardPopDuration = 0.5f;

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

            _continueButton.onClick.AddListener((() => OnContinuePressed().Forget()));

            base.Initialize();
        }

        /// <summary>
        /// Show reward presentation with full animation sequence.
        /// </summary>
        public async UniTask ShowAndGrantRewardAsync(int coins)
        {

            var missionData = _missionsManager.GetMissionData(
                _missionsManager.CurrentMission,
                _missionsManager.CurrentDifficulty);

            var rewarddata = missionData.rewardData;

            RewardGrantResult rewardGrantResult = null;

            rewardGrantResult = await _rewardService.GrantRewardAsync(rewarddata, coins, missionData.isRewardReceived);

            _missionsManager.CompleteCurrentMission();

            var presentationData = BuildPresentationData(rewardGrantResult, coins);

            gameObject.SetActive(true);
            _continueButton.interactable = false;

            // Reset visuals
            ResetVisuals();

            // Play victory music
            PlayVictoryMusic();

            // Build animation sequence
            _animationSequence = DOTween.Sequence();

            // 1. Fade in background
            _animationSequence.Append(_backgroundGroup.DOFade(1f, _backgroundFadeDuration));

            // 2. Title pop
            _animationSequence.Append(_titleText.transform.DOScale(1.2f, 0.3f).SetEase(Ease.OutBack)).AppendCallback(() => _titleText.text = _victoryTitle);

            // 3. Coins collected (with count-up animation)
            _animationSequence.AppendInterval(_elementDelay);
            _animationSequence.AppendCallback(() =>
            {
                PlaySound(_coinSoundData);
                AnimateCoinCount(presentationData.collectedCoinsScore);
            });

            // 4. Reward display
            _animationSequence.AppendInterval(_elementDelay);
            _animationSequence.AppendCallback(() =>
            {
                PlaySound(_rewardSoundData);
                ShowReward(presentationData);
            });

            // 5. Characters XP bars
            _animationSequence.AppendInterval(_elementDelay);
            _animationSequence.AppendCallback(() =>
            {
                ShowCharacterEntries(presentationData.characterEntries);
            });

            // 6. Confetti
            _animationSequence.AppendCallback(() =>
            {
                if (_confettiParticles != null)
                    _confettiParticles.Play();
            });

            // 7. Enable continue button after delay
            _animationSequence.AppendInterval(_delayBeforeContinue);
            _animationSequence.AppendCallback(() =>
            {
                _continueButton.interactable = true;
            });

            _animationSequence.Play();

            // Wait for user to press continue
            await UniTask.WaitUntil(() => !_continueButton.interactable || !gameObject.activeSelf);
            await UniTask.WaitUntil(() => !gameObject.activeSelf);
        }




        /// <summary>
        /// Builds the data package for reward presentation display.
        /// Includes reward result, collected coins, and character XP stubs.
        /// </summary>
        private RewardPresentationData BuildPresentationData(
            RewardGrantResult rewardResult, int collectedCoins)
        {
            var data = new RewardPresentationData
            {
                rewardResult = rewardResult,
                collectedCoinsScore = collectedCoins,
                characterEntries = new List<CharacterRewardEntry>()
            };

            // Get selected characters from CharactersManager
            var charactersData = CoreManager.Instance.CharactersManager.CharactersData;
            if (charactersData != null)
            {
                foreach (var character in charactersData.SelectedCharactersPool)
                {
                    if (character == null || character.CharacterSettings == null) continue;

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
            await GameplayCoreManager.Instance.GameLogic.ExecuteRewardRecieved();
        }

        public override void Hide()
        {
            if (_animationSequence != null && _animationSequence.IsActive())
            {
                _animationSequence.Kill();
            }
            _backgroundGroup.alpha = 0f;
            base.Hide();
        }

        private void ResetVisuals()
        {
            _backgroundGroup.alpha = 0f;
            _titleText.text = "";
            _titleText.transform.localScale = Vector3.zero;
            _coinsCollectedText.text = "Coins: 0";
            _rewardIcon.sprite = null;
            _rewardIcon.enabled = false;
            _rewardNameText.text = "";
            _rewardReceivedCheckmark?.SetActive(false);
            _continueButton.interactable = false;

            // Clear character entries
            foreach (Transform child in _charactersContainer)
                Destroy(child.gameObject);

            if (_confettiParticles != null)
                _confettiParticles.Stop();
        }

        private void PlayVictoryMusic()
        {
            if (_rewardMusicData != null)
            {
                CoreManager.Instance.AudioManager.PlayClipData(
                    _rewardMusicData, AudioCategory.Music, false);
            }
        }

        private void PlaySound(AudioClipData data)
        {
            if (data != null)
            {
                CoreManager.Instance.AudioManager.PlayClipData(data, AudioCategory.SFX, true);
            }
        }

        private void AnimateCoinCount(int targetCount)
        {
            _coinsCollectedText.text = "Coins: 0";
            DOTween.To(
                () => 0,
                x => _coinsCollectedText.text = $"Coins: {x}",
                targetCount,
                0.8f
            ).SetEase(Ease.OutQuad);
        }

        private void ShowReward(RewardPresentationData data)
        {
            var result = data.rewardResult;
            if (result == null) return;

            // Show checkmark if reward was already received
            if (result.isDuplicate || result.rewardType == RewardType.None)
            {
                _rewardReceivedCheckmark?.SetActive(true);
            }

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
                    // Load actual icon async
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

            // Pop animation for reward icon
            _rewardIcon.transform.localScale = Vector3.zero;
            _rewardIcon.transform.DOScale(1f, _rewardPopDuration).SetEase(Ease.OutBack);
        }

        private async UniTaskVoid LoadItemIcon(ItemSettings settings)
        {
            if (settings == null || settings.IconReference == null) return;
            if (string.IsNullOrEmpty(settings.IconReference.AssetGUID)) return;

            var sprite = await _assetProvider.LoadSpriteAsync(settings.IconReference);
            if (sprite != null && this != null)
            {
                _rewardIcon.sprite = sprite;
            }
        }

        private async UniTaskVoid LoadCharacterIcon(CharacterSettings settings)
        {
            if (settings == null || settings.portrait == null) return;
            if (string.IsNullOrEmpty(settings.portrait.AssetGUID)) return;

            var sprite = await _assetProvider.LoadSpriteAsync(settings.portrait);
            if (sprite != null && this != null)
            {
                _rewardIcon.sprite = sprite;
            }
        }

        private void ShowCharacterEntries(List<CharacterRewardEntry> entries)
        {
            if (_charactersContainer == null)
            {
                Debug.LogError("UI_RewardPresentation: _charactersContainer is not assigned!");
                return;
            }

            // Clear existing entries
            foreach (Transform child in _charactersContainer)
            {
                Destroy(child.gameObject);
            }

            if (entries == null || entries.Count == 0)
            {
                Debug.LogWarning("UI_RewardPresentation: No character entries to display");
                return;
            }

            // Spawn entry for each character
            foreach (var entry in entries)
            {
                if (entry == null || entry.characterSettings == null) continue;

                var entryGO = Instantiate(_characterEntryPrefab, _charactersContainer);
                var entryComponent = entryGO.GetComponent<UI_CharacterRewardEntry>();

                if (entryComponent != null)
                {
                    entryComponent.Setup(entry, _barFillDuration, _elementDelay);
                }
                else
                {
                    Debug.LogError("UI_RewardPresentation: UI_CharacterRewardEntry component not found on prefab!");
                }
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