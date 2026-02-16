using System;
using System.Collections.Generic;
using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Enum for UI object types
    /// </summary>
    [Serializable]
    public enum UIObjectType
    {
        NULL = 0,
        PRE_ItemSlot = 1,
    }

    /// <summary>
    /// Factory for creating UI game objects
    /// </summary>
    public class UIObjectsFactory : BaseFactory<UIObjectType>
    {
        /// <inheritdoc/>
        protected override Dictionary<UIObjectType, Func<GameObject>> Prefabs => new Dictionary<UIObjectType, Func<GameObject>>
        {
            { UIObjectType.PRE_ItemSlot, () => Resources.Load(UIObjectType.PRE_ItemSlot.ToString()) as GameObject },
        };
    }
}