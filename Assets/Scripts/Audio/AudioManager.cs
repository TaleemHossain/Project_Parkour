using UnityEngine;
using System;
using UnityEngine.Audio;
public class AudioManager : MonoBehaviour
{
    public Sound[] sounds;
    void Awake()
    {
        foreach (var sound in sounds)
        {
            sound.source = gameObject.AddComponent<AudioSource>();
            sound.source.clip = sound.clip;
            sound.source.pitch = sound.pitch;
            sound.source.volume = sound.volume;
            sound.source.loop = sound.loop;
        }
    }
    public void PlaySound(string name)
    {
        Sound sound = Array.Find(sounds, sound => sound.name == name);
        if (sound == null)
        {
            Debug.Log("Sound null for " + name);
            return;
        }
        sound.source.Play();
    }
    public void PauseSound(string name)
    {
        Sound sound = Array.Find(sounds, sound => sound.name == name);
        if (sound == null)
        {
            Debug.Log("Sound null for " + name);
            return;
        }
        sound.source.Pause();
    }
}
