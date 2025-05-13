using System.Collections.Generic;
using System;
using UnityEngine;

namespace Angry_Girls
{
    public enum CharacterType
    {
        NULL,
        YBot_Original,
        YBot_Red,
        YBot_Green,
        YBot_Yellow,
        YBot_Blue,
        Enemy_Air_Green_Variant,
        Enemy_AirToGround_Red_Variant,
        Enemy_Ground_Blue_Variant,
        Enemyt_AirToGround_Yellow_Variant
    }

    public class CharacterFactory : BaseFactory<CharacterType>
    {
        protected override Dictionary<CharacterType, Func<GameObject>> Prefabs => new Dictionary<CharacterType, Func<GameObject>>
        {
            { CharacterType.YBot_Original, () => Resources.Load(CharacterType.YBot_Original.ToString()) as GameObject },
            { CharacterType.YBot_Yellow, () => Resources.Load(CharacterType.YBot_Yellow.ToString()) as GameObject },
            { CharacterType.YBot_Green, () => Resources.Load(CharacterType.YBot_Green.ToString()) as GameObject },
            { CharacterType.YBot_Red, () => Resources.Load(CharacterType.YBot_Red.ToString()) as GameObject },
            { CharacterType.YBot_Blue, () => Resources.Load(CharacterType.YBot_Blue.ToString()) as GameObject },
            { CharacterType.Enemy_Air_Green_Variant, () => Resources.Load(CharacterType.Enemy_Air_Green_Variant.ToString()) as GameObject },
            { CharacterType.Enemy_AirToGround_Red_Variant, () => Resources.Load(CharacterType.Enemy_AirToGround_Red_Variant.ToString()) as GameObject },
            { CharacterType.Enemy_Ground_Blue_Variant, () => Resources.Load(CharacterType.Enemy_Ground_Blue_Variant.ToString()) as GameObject },
            { CharacterType.Enemyt_AirToGround_Yellow_Variant, () => Resources.Load(CharacterType.Enemyt_AirToGround_Yellow_Variant.ToString()) as GameObject },
        };
    }
}
