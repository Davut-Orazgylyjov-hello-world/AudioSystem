using UnityEngine;
using UnityEngine.Audio;

namespace AudioSystem
{
    internal sealed class ChannelVolumes
    {
        private static readonly string[] Parameters = { "MasterAudio", "EffectsAudio", "VoiceAudio", "MusicAudio", "PauseAudio" };

        private readonly AudioMixer _mixer;
        private readonly float[] _values = new float[Parameters.Length];
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private bool _warned;
#endif

        internal ChannelVolumes(AudioMixer mixer, float[] startValues)
        {
            _mixer = mixer;
            for (int i = 0; i < _values.Length; i++)
                _values[i] = startValues != null && i < startValues.Length ? Mathf.Clamp01(startValues[i]) : 1f;
        }

        internal float Get(SoundChannel channel)
        {
            return _values[(int)channel];
        }

        internal void Set(SoundChannel channel, float volume)
        {
            int index = (int)channel;
            _values[index] = Mathf.Clamp01(volume);
            Apply(index);
        }

        internal void ApplyAll()
        {
            for (int i = 0; i < _values.Length; i++)
                Apply(i);
        }

        private void Apply(int index)
        {
            if (_mixer != null && _mixer.SetFloat(Parameters[index], _values[index].ToLogarithmicVolume())) return;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (_warned) return;

            _warned = true;
            Debug.LogWarning("AudioSystem: mixer parameter " + Parameters[index] + " is not available, check the mixer in Tools > Audio System > Settings");
#endif
        }
    }
}
