using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneMusicManager : MonoBehaviour
{
    [SerializeField] private AudioClip sceneMusic;

    void Start()
    {
        if (BackgroundMusic.Instance != null)
        {
            BackgroundMusic.Instance.ChangeMusic(sceneMusic);
            SoundManager.Instance?.UpdateAudio();
        }
        
    }
}
