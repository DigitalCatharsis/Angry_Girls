using UnityEngine;

namespace Angry_Girls
{
    [CreateAssetMenu(fileName = "ShopSettings", menuName = "Angry_Girls/Shop Settings")]
    public class ShopSettings : ScriptableObject
    {
        [Header("Refresh Settings")]
        [Tooltip("Cost to manually refresh the shop assortment")]
        public int ManualRefreshCost = 100;

        [Tooltip("Allow players to manually refresh the shop?")]
        public bool AllowManualRefresh = true;

        [Header("Item Display Counts")]
        [Tooltip("Number of items displayed from the Easy collection")]
        public int EasyCollectionItemCount = 6;

        [Tooltip("Number of items displayed from the Normal collection when progress is 25-49%")]
        public int NormalCollectionItemCountLow = 3;

        [Tooltip("Number of items displayed from the Normal collection when progress is >=50%")]
        public int NormalCollectionItemCountHigh = 6;

        [Tooltip("Number of items displayed from the Hard collection when progress is 50-99%")]
        public int HardCollectionItemCountLow = 1;

        [Tooltip("Number of items displayed from the Hard collection when progress is 100%")]
        public int HardCollectionItemCountHigh = 3;
    }
}