using UnityEngine;
using Random = UnityEngine.Random;

namespace AudioSystem
{
    public class RandomEffectSoundOnEnable : MonoBehaviour
    {
        [SerializeField] private AudioClip[] sounds;
        [SerializeField] private SoundConfiguration soundConfiguration;
        [SerializeField] [Range(0f, 1f)] private float volume = 1f;

        private void OnEnable()
        {
            PlayRandomSound();
        }

        private void PlayRandomSound()
        {
            AudioController.CreateSoundBuilder()
                .WithVolume(volume)
                .WithPosition(transform.position)
                .Play(GetRandomClip(), soundConfiguration);
        }

        private AudioClip GetRandomClip()
        {
            return sounds[Random.Range(0, sounds.Length)];
        }
    }
}