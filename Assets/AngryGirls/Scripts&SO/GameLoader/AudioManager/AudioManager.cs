using UnityEngine;

namespace Angry_Girls
{
    public enum AudioSourceType
    {
        CharacterHit,
    }

    public class AudioManager : MonoBehaviour
    {
        [Header("Sound Collections")]
        [SerializeField] private SoundClipsCollection _characterHitSounds;

        [Header("AudioSources")]
        [SerializeField] private AudioSource _levelMusicAudioSource;
        [SerializeField] private AudioSource _characterHitAudioSource;

        public void PlayRandomSound(AudioSourceType type, bool randomPitch = false)
        {
            switch (type)
            {
                case AudioSourceType.CharacterHit:
                    {
                        if (randomPitch)
                        {
                            _characterHitAudioSource.pitch = (Random.Range(0.6f, 0.9f));
                        }
                        _characterHitAudioSource.PlayOneShot(GetRandomAudioClip(_characterHitSounds), _characterHitAudioSource.volume);
                        return;
                    }
            }
        }

        public void PlayCustomSound(AudioSourceType type, int index = 0, bool randomPitch = false)
        {
            switch (type)
            {
                case AudioSourceType.CharacterHit:
                    {
                        if (randomPitch)
                        {
                            _characterHitAudioSource.pitch = (Random.Range(0.6f, 0.9f));
                        }
                        _characterHitAudioSource.clip = GetCusomAudioClip(_characterHitSounds, index);
                        _characterHitAudioSource.Play();
                        //audioSource.PlayOneShot ?
                        return;
                    }
            }
        }

        private AudioClip GetRandomAudioClip(SoundClipsCollection clipsCollection)
        {
            var randomIndex = Random.Range(0, (clipsCollection.audioClips.Count - 1));
            var randomValue = clipsCollection.audioClips[randomIndex];
            return randomValue;
        }
        private AudioClip GetCusomAudioClip(SoundClipsCollection clipsCollection, int index)
        {
            var value = clipsCollection.audioClips[index];
            return value;
        }
    }
}