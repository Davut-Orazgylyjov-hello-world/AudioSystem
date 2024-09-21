using UnityEngine;
using Random = UnityEngine.Random;

namespace AudioSystem
{
    public class RandomSound : MonoBehaviour
    {
        private enum ActivateOn
        {
            Enable,
            Function,
            Start
        }

        [SerializeField] private AudioClip[] sounds;
        [SerializeField] private SoundConfiguration soundConfiguration;
        [SerializeField] private ActivateOn activateOn;
        [SerializeField] [Range(0f, 1f)] private float volume = 1f;
        [SerializeField] private bool isFrequent = false;

        private void OnEnable()
        {
            if (activateOn == ActivateOn.Enable)
                PlayRandomSound();
        }

        private void Start()
        {
            if (activateOn == ActivateOn.Start)
                PlayRandomSound();
        }

        public void PlayRandomSound()
        {
            AudioController.CreateSoundBuilder()
                .WithVolume(volume)
                .WithPosition(transform.position)
                .Play(GetRandomClip(), soundConfiguration, isFrequent);
        }

        private AudioClip GetRandomClip()
        {
            return sounds[Random.Range(0, sounds.Length)];
        }
    }
}