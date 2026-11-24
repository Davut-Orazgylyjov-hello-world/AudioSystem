using UnityEngine;

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
        [SerializeField] private bool isFrequent;

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
            if (sounds == null || sounds.Length == 0) return;

            Sound.Clip(sounds[Random.Range(0, sounds.Length)])
                .Config(soundConfiguration)
                .At(transform.position)
                .Volume(volume)
                .Frequent(isFrequent)
                .Play();
        }
    }
}
