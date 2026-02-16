using System.Collections.Generic;
using UnityEngine;

namespace Angry_Girls
{
    /// <summary>
    /// Collection of audio clips for specific sound category
    /// </summary>
    [CreateAssetMenu(fileName = "SoundClipsCollection", menuName = "Angry_Girls/Audio/SoundClipsCollection")]
    public class SoundClipsCollection : ScriptableObject
    {
        public List<AudioClip> audioClips;
    }
}