using System;
using UnityEngine;

namespace AudioSystem
{
    [Serializable]
    public class SoundData 
    {
        public AudioClip audioClip;
        public SoundSettings soundSettings;

        public SoundData(AudioClip audioClip, SoundSettings soundSettings)
        {
            this.audioClip = audioClip;
            this.soundSettings = soundSettings;
        }
    }
}