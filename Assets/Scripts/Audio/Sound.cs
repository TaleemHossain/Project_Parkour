using UnityEngine;
using UnityEngine.Audio;
[System.Serializable]
public class Sound
{
    public AudioClip clip;
    public string name;
    [Range(0f, 1f)]
    public float volume;
    [Range(-1f, 1f)]
    public float pitch;
    public bool loop = false;
    [HideInInspector]
    public AudioSource source;
}
