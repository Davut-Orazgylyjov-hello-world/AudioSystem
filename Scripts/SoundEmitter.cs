using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace AudioSystem
{
    [RequireComponent(typeof(AudioSource))]
    public class SoundEmitter : MonoBehaviour
    {
        public SoundData Data { get; private set; }
        public LinkedListNode<SoundEmitter> Node { get; set; }

        private AudioSource audioSource;
        private Coroutine playingCoroutine;

        private void Awake()
        {
            audioSource = gameObject.GetOrAdd<AudioSource>();
        }

        public void Initialize(SoundData data)
        {
            Data = data;
            audioSource.clip = data.audioClip;
            audioSource.outputAudioMixerGroup = data.soundSettings.mixerGroup;
            audioSource.loop = data.soundSettings.loop;
            audioSource.playOnAwake = data.soundSettings.playOnAwake;

            audioSource.mute = data.soundSettings.mute;
            audioSource.bypassEffects = data.soundSettings.bypassEffects;
            audioSource.bypassListenerEffects = data.soundSettings.bypassListenerEffects;
            audioSource.bypassReverbZones = data.soundSettings.bypassReverbZones;

            audioSource.priority = data.soundSettings.priority;
            audioSource.volume = data.soundSettings.volume;
            audioSource.pitch = data.soundSettings.pitch;
            audioSource.panStereo = data.soundSettings.panStereo;
            audioSource.spatialBlend = data.soundSettings.spatialBlend;
            audioSource.reverbZoneMix = data.soundSettings.reverbZoneMix;
            audioSource.dopplerLevel = data.soundSettings.dopplerLevel;
            audioSource.spread = data.soundSettings.spread;

            audioSource.minDistance = data.soundSettings.minDistance;
            audioSource.maxDistance = data.soundSettings.maxDistance;

            audioSource.ignoreListenerVolume = data.soundSettings.ignoreListenerVolume;
            audioSource.ignoreListenerPause = data.soundSettings.ignoreListenerPause;

            audioSource.rolloffMode = data.soundSettings.rolloffMode;
        }


        private bool _timePlay = false;
        private float _timePlayerSeconds;

        public void WithTimePlay(float seconds)
        {
            _timePlay = true;
            _timePlayerSeconds = seconds;
        }

        public void Play()
        {
            if (playingCoroutine != null)
            {
                StopCoroutine(playingCoroutine);
            }

            audioSource.Play();

            if (_timePlay)
                playingCoroutine = StartCoroutine(WaitForTimePlay(_timePlayerSeconds));
            else
                playingCoroutine = StartCoroutine(WaitForSoundToEnd());
        }

        private IEnumerator WaitForSoundToEnd()
        {
            yield return new WaitWhile(() => audioSource.isPlaying);
            Stop();
        }

        private IEnumerator WaitForTimePlay(float seconds)
        {
            yield return new WaitForSeconds(seconds);
            Stop();
        }

        public void Stop()
        {
            if (playingCoroutine != null)
            {
                StopCoroutine(playingCoroutine);
                playingCoroutine = null;
            }

            audioSource.Stop();
            AudioController.ReturnToPool(this);
        }

        public void WithRandomPitch(float min = -0.05f, float max = 0.05f)
        {
            audioSource.pitch += Random.Range(min, max);
        }

        public void WithVolume(float volume)
        {
            audioSource.volume = volume;
        }
    }
}