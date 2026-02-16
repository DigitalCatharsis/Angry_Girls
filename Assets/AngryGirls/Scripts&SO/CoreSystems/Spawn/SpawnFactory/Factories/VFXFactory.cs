using System.Collections.Generic;
using System;
using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Enum for VFX types
    /// </summary>
    public enum VFX_Type
    {
        None = 0,
        VFX_Downsmash,
        VFX_FireBall,
        VFX_Uppercut,
        VFX_DamageNumbers,
        VFX_Eclipse,
        VFX_CoinValue,
    }

    /// <summary>
    /// Factory for creating VFX game objects
    /// </summary>
    public class VFXFactory : BaseFactory<VFX_Type>
    {
        /// <inheritdoc/>
        protected override Dictionary<VFX_Type, Func<GameObject>> Prefabs => new Dictionary<VFX_Type, Func<GameObject>>
        {
            { VFX_Type.VFX_Downsmash, () => Resources.Load(VFX_Type.VFX_Downsmash.ToString()) as GameObject },
            { VFX_Type.VFX_FireBall, () => Resources.Load(VFX_Type.VFX_FireBall.ToString()) as GameObject },
            { VFX_Type.VFX_Uppercut, () => Resources.Load(VFX_Type.VFX_Uppercut.ToString()) as GameObject },
            { VFX_Type.VFX_DamageNumbers, () => Resources.Load(VFX_Type.VFX_DamageNumbers.ToString()) as GameObject },
            { VFX_Type.VFX_Eclipse, () => Resources.Load(VFX_Type.VFX_Eclipse.ToString()) as GameObject },
            { VFX_Type.VFX_CoinValue, () => Resources.Load(VFX_Type.VFX_CoinValue.ToString()) as GameObject }
        };
    }
}