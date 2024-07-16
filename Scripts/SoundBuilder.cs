using UnityEngine;

namespace AudioSystem
{
    public class SoundBuilder
    {
        private Vector3 _position = Vector3.zero;
        private bool _randomPitch;
        private float _volume = 1f;
        private float _timePlay;


        public SoundBuilder WithPosition(Vector3 position)
        {
            _position = position;
            return this;
        }

        public SoundBuilder WithRandomPitch()
        {
            _randomPitch = true;
            return this;
        }

        public SoundBuilder WithVolume(float volume)
        {
            _volume = volume;
            return this;
        }

        public SoundBuilder WithTimePlay(float seconds)
        {
            _timePlay = seconds;
            return this;
        }

        public void Play(AudioClip audioClip, SoundConfiguration soundConfiguration)
        {
            Play(new SoundData(audioClip, AudioController.GetSoundSettings(soundConfiguration)));
        }

        public void Play(SoundData soundData)
        {
            if (soundData == null)
            {
                Debug.LogError("SoundData is null");
                return;
            }

            if (!AudioController.CanPlaySound(soundData)) return;

            SoundEmitter soundEmitter = AudioController.Get();
            soundEmitter.Initialize(soundData);
            soundEmitter.transform.position = _position;
            soundEmitter.transform.parent = AudioController.Transform;

            soundEmitter.WithVolume(_volume);
            if (_timePlay != 0)
                soundEmitter.WithTimePlay(_timePlay);

            if (_randomPitch)
            {
                soundEmitter.WithRandomPitch();
            }

            if (soundData.soundSettings.frequentSound)
            {
                soundEmitter.Node = AudioController.FrequentSoundEmitters.AddLast(soundEmitter);
            }

            soundEmitter.Play();
        }
    }
}