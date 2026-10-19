using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Angry_Girls
{
    /// <summary>
    /// Displays one future character action in the turn order.
    /// </summary>
    public sealed class UI_TurnOrderSegment
        : MonoBehaviour,
          IPointerEnterHandler,
          IPointerExitHandler
    {
        [Header("UI")]
        [SerializeField] private Image _portrait;
        [SerializeField] private TextMeshProUGUI _actionText;
        [SerializeField] private Image _currentHighlight;
        [SerializeField] private Button _portraitButton;

        [Header("End")]
        [SerializeField] private GameObject _endVisual;

        [Header("Fallback")]
        [SerializeField] private Sprite _fallbackPortrait;

        private CControl _character;
        private TurnOrderHoverIndicator _hoverIndicator;
        private int _setupVersion;
        private bool _isPointerOver;

        /// <summary>
        /// Gets the character represented by this segment.
        /// </summary>
        public CControl Character =>
            _character;

        private void Awake()
        {
            if (_portraitButton != null)
            {
                _portraitButton.onClick.AddListener(
                    OnPortraitClicked);
            }
        }

        /// <summary>
        /// Assigns the shared hover indicator.
        /// </summary>
        public void SetHoverIndicator(
            TurnOrderHoverIndicator hoverIndicator)
        {
            _hoverIndicator =
                hoverIndicator;

            if (_isPointerOver &&
                _character != null)
            {
                _hoverIndicator?.Show(
                    _character);
            }
        }

        /// <summary>
        /// Configures the segment to represent a character action.
        /// </summary>
        public void SetupCharacter(
            CControl character,
            bool isLaunch,
            bool isCurrent)
        {
            _setupVersion++;

            var setupVersion =
                _setupVersion;

            _character =
                character;

            if (_endVisual != null)
                _endVisual.SetActive(false);

            if (_portrait != null)
            {
                _portrait.gameObject.SetActive(true);
            }

            if (_actionText != null)
            {
                _actionText.text =
                    isLaunch
                        ? "L"
                        : "A";
            }

            SetCurrent(isCurrent);

            ResetPortrait();

            if (_isPointerOver &&
                _hoverIndicator != null)
            {
                _hoverIndicator.Show(
                    _character);
            }

            if (_character == null)
                return;

            ApplyPortrait(_character);
        }

        /// <summary>
        /// Configures the segment as the final END marker.
        /// </summary>
        public void SetupEnd()
        {
            _setupVersion++;

            _character =
                null;

            SetCurrent(false);

            if (_portrait != null)
            {
                _portrait.sprite = null;
                _portrait.enabled = false;
                _portrait.gameObject.SetActive(false);
            }

            if (_actionText != null)
            {
                _actionText.text =
                    string.Empty;
            }

            if (_endVisual != null)
            {
                _endVisual.SetActive(true);
            }

            if (_isPointerOver &&
                _hoverIndicator != null)
            {
                _hoverIndicator.Hide();
            }
        }

        public void SetCurrent(
            bool isCurrent)
        {
            if (_currentHighlight != null)
            {
                _currentHighlight.enabled =
                    isCurrent;
            }
        }

        public void OnPointerEnter(
            PointerEventData eventData)
        {
            _isPointerOver =
                true;

            if (_character == null)
                return;

            _hoverIndicator?.Show(
                _character);
        }

        public void OnPointerExit(
            PointerEventData eventData)
        {
            _isPointerOver =
                false;

            _hoverIndicator?.Hide();
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

        private void ApplyPortrait(CControl character)
        {
            var sprite = character?.profile?.CharacterSettings?.portrait;
            if (sprite == null || _portrait == null)
                return;

            _portrait.sprite = sprite;
            _portrait.enabled = true;
        }

        /// <summary>
        /// Moves the camera to the character represented by this portrait.
        /// </summary>
        public void OnPortraitClicked()
        {
            if (_character == null)
                return;

            var cameraManager =
                GameplayCoreManager.Instance?
                    .CameraManager;

            if (cameraManager == null)
                return;

            cameraManager.MoveCameraTo(
                _character.transform.position,
                0.35f,
                false);
        }

        private void OnDestroy()
        {
            if (_portraitButton != null)
            {
                _portraitButton.onClick.RemoveListener(
                    OnPortraitClicked);
            }

            if (_isPointerOver)
            {
                _hoverIndicator?.Hide();
            }
        }
    }
}