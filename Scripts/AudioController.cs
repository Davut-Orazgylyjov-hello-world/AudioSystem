using UnityEngine;

namespace AudioSystem
{
    [AddComponentMenu("")]
    [DefaultExecutionOrder(10000)]
    internal sealed class AudioController : MonoBehaviour
    {
        private const float IdleDisableInterval = 0.25f;
        private const int MusicReserve = 2;
        private const int PrewarmedPresets = (int)SoundConfiguration.Music;
        private const int VolumeApplyFrames = 2;
        private const int ResumeSkipFrames = 2;

        private static AudioController _instance;
        private static bool _ready;
        private static bool _quitting;

        private readonly FrequentSounds _frequent = new FrequentSounds();
        private AudioSystemSettings _settings;
        private ChannelVolumes _volumes;
        private EmitterPools _pools;
        private SoundEmitter[] _active = new SoundEmitter[64];
        private SoundHandle _music;
        private AudioClip _musicClip;
        private int _activeCount;
        private int _maxActive;
        private int _skipFrames;
        private int _applyVolumesFrames;
        private float _disableIdleIn;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private bool _limitWarned;
#endif

        private static AudioController Instance => _ready ? _instance : Create();

        internal static SoundHandle Play(in SoundBuilder builder)
        {
            AudioClip clip = builder._clip;
            if (clip == null)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Debug.LogWarning("AudioSystem: clip is null");
#endif
                return default;
            }

            AudioController controller = Instance;
            if (ReferenceEquals(controller, null)) return default;

            return controller.Play(in builder, clip);
        }

        internal static SoundHandle PlayMusic(AudioClip clip, float fadeSeconds)
        {
            if (clip == null)
            {
                StopMusic(fadeSeconds);
                return default;
            }

            AudioController controller = Instance;
            if (ReferenceEquals(controller, null)) return default;

            return controller.SwitchMusic(clip, fadeSeconds);
        }

        internal static void StopMusic(float fadeSeconds)
        {
            if (_ready) _instance.EndMusic(fadeSeconds);
        }

        internal static void SetVolume(SoundChannel channel, float volume)
        {
            AudioController controller = Instance;
            if (!ReferenceEquals(controller, null)) controller._volumes.Set(channel, volume);
        }

        internal static float GetVolume(SoundChannel channel)
        {
            AudioController controller = Instance;
            return ReferenceEquals(controller, null) ? 1f : controller._volumes.Get(channel);
        }

        internal static void Stop(SoundEmitter emitter)
        {
            if (_ready) _instance.Release(emitter, true);
        }

