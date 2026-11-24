using UnityEngine;

namespace AudioSystem
{
    [AddComponentMenu("")]
    [RequireComponent(typeof(AudioSource))]
    internal sealed class SoundEmitter : MonoBehaviour
    {
        private const float LoopPollInterval = 0.5f;
        private const float EndPollInterval = 0.02f;
        private const float MaxFadeStep = 0.1f;

        internal EmitterPool Pool;
        internal SoundEmitter FrequentPrev;
        internal SoundEmitter FrequentNext;

        private AudioSource _source;
        private Transform _transform;
        private AudioClip _clip;
        private SoundSettings _settings;

        internal Transform Target;
        internal float PollIn;
        internal float Remaining;
        internal bool Timed;
        internal bool Fading;
        internal bool IgnoreListenerPause;

        internal int Generation;
        internal int ActiveIndex = -1;
        internal int SettingsHash;
        internal bool Frequent;

        private float _clipLength;
        private float _volumeScale = 1f;
        private float _volume = -1f;
        private float _pitch = float.NaN;
        private float _fade = 1f;
        private float _fadeGain = 1f;
        private float _fadeTarget;
        private float _fadeSpeed;
        private bool _looping;
        private bool _stopAfterFade;

        internal static SoundEmitter CreateTemplate(SoundSettings settings, int hash, Transform parent)
        {
            SoundEmitter template = new GameObject(nameof(SoundEmitter)).AddComponent<SoundEmitter>();
            template.Bind();
            template._transform.SetParent(parent, false);
            template._source.playOnAwake = false;
            template.ApplySettings(settings, hash);
            template._source.enabled = false;
            return template;
        }

        internal SoundEmitter Clone(Transform parent)
        {
            SoundEmitter clone = Instantiate(this, parent);
            clone.Bind();
            clone._settings = _settings;
            clone.SettingsHash = SettingsHash;
            clone.ApplyListenerFlags(_settings);
            return clone;
        }

        internal void Wake()
        {
            _source.enabled = true;
        }

        internal void Sleep()
        {
            if (!ReferenceEquals(_clip, null))
            {
                _clip = null;
                _source.clip = null;
            }

            _source.enabled = false;
        }

        internal void Begin(in SoundBuilder builder, AudioClip clip, SoundSettings settings, Transform target)
        {
            int hash = settings.ComputeHash();
            if (!ReferenceEquals(_settings, settings) || SettingsHash != hash)
                ApplySettings(settings, hash);

            if (!ReferenceEquals(_clip, clip))
            {
                _clip = clip;
                _source.clip = clip;
                _clipLength = clip.length;
            }

            if (builder._fadeIn > 0f)
            {
                SetFade(0f);
                StartFade(1f, builder._fadeIn, false);
            }
            else
            {
                _fade = 1f;
                _fadeGain = 1f;
                Fading = false;
            }

            float range = builder._randomPitch;
            SetVolume(builder._hasVolume ? builder._volume : 1f);
            ApplyPitch(builder._hasPitch ? builder._pitch : 1f, range > 0f ? Random.Range(-range, range) : 0f);

            bool loop = builder._loop || settings.loop;
            if (_looping != loop)
            {
                _looping = loop;
                _source.loop = loop;
            }

            PollIn = loop ? LoopPollInterval : ExpectedLength();

            Target = target;
            if (!ReferenceEquals(target, null))
                _transform.position = target.position;
            else if (builder._hasPosition)
                _transform.position = builder._position;
            else if (settings.spatialBlend > 0f)
                _transform.localPosition = Vector3.zero;

            Timed = builder._duration > 0f;
            Remaining = builder._duration;

            _source.Play();
        }

        internal void End(bool stop)
        {
            if (stop) _source.Stop();

            Generation++;
            Timed = false;
            Fading = false;
            Target = null;
        }

        internal bool UpdateFade(float deltaTime)
        {
            float step = _fadeSpeed * (deltaTime < MaxFadeStep ? deltaTime : MaxFadeStep);
            float fade = _fade < _fadeTarget ? Mathf.Min(_fade + step, _fadeTarget) : Mathf.Max(_fade - step, _fadeTarget);
            SetFade(fade);
            ApplyVolume();

            if (fade != _fadeTarget) return false;

            Fading = false;
            return _stopAfterFade;
        }

        internal void Follow()
        {
            if (Target == null)
            {
                Target = null;
                return;
            }

            _transform.position = Target.position;
        }

        internal bool HasEnded()
        {
            if (_source.isPlaying)
            {
                PollIn = _looping ? LoopPollInterval : EndPollInterval;
                return false;
            }

            return _clip == null || _clip.loadState != AudioDataLoadState.Loading;
        }

        internal void SetVolume(float volume)
        {
            _volumeScale = volume;
            ApplyVolume();
        }

        internal void SetPitch(float pitch)
        {
            ApplyPitch(pitch, 0f);
            PollIn = 0f;
        }

        internal void FadeOut(float seconds)
        {
            StartFade(0f, seconds, true);
        }

        internal void ApplySettings(SoundSettings settings, int hash)
        {
            _settings = settings;
            SettingsHash = hash;

            AudioSource source = _source;
            source.outputAudioMixerGroup = settings.mixerGroup;

            source.mute = settings.mute;
            source.bypassEffects = settings.bypassEffects;
            source.bypassListenerEffects = settings.bypassListenerEffects;
            source.bypassReverbZones = settings.bypassReverbZones;

            source.priority = settings.priority;
            source.panStereo = settings.panStereo;
            source.spatialBlend = settings.spatialBlend;
            source.reverbZoneMix = settings.reverbZoneMix;
            source.dopplerLevel = settings.dopplerLevel;
            source.spread = settings.spread;

            source.minDistance = settings.minDistance;
            source.maxDistance = settings.maxDistance;

            source.rolloffMode = settings.rolloffMode;

            ApplyListenerFlags(settings);
        }

        private void Bind()
        {
            _source = GetComponent<AudioSource>();
            _transform = transform;
        }

        private void ApplyListenerFlags(SoundSettings settings)
        {
            IgnoreListenerPause = settings.ignoreListenerPause;
            _source.ignoreListenerVolume = settings.ignoreListenerVolume;
            _source.ignoreListenerPause = settings.ignoreListenerPause;
        }

        private void ApplyVolume()
        {
            float volume = _volumeScale * _settings.volume * _fadeGain;
            if (_volume == volume) return;

            _volume = volume;
            _source.volume = volume;
        }

        private void ApplyPitch(float pitch, float offset)
        {
            pitch = pitch * _settings.pitch + offset;
            if (_pitch == pitch) return;

            _pitch = pitch;
            _source.pitch = pitch;
        }

        private void StartFade(float target, float seconds, bool stopAtEnd)
        {
            _fadeTarget = target;
            _fadeSpeed = 1f / seconds;
            _stopAfterFade = stopAtEnd;
            Fading = true;
        }

        private void SetFade(float fade)
        {
            _fade = fade;
            _fadeGain = Mathf.Sqrt(fade);
        }

        private float ExpectedLength()
        {
            float speed = _pitch < 0f ? -_pitch : _pitch;
            return speed > 0.01f ? _clipLength / speed : 0f;
        }
    }
}
