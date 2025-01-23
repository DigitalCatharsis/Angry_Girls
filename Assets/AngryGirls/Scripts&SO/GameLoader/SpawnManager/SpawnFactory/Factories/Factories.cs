using System.Collections.Generic;
using System;
using UnityEngine;

namespace Angry_Girls
{
    public enum CharacterType
    {
        YBot_Original,
        YBot_Red,
        YBot_Green,
        YBot_Yellow,
        YBot_Blue,
    }
    public enum VFX_Type
    {
        VFX_Damage_White,
        VFX_Shouryken,
        VFX_FireBall,
        VFX_Flame,
        VFX_Flame2,
        VFX_TestOnHitEffect,
        VFX_Eclipse,
    }

    public class CharacterFactory : BaseFactory<CharacterType>
    {
        protected override Dictionary<CharacterType, Func<GameObject>> Prefabs => new Dictionary<CharacterType, Func<GameObject>>
        {
            { CharacterType.YBot_Original, () => Resources.Load(CharacterType.YBot_Original.ToString()) as GameObject },
            { CharacterType.YBot_Yellow, () => Resources.Load(CharacterType.YBot_Yellow.ToString()) as GameObject },
            { CharacterType.YBot_Green, () => Resources.Load(CharacterType.YBot_Green.ToString()) as GameObject },
            { CharacterType.YBot_Red, () => Resources.Load(CharacterType.YBot_Red.ToString()) as GameObject },
            { CharacterType.YBot_Blue, () => Resources.Load(CharacterType.YBot_Blue.ToString()) as GameObject }
        };
    }

    public class VFXFactory : BaseFactory<VFX_Type>
    {
        protected override Dictionary<VFX_Type, Func<GameObject>> Prefabs => new Dictionary<VFX_Type, Func<GameObject>>
        {
            { VFX_Type.VFX_Damage_White, () => Resources.Load(VFX_Type.VFX_Damage_White.ToString()) as GameObject },
            { VFX_Type.VFX_Shouryken, () => Resources.Load(VFX_Type.VFX_Shouryken.ToString()) as GameObject },
            { VFX_Type.VFX_FireBall, () => Resources.Load(VFX_Type.VFX_FireBall.ToString()) as GameObject },
            { VFX_Type.VFX_Flame, () => Resources.Load(VFX_Type.VFX_Flame.ToString()) as GameObject },
            { VFX_Type.VFX_Flame2, () => Resources.Load(VFX_Type.VFX_Flame2.ToString()) as GameObject },
            { VFX_Type.VFX_TestOnHitEffect, () => Resources.Load(VFX_Type.VFX_TestOnHitEffect.ToString()) as GameObject },
            { VFX_Type.VFX_Eclipse, () => Resources.Load(VFX_Type.VFX_Eclipse.ToString()) as GameObject }
        };
    }
}