        internal static void StopAll()
        {
            if (_ready) _instance.ReleaseAll();
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _instance = null;
            _ready = false;
            _quitting = false;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateOnLoad()
        {
            if (!_ready) Create();
        }

        private static AudioController Create()
        {
            if (_quitting || !Application.isPlaying) return null;

            AudioSystemSettings settings = AudioSystemSettings.Load();
            if (settings == null)
            {
                Debug.LogError("AudioSystem: settings asset not found in Resources");
                return null;
            }

            GameObject host = new GameObject(nameof(AudioSystem));
            DontDestroyOnLoad(host);

            AudioController controller = host.AddComponent<AudioController>();
            controller.Initialize(settings);
            _instance = controller;
            _ready = true;
            return controller;
        }

        private void Initialize(AudioSystemSettings settings)
        {
            _settings = settings;

            int virtualVoices = AudioSettings.GetConfiguration().numVirtualVoices;
            _maxActive = virtualVoices > MusicReserve ? virtualVoices - MusicReserve : int.MaxValue;

            _volumes = new ChannelVolumes(settings.mixer, settings.volumes);
            _volumes.ApplyAll();
            _applyVolumesFrames = VolumeApplyFrames;

            _pools = new EmitterPools(transform, settings.maxPoolSize);
            _pools.Prewarm(settings.presets, PrewarmedPresets, settings.prewarmCount);
        }

        private void OnDestroy()
        {
            if (!ReferenceEquals(_instance, this)) return;

            for (int i = 0; i < _activeCount; i++)
                _active[i].Generation++;

            _instance = null;
            _ready = false;
        }

        private void OnApplicationQuit()
        {
            _quitting = true;
        }

        private void OnApplicationPause(bool paused)
        {
            _skipFrames = ResumeSkipFrames;
        }

        private void OnApplicationFocus(bool focused)
        {
            _skipFrames = ResumeSkipFrames;
        }

        private void LateUpdate()
        {
            if (_applyVolumesFrames > 0)
            {
                _applyVolumesFrames--;
                _volumes.ApplyAll();
            }

            if (_skipFrames > 0)
            {
                _skipFrames--;
                return;
            }

            float deltaTime = Time.unscaledDeltaTime;
            if (_activeCount > 0) UpdateActive(deltaTime, AudioListener.pause);

            _disableIdleIn -= deltaTime;
            if (_disableIdleIn > 0f) return;

            _disableIdleIn = IdleDisableInterval;
            _pools.DisableIdle();
        }

        private void UpdateActive(float deltaTime, bool listenerPaused)
        {
            for (int i = _activeCount - 1; i >= 0; i--)
            {
                SoundEmitter emitter = _active[i];
                if (listenerPaused && !emitter.IgnoreListenerPause) continue;

                if (emitter.Fading && emitter.UpdateFade(deltaTime))
                {
                    Release(emitter, true);
                    continue;
                }

                if (!ReferenceEquals(emitter.Target, null))
                    emitter.Follow();

                if (emitter.Timed)
                {
                    emitter.Remaining -= deltaTime;
                    if (emitter.Remaining <= 0f)
                    {
                        Release(emitter, true);
                        continue;
                    }
                }

                if (emitter.PollIn > 0f)
                {
                    emitter.PollIn -= deltaTime;
                    continue;
                }

                if (emitter.HasEnded()) Release(emitter, false);
            }
        }

        private SoundSettings FindPreset(SoundConfiguration configuration)
        {
            SoundSettings preset = _settings.GetPreset(configuration);
            if (preset == null)
            {
                Debug.LogError("AudioSystem: preset " + configuration + " is not assigned in " + _settings.name);
                return null;
            }

            return preset;
        }

        private SoundHandle SwitchMusic(AudioClip clip, float fadeSeconds)
        {
            if (_music.IsPlaying && ReferenceEquals(_musicClip, clip)) return _music;

            _music.Stop(fadeSeconds);

            SoundBuilder builder = new SoundBuilder(clip).Config(SoundConfiguration.Music).FadeIn(fadeSeconds);
            builder._music = true;

            _musicClip = clip;
            _music = Play(in builder, clip);
            return _music;
        }

        private void EndMusic(float fadeSeconds)
        {
            _music.Stop(fadeSeconds);
            _music = default;
            _musicClip = null;
        }

        private SoundHandle Play(in SoundBuilder builder, AudioClip clip)
        {
            Transform target = builder._target;
            bool follow = target != null;

            SoundSettings settings = builder._settings;
            if (settings == null)
            {
                settings = FindPreset(builder.ResolveConfiguration(follow));
                if (ReferenceEquals(settings, null)) return default;
            }

            if (builder._frequent && _frequent.Count > 0 && _frequent.Count >= _settings.maxFrequentSounds)
                Release(_frequent.Oldest, true);

            if (_activeCount >= _maxActive && !builder._music)
            {
                WarnLimit();
                return default;
            }

            SoundEmitter emitter = _pools.Take(settings);

            if (_activeCount == _active.Length)
                System.Array.Resize(ref _active, _activeCount * 2);

            emitter.ActiveIndex = _activeCount;
            _active[_activeCount++] = emitter;

            if (builder._frequent) _frequent.Add(emitter);

            emitter.Begin(in builder, clip, settings, follow ? target : null);
            return new SoundHandle(emitter);
        }

        private void Release(SoundEmitter emitter, bool stop)
        {
            emitter.End(stop);
            if (emitter.Frequent) _frequent.Remove(emitter);

            int index = emitter.ActiveIndex;
            int last = --_activeCount;
            SoundEmitter moved = _active[last];
            _active[index] = moved;
            moved.ActiveIndex = index;
            _active[last] = null;
            emitter.ActiveIndex = -1;

            _pools.Release(emitter);
        }

        private void ReleaseAll()
        {
            for (int i = _activeCount - 1; i >= 0; i--)
                Release(_active[i], true);
        }

        private void WarnLimit()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (_limitWarned) return;

            _limitWarned = true;
            Debug.LogWarning("AudioSystem: " + _maxActive + " sounds are playing at once, new sounds are skipped. Raise Max Virtual Voices in Project Settings > Audio (" + MusicReserve + " are kept for music)");
#endif
        }
    }
}
