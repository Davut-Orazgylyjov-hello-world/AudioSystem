using UnityEngine;
using UnityEngine.Audio;

namespace AudioSystem
{
    public sealed class AudioSystemSettings : ScriptableObject
    {
        public const string ProjectResource = "AudioSystemSettings";
        public const string DefaultResource = "AudioSystemDefaultSettings";

        [Tooltip("One preset per SoundConfiguration: Effect2D, Effect3D, Voice2D, Voice3D, Music, Pause")]
        public SoundSettings[] presets = new SoundSettings[6];

        [Tooltip("Mixer with exposed volume parameters MasterAudio, EffectsAudio, VoiceAudio, MusicAudio, PauseAudio")]
        public AudioMixer mixer;

        [Tooltip("Start volume per SoundChannel: Master, Effects, Voice, Music, Pause")]
        public float[] volumes = { 1f, 1f, 1f, 1f, 1f };

        [Tooltip("Sound sources created at startup, shared between effect and voice presets")]
        [Min(0)] public int prewarmCount = 64;

        [Tooltip("Idle sound sources kept for reuse, extra ones are destroyed")]
        [Min(0)] public int maxPoolSize = 512;

        [Tooltip("Sounds marked Frequent that may play at once, the oldest one is stopped beyond it")]
        [Min(1)] public int maxFrequentSounds = 30;

        public static AudioSystemSettings Load()
        {
            AudioSystemSettings settings = Resources.Load<AudioSystemSettings>(ProjectResource);
            return settings != null ? settings : Resources.Load<AudioSystemSettings>(DefaultResource);
        }

        public SoundSettings GetPreset(SoundConfiguration configuration)
        {
            int index = (int)configuration;
            return presets != null && index < presets.Length ? presets[index] : null;
        }
    }
}
