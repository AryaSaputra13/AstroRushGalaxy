using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundMusic : MonoBehaviour
{
    public static BackgroundMusic Instance { get; private set; }

    private AudioSource audioSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            audioSource = GetComponent<AudioSource>();
            ApplyMuteStatus();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetVolume(float volume)
    {
        if (audioSource != null)
        {
            audioSource.volume = volume;
        }
    }

    public void ChangeMusic(AudioClip newClip)
    {
        if (audioSource.clip == newClip) return;

        audioSource.Stop();
        audioSource.clip = newClip;
        ApplyMuteStatus();
        audioSource.Play();
    }

    private void ApplyMuteStatus()
    {
        int muted = PlayerPrefs.GetInt("muted", 0);
        SetVolume(muted == 1 ? 0f : 0.45f);
    }

    public bool IsPlaying(AudioClip clip)
    {
        return audioSource.clip == clip && audioSource.isPlaying;
    }
}
