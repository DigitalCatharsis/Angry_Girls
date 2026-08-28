using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Angry_Girls
{
    /// <summary>
    /// Displays one action in the predicted turn order.
    /// </summary>
    public sealed class TurnOrderSegmentUI : MonoBehaviour
    {
        [Header("UI")]
        [SerializeField] private Image _portrait;
        [SerializeField] private TextMeshProUGUI _actionText;
        [SerializeField] private Image _currentHighlight;
        [SerializeField] private GameObject _endVisual;

        [Header("Fallback")]
        [SerializeField] private Sprite _fallbackPortrait;

        private CControl _character;
        private int _setupVersion;

        /// <summary>
        /// Configures the segment for a character action.
        /// </summary>
        public void SetupCharacter(
            CControl character,
            bool isLaunch,
            bool isCurrent)
        {
            _setupVersion++;

            var setupVersion = _setupVersion;

            _character = character;

            if (_endVisual != null)
                _endVisual.SetActive(false);

            if (_portrait != null)
            {
                _portrait.gameObject.SetActive(true);
            }

            if (_actionText != null)
            {
                _actionText.text =
                    isLaunch ? "L" : "A";
            }

            SetCurrent(isCurrent);
            ResetPortrait();

            if (_character == null)
                return;

            LoadPortraitAsync(
                _character,
                setupVersion)
                .Forget();
        }

        /// <summary>
        /// Configures the segment as the final END marker.
        /// </summary>
        public void SetupEnd()
        {
            _setupVersion++;

            _character = null;

            SetCurrent(false);

            if (_actionText != null)
                _actionText.text = string.Empty;

            if (_portrait != null)
            {
                _portrait.sprite = null;
                _portrait.enabled = false;
                _portrait.gameObject.SetActive(false);
            }

            if (_endVisual != null)
                _endVisual.SetActive(true);
        }

        /// <summary>
        /// Sets the current action highlight state.
        /// </summary>
        public void SetCurrent(bool isCurrent)
        {
            if (_currentHighlight != null)
                _currentHighlight.enabled = isCurrent;
        }

        private void ResetPortrait()
        {
            if (_portrait == null)
                return;

            _portrait.gameObject.SetActive(true);

            _portrait.sprite =
                _fallbackPortrait;

            _portrait.enabled =
                _fallbackPortrait != null;
        }

        private async UniTaskVoid LoadPortraitAsync(
            CControl character,
            int setupVersion)
        {
            if (character == null ||
                character.profile == null ||
                character.profile.CharacterSettings == null)
            {
                return;
            }

            var portraitReference =
                character
                    .profile
                    .CharacterSettings
                    .portrait;

            if (portraitReference == null ||
                !portraitReference.RuntimeKeyIsValid())
            {
                return;
            }

            try
            {
                var sprite =
                    await CoreManager.Instance
                        .AddressableAssetManager
                        .LoadSpriteAsync(
                            portraitReference);

                if (this == null ||
                    _portrait == null ||
                    setupVersion != _setupVersion ||
                    _character != character)
                {
                    return;
                }

                if (sprite != null)
                {
                    _portrait.sprite = sprite;
                    _portrait.enabled = true;
                }
            }
            catch (System.Exception ex)
            {
                if (this != null &&
                    setupVersion == _setupVersion)
                {
                    Debug.LogWarning(
                        $"TurnOrderSegmentUI: Failed to load portrait for '{character.name}': {ex.Message}");
                }
            }
        }
    }
}