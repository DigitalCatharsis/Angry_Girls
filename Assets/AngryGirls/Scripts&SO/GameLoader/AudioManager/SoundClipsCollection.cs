using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Angry_Girls
{
    [CreateAssetMenu(fileName = "SoundClipsCollection", menuName = "Angry_Girls/Audio/SoundClipsCollection" +
        "")]
    public class SoundClipsCollection : ScriptableObject
    {
        public List<AudioClip> audioClips;
    }
}