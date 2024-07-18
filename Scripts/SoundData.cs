using System;
using UnityEngine;

namespace AudioSystem
{
    [Serializable]
    public struct SoundData 
    {
        public AudioClip audioClip;
        public SoundSettings soundSettings;
        public bool isFrequentSound;

        public SoundData(AudioClip audioClip, SoundSettings soundSettings, bool isFrequentSound)
        {
            this.audioClip = audioClip;
            this.soundSettings = soundSettings;
            this.isFrequentSound = isFrequentSound;
        }
    }
}