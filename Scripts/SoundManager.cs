using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [SerializeField] Image soundOnIcon;
    [SerializeField] Image soundOffIcon;

    private bool muted = false;

    void Awake() 
    {
        if (Instance == null) 
        {
            Instance = this;
        }
        else 
        {
            Destroy(gameObject);
        }
    }

    void Start() 
    {
        Load();
        UpdateAudio();
        UpdateButtonIcon();
    }

    public void UpdateAudio() 
    {
        BackgroundMusic.Instance?.SetVolume(muted ? 0f : 0.45f);
    }

    public void OnButtonPress() 
    {
        muted = !muted;
        Save();
        UpdateAudio();
        UpdateButtonIcon();
    }

    private void UpdateButtonIcon() 
    {
        soundOnIcon.enabled = !muted;
        soundOffIcon.enabled = muted;
    }

    private void Load() 
    {
        muted = PlayerPrefs.GetInt("muted", 0) == 1;
    }

    private void Save() 
    {
        PlayerPrefs.SetInt("muted", muted ? 1 : 0);
    }
}
