using UnityEngine;

namespace AudioSystem
{
    public struct SoundBuilder
    {
        internal AudioClip _clip;
        internal SoundSettings _settings;
        internal Transform _target;
        internal Vector3 _position;
        internal float _volume;
        internal float _pitch;
        internal float _randomPitch;
        internal float _duration;
        internal float _fadeIn;
        internal SoundConfiguration _configuration;
        internal bool _hasConfiguration;
        internal bool _hasPosition;
        internal bool _hasVolume;
        internal bool _hasPitch;
        internal bool _voice;
        internal bool _loop;
        internal bool _frequent;
        internal bool _music;

        internal SoundBuilder(AudioClip clip)
        {
            this = default;
            _clip = clip;
        }

        public SoundBuilder Voice()
        {
            _voice = true;
            return this;
        }

        public SoundBuilder At(Vector3 position)
        {
            _position = position;
            _hasPosition = true;
            return this;
        }

        public SoundBuilder Follow(Transform target)
        {
            _target = target;
            return this;
        }

        public SoundBuilder Volume(float volume)
        {
            _volume = volume;
            _hasVolume = true;
            return this;
        }

        public SoundBuilder Pitch(float pitch)
        {
            _pitch = pitch;
            _hasPitch = true;
            return this;
        }

        public SoundBuilder RandomPitch(float range = 0.05f)
        {
            _randomPitch = range;
            return this;
        }

        public SoundBuilder Duration(float seconds)
        {
            _duration = seconds;
            return this;
        }

        public SoundBuilder FadeIn(float seconds)
        {
            _fadeIn = seconds;
            return this;
        }

        public SoundBuilder Loop(bool loop = true)
        {
            _loop = loop;
            return this;
        }

        public SoundBuilder Frequent(bool frequent = true)
        {
            _frequent = frequent;
            return this;
        }

        public SoundBuilder Config(SoundConfiguration configuration)
        {
            _configuration = configuration;
            _hasConfiguration = true;
            return this;
        }

        public SoundBuilder Settings(SoundSettings settings)
        {
            _settings = settings;
            return this;
        }

        public SoundHandle Play()
        {
            return AudioController.Play(in this);
        }

        internal readonly SoundConfiguration ResolveConfiguration(bool follow)
        {
            if (_hasConfiguration) return _configuration;

            bool spatial = follow || _hasPosition;
            if (_voice) return spatial ? SoundConfiguration.Voice3D : SoundConfiguration.Voice2D;
            return spatial ? SoundConfiguration.Effect3D : SoundConfiguration.Effect2D;
        }
    }
}
