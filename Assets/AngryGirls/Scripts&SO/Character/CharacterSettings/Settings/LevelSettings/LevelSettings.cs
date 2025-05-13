using System.Collections.Generic;
using System;
using UnityEngine;

namespace Angry_Girls
{
    //gameloader comp
    public class LevelSettings : MonoBehaviour
    {
        [SerializeField] private LevelSettings_Data _levelData;
        public PlayerData playerData;

        private void Start()
        {
            GameLoader.Instance.audioManager.PlayCustomSound(_levelData.audioSourceType, _levelData.audioClipEllementIndex);
        }
    }
}