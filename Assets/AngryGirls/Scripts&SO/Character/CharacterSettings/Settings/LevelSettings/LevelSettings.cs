using Angry_Girls;
using UnityEngine;

//gameloader comp
public class LevelSettings : MonoBehaviour
{
    [SerializeField] private LevelSettings_Data _data;

    private void Start()
    {
        GameLoader.Instance.audioManager.PlayCustomSound(_data.audioSourceType, _data.audioClipEllementIndex);
    }
}
