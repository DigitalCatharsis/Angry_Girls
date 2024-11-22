using AYellowpaper.SerializedCollections;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Angry_Girls
{
    public enum AudioSourceType
    {
        CharacterHit,
        LevelMusic,
    }

    public class AudioManager : MonoBehaviour
    {
        [SerializeField] private SerializedDictionary<AudioSourceType, SoundData> soundDataDict = new();

        public void PlayRandomSound(AudioSourceType type, bool randomPitch = false)
        {   
            var audiosource = soundDataDict[type].audioSorce;
            var audioclipCollection = soundDataDict[type].audioClipCollection;

            if (randomPitch)
            {
                audiosource.pitch = (UnityEngine.Random.Range(0.6f, 0.9f));
            }
            audiosource.PlayOneShot(GetRandomAudioClip(audioclipCollection), audiosource.volume);
        }

        public void PlayCustomSound(AudioSourceType type, int index = 0, bool randomPitch = false)
        {
            var audiosource = soundDataDict[type].audioSorce;
            var audioclipCollection = soundDataDict[type].audioClipCollection;

            if (randomPitch)
            {
                audiosource.pitch = (UnityEngine.Random.Range(0.6f, 0.9f));
            }
            audiosource.PlayOneShot(audioclipCollection.audioClips[index], audiosource.volume);
            //audioSource.PlayOneShot ?
        }

        //public void PlayCustomSound(AudioClip audioClip, AudioSourceType type)
        //{

        //    _characterHitAudioSource.Play();
        //}

        private AudioClip GetRandomAudioClip(SoundClipsCollection clipsCollection)
        {
            var randomIndex = UnityEngine.Random.Range(0, (clipsCollection.audioClips.Count));
            var randomValue = clipsCollection.audioClips[randomIndex];
            return randomValue;
        }

        private AudioClip GetCusomAudioClip(SoundClipsCollection clipsCollection, int index)
        {
            var value = clipsCollection.audioClips[index];
            return value;
        }
    }

    [Serializable]
    public class SoundData
    {
        public AudioSource audioSorce;
        public SoundClipsCollection audioClipCollection;
    }
}