using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Gazeus.DesafioMatch3
{
    public class AudioController : MonoBehaviour
    {
        // Audio sources
        [SerializeField] AudioSource _audioMusicSource;

        // Music
        [SerializeField] List<AudioClip> _gameSoundtrack;

        private void Start()
        {
            StartCoroutine(PlaySoundtrack());
        }

        public void PlayAudio(AudioClip audioClip) 
        {
            _audioMusicSource.PlayOneShot(audioClip);
        }

        IEnumerator PlaySoundtrack() 
        {            
            _audioMusicSource.PlayOneShot(_gameSoundtrack[Random.Range(0, _gameSoundtrack.Count)]);
            yield return new WaitUntil(() => !_audioMusicSource.isPlaying);
            StartCoroutine(PlaySoundtrack());
        }
    }
}
