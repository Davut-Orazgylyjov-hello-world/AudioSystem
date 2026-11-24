using UnityEngine;

namespace AudioSystem
{
    public static class Sound
    {
        public static SoundHandle Play(AudioClip clip)
        {
            return new SoundBuilder(clip).Play();
        }

        public static SoundHandle Play(AudioClip clip, Vector3 position)
        {
            return new SoundBuilder(clip).At(position).Play();
        }

        public static SoundHandle Voice(AudioClip clip)
        {
            return new SoundBuilder(clip).Voice().Play();
        }

        public static SoundHandle Voice(AudioClip clip, Vector3 position)
        {
            return new SoundBuilder(clip).Voice().At(position).Play();
        }

        public static SoundHandle InPause(AudioClip clip)
        {
            return new SoundBuilder(clip).Config(SoundConfiguration.Pause).Play();
        }

        public static SoundHandle Music(AudioClip clip, float fadeSeconds = 1f)
        {
            return AudioController.PlayMusic(clip, fadeSeconds);
        }

        public static void StopMusic(float fadeSeconds = 1f)
        {
            AudioController.StopMusic(fadeSeconds);
        }

        public static SoundBuilder Clip(AudioClip clip)
        {
            return new SoundBuilder(clip);
        }

        public static void StopAll()
        {
            AudioController.StopAll();
        }

        public static void SetVolume(SoundChannel channel, float volume)
        {
            AudioController.SetVolume(channel, volume);
        }

        public static float GetVolume(SoundChannel channel)
        {
            return AudioController.GetVolume(channel);
        }

        public static SoundSettings GetSettings(SoundConfiguration configuration)
        {
            AudioSystemSettings settings = AudioSystemSettings.Load();
            return settings != null ? settings.GetPreset(configuration) : null;
        }
    }
}
