using System.Collections.Generic;
using System;
using UnityEngine;

namespace Angry_Girls
{
    public enum VFX_Type
    {
        VFX_Damage_White,
        VFX_Uppercut,
        VFX_FireBall,
        VFX_Flame,
        VFX_Flame2,
        VFX_TestOnHitEffect,
        VFX_Eclipse,
        VFX_CoinValue,
    }



    public class VFXFactory : BaseFactory<VFX_Type>
    {
        protected override Dictionary<VFX_Type, Func<GameObject>> Prefabs => new Dictionary<VFX_Type, Func<GameObject>>
        {
            { VFX_Type.VFX_Damage_White, () => Resources.Load(VFX_Type.VFX_Damage_White.ToString()) as GameObject },
            { VFX_Type.VFX_Uppercut, () => Resources.Load(VFX_Type.VFX_Uppercut.ToString()) as GameObject },
            { VFX_Type.VFX_FireBall, () => Resources.Load(VFX_Type.VFX_FireBall.ToString()) as GameObject },
            { VFX_Type.VFX_Flame, () => Resources.Load(VFX_Type.VFX_Flame.ToString()) as GameObject },
            { VFX_Type.VFX_Flame2, () => Resources.Load(VFX_Type.VFX_Flame2.ToString()) as GameObject },
            { VFX_Type.VFX_TestOnHitEffect, () => Resources.Load(VFX_Type.VFX_TestOnHitEffect.ToString()) as GameObject },
            { VFX_Type.VFX_Eclipse, () => Resources.Load(VFX_Type.VFX_Eclipse.ToString()) as GameObject },
            { VFX_Type.VFX_CoinValue, () => Resources.Load(VFX_Type.VFX_CoinValue.ToString()) as GameObject }
        };
    }
}
