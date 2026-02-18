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
        PRE_Projectile_Downsmash,
        PRE_Projectile_FireBall,
        PRE_Projectile_Uppercut,
        PRE_Projectile_Eclipse,
        PRE_VFX_DamageNumbers,
        PRE_VFX_CoinValue,
    }

    /// <summary>
    /// Factory for creating VFX game objects
    /// </summary>
    public class VFXFactory : BaseFactory<VFX_Type>
    {
        /// <inheritdoc/>
        protected override Dictionary<VFX_Type, Func<GameObject>> Prefabs => new Dictionary<VFX_Type, Func<GameObject>>
        {
            { VFX_Type.PRE_Projectile_Downsmash, () => Resources.Load(VFX_Type.PRE_Projectile_Downsmash.ToString()) as GameObject },
            { VFX_Type.PRE_Projectile_FireBall, () => Resources.Load(VFX_Type.PRE_Projectile_FireBall.ToString()) as GameObject },
            { VFX_Type.PRE_Projectile_Uppercut, () => Resources.Load(VFX_Type.PRE_Projectile_Uppercut.ToString()) as GameObject },
            { VFX_Type.PRE_VFX_DamageNumbers, () => Resources.Load(VFX_Type.PRE_VFX_DamageNumbers.ToString()) as GameObject },
            { VFX_Type.PRE_Projectile_Eclipse, () => Resources.Load(VFX_Type.PRE_Projectile_Eclipse.ToString()) as GameObject },
            { VFX_Type.PRE_VFX_CoinValue, () => Resources.Load(VFX_Type.PRE_VFX_CoinValue.ToString()) as GameObject }
        };
    }
}