using System;
using System.Collections.Generic;
using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Enum for character types
    /// </summary>
    [Serializable]
    public enum CharacterType
    {
        NULL = 0,
        Player_YBot_Ground_Original = 1,
        Player_YBot_AirToGround_Red = 2,
        Player_YBot_Air_Green = 3,
        Player_YBot_AirToGround_Yellow = 4,
        Player_YBot_Ground_Blue = 5,
        Enemy_YBot_Ground_Blue = 8,
        Enemy_YBot_Air_Green = 6,
        Enemy_YBot_AirToGround_Red = 7,
        Enemy_YBot_AirToGround_Yellow = 9,
    }

    /// <summary>
    /// Factory for creating character game objects
    /// </summary>
    public class CharacterFactory : BaseFactory<CharacterType>
    {
        #region Prefab
        /// <inheritdoc/>
        protected override Dictionary<CharacterType, Func<GameObject>> Prefabs => new Dictionary<CharacterType, Func<GameObject>>
        {
            { CharacterType.Player_YBot_Ground_Original, () => Resources.Load(CharacterType.Player_YBot_Ground_Original.ToString()) as GameObject },
            { CharacterType.Player_YBot_AirToGround_Yellow, () => Resources.Load(CharacterType.Player_YBot_AirToGround_Yellow.ToString()) as GameObject },
            { CharacterType.Player_YBot_Air_Green, () => Resources.Load(CharacterType.Player_YBot_Air_Green.ToString()) as GameObject },
            { CharacterType.Player_YBot_AirToGround_Red, () => Resources.Load(CharacterType.Player_YBot_AirToGround_Red.ToString()) as GameObject },
            { CharacterType.Player_YBot_Ground_Blue, () => Resources.Load(CharacterType.Player_YBot_Ground_Blue.ToString()) as GameObject },
            { CharacterType.Enemy_YBot_Air_Green, () => Resources.Load(CharacterType.Enemy_YBot_Air_Green.ToString()) as GameObject },
            { CharacterType.Enemy_YBot_AirToGround_Red, () => Resources.Load(CharacterType.Enemy_YBot_AirToGround_Red.ToString()) as GameObject },
            { CharacterType.Enemy_YBot_Ground_Blue, () => Resources.Load(CharacterType.Enemy_YBot_Ground_Blue.ToString()) as GameObject },
            { CharacterType.Enemy_YBot_AirToGround_Yellow, () => Resources.Load(CharacterType.Enemy_YBot_AirToGround_Yellow.ToString()) as GameObject },
        };

        #endregion
    }
}