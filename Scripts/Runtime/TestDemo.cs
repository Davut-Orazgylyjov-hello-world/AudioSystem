using UnityEngine;
using AudioSystem;

public class TestDemo : MonoBehaviour
{
    public SoundConfiguration soundConfiguration;
    public AudioClip shot;
    public bool withRandomPitch;

    public void ActivateSound()
    {
        if (withRandomPitch)
            AudioController.CreateSoundBuilder().WithRandomPitch().Play(shot, soundConfiguration);
        else AudioController.CreateSoundBuilder().Play(shot, soundConfiguration);
    }

    void OnGUI()
    {
        // Make a background box
        GUI.Box(new Rect(10, 10, 100, 90), "Test Sound Demo");

        if (GUI.Button(new Rect(20, 40, 80, 20), "ActivateSound"))
        {
            ActivateSound();
        }

        if (GUI.Button(new Rect(20, 70, 80, 20), "Nothing"))
        {
        }
    }
}