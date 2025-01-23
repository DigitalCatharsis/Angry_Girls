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
}
