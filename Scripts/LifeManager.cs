using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LifeManager : MonoBehaviour
{
    public static LifeManager Instance;
    
    [SerializeField] private TextMeshProUGUI lifeText; // Referensi ke TMP UI Text
    [SerializeField] private int startingLives = 5;
    
    private int currentLives;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        UpdateLifeDisplay();
    }

    public void SetStartingLives(int lives)
    {
        startingLives = lives;
        currentLives = lives;
        UpdateLifeDisplay();
    }

    public void TakeHit()
    {
        currentLives--;

        if (currentLives <= 0)
        {
            currentLives = 0;
            UpdateLifeDisplay();
            GameManager.Instance.GameOver(); // Panggil game over dari GameManager
        }
        else
        {
            UpdateLifeDisplay();
        }
    }

    public void UpdateLifeDisplay()
    {
        lifeText.text = "X " + currentLives;
    }

    public int GetCurrentLives()
    {
        return currentLives;
    }

    public void ResetLives()
    {
        currentLives = startingLives;
        UpdateLifeDisplay();
    }
}
