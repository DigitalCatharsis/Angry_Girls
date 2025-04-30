using System.Collections.Generic;
using System;
using UnityEngine;

namespace Angry_Girls
{
    //gameloader comp
    public class LevelSettings : MonoBehaviour
    {
        [SerializeField] private LevelSettings_Data _data;

        private void Start()
        {
            GameLoader.Instance.audioManager.PlayCustomSound(_data.audioSourceType, _data.audioClipEllementIndex);
        }

        private void SetInitialStage()
        {
            throw new NotImplementedException();
        }
    }
}