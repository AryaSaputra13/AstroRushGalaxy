using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LifeManager : MonoBehaviour
{
    public static LifeManager Instance;
    
    [SerializeField] private Image[] _lifeIcons;
    
    private void Awake()
    {
        Instance = this;
    }
    
    public void UpdateLives(int currentLives)
    {
        for (int i = 0; i < _lifeIcons.Length; i++)
        {
            _lifeIcons[i].enabled = i < currentLives;
        }
    }
}
