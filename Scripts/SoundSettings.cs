using UnityEngine;
using UnityEngine.Audio;

namespace AudioSystem
{
    [CreateAssetMenu(fileName = "SoundSettings", menuName = "AudioSystem/SoundSettings", order = 1)]
    public class SoundSettings : ScriptableObject
    {
        public AudioMixerGroup mixerGroup;
        public bool loop;

        public bool mute;
        public bool bypassEffects;
        public bool bypassListenerEffects;
        public bool bypassReverbZones;

        [Range(0, 256)] public int priority = 128;
        [Range(0f, 1f)] public float volume = 1f;
        [Range(-3f, 3f)] public float pitch = 1f;
        [Range(-1f, 1f)] public float panStereo;
        [Range(0f, 1f)] public float spatialBlend;
        [Range(0f, 1.1f)] public float reverbZoneMix = 1f;
        [Range(0f, 5f)] public float dopplerLevel = 1f;
        [Range(0f, 360f)] public float spread;

        public float minDistance = 1f;
        public float maxDistance = 500f;

        public bool ignoreListenerVolume;
        public bool ignoreListenerPause;

        public AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;

        internal int ComputeHash()
        {
            unchecked
            {
                int hash = ReferenceEquals(mixerGroup, null) ? 0 : mixerGroup.GetHashCode();
                hash = hash * 31 + priority;
                hash = hash * 31 + panStereo.GetHashCode();
                hash = hash * 31 + spatialBlend.GetHashCode();
                hash = hash * 31 + reverbZoneMix.GetHashCode();
                hash = hash * 31 + dopplerLevel.GetHashCode();
                hash = hash * 31 + spread.GetHashCode();
                hash = hash * 31 + minDistance.GetHashCode();
                hash = hash * 31 + maxDistance.GetHashCode();
                hash = hash * 31 + (int)rolloffMode;

                int flags = mute ? 1 : 0;
                if (bypassEffects) flags |= 2;
                if (bypassListenerEffects) flags |= 4;
                if (bypassReverbZones) flags |= 8;
                if (ignoreListenerVolume) flags |= 16;
                if (ignoreListenerPause) flags |= 32;
                return hash * 31 + flags;
            }
        }
    }
}
