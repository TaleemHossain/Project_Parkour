using NUnit.Framework;
using UnityEngine;
using UnityEngine.Audio;

public class optionsMenu : MonoBehaviour
{
    public AudioMixer audioMixer;
    public void SetVolume(float volume)
    {
        var setVolume = volume * 100 - 80;
        audioMixer.SetFloat("volume", setVolume);
    }
    public void SetQuality(int index)
    {
        FindFirstObjectByType<AudioManager>().PlaySound("Click");
        QualitySettings.SetQualityLevel(index);
    }
    public void SetFullScreen(bool isFullScreen)
    {
        FindFirstObjectByType<AudioManager>().PlaySound("Click");
        Screen.fullScreen = isFullScreen;
    }
}