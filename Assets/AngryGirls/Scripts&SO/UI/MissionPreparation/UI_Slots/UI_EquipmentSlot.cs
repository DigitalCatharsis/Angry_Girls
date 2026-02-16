using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Angry_Girls
{
    /// <summary>
    /// Simple UI slot for equipment popup (not pooled, not shop-related).
    /// </summary>
    public class UI_EquipmentSlot : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _statsText;
        [SerializeField] private Button _button;

        public InventoryItem Item { get; private set; }
        private System.Action<UI_EquipmentSlot, InventoryItem> _onClickCallback;

        /// <summary>
        /// Initialize slot with click callback.
        /// </summary>
        public void Initialize(System.Action<UI_EquipmentSlot, InventoryItem> onClickCallback)
        {
            _onClickCallback = onClickCallback;
            if (_button != null)
            {
                _button.onClick.AddListener(OnClicked);
            }
        }

        /// <summary>
        /// Set item to display.
        /// </summary>
        public async void SetItem(InventoryItem item)
        {
            Item = item;
            if (item == null)
            {
                Clear();
                return;
            }

            _nameText.text = item.ItemSettings.ItemName;
            _nameText.color = Color.white;

            // Format stats
            string stats = "";
            if (item.ItemSettings.DamageBonus != 0) stats += $"\nATK: +{item.ItemSettings.DamageBonus}";
            if (item.ItemSettings.HealthBonus != 0) stats += $"\nHP: +{item.ItemSettings.HealthBonus}";
            _statsText.text = stats.TrimStart('\n');

            // Load icon
            if (!string.IsNullOrEmpty(item.ItemSettings.IconReference?.AssetGUID))
            {
                var sprite = await CoreManager.Instance.AddressableAssetManager.LoadSpriteAsync(item.ItemSettings.IconReference.AssetGUID);
                if (sprite != null && _iconImage != null)
                {
                    _iconImage.sprite = sprite;
                    _iconImage.enabled = true;
                }
            }
        }

        /// <summary>
        /// Set custom label (e.g., "Unequip").
        /// </summary>
        public void SetLabel(string label)
        {
            _nameText.text = label;
            _nameText.color = Color.gray;
            _statsText.text = "";
            if (_iconImage != null) _iconImage.enabled = false;
        }

        private void Clear()
        {
            _nameText.text = "";
            _statsText.text = "";
            if (_iconImage != null) _iconImage.enabled = false;
        }

        private void OnClicked()
        {
            _onClickCallback?.Invoke(this, Item);
        }
    }
}