using Angry_Girls;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSettings : MonoBehaviour
{
    [SerializeField] private LevelSettings_Data _data;

    private void Start()
    {
        GameLoader.Instance.audioManager.PlayCustomSound(_data.audioSourceType, _data.audioClipEllementIndex);
        //GameLoader.Instance.audioManager.PlayRandomSound(_data.audioSourceType);
    }
}
