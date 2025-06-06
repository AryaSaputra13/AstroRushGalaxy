using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [Header("Level Settings")]
    [SerializeField] private List<LevelConfig> _levels; // List level, atur di Inspector
    [SerializeField] private float _delayBetweenLevels = 2f;

    [Header("UI References")]
    [SerializeField] private CanvasGroup levelTitleCanvasGroup;
    [SerializeField] private TextMeshProUGUI levelTitleText;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float levelDisplayDuration = 1.5f;

    private int _currentLevel = 0;
    private int _activeEnemies = 0;
    private int _score;
    private bool dropSpawned = false;

    [System.Serializable]
    public class LevelConfig
    {
        public int level;
        public Enemy enemyPrefab;
        public int enemyCount;
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    private void Start()
    {
        StartCoroutine(StartLevel(_currentLevel));
    } 

    public bool CanDropItem()
    {
        return !dropSpawned;
    }

    public void MarkDropSpawned()
    {
        dropSpawned = true;
    }

    public void EnemyDefeated()
    {
        _activeEnemies--;

        if (_activeEnemies <= 0)
        {
            dropSpawned = false; // Reset drop untuk level berikutnya
            _currentLevel++;
            StartCoroutine(StartLevel(_currentLevel));
        }
    }
    
    private IEnumerator StartLevel(int levelIndex)
    {
        if (levelIndex >= _levels.Count)
        {
            Debug.Log("All levels completed!");
            GameOver();
            yield break;
        }

        LevelConfig config = _levels[levelIndex];

        // Tampilkan UI Level
        levelTitleText.text = $"Level {config.level}";
        yield return StartCoroutine(FadeCanvasGroup(levelTitleCanvasGroup, 0, 1, fadeDuration));

        // Tunggu durasi tampil
        yield return new WaitForSeconds(levelDisplayDuration);

        // Sembunyikan UI Level
        yield return StartCoroutine(FadeCanvasGroup(levelTitleCanvasGroup, 1, 0, fadeDuration));

        // Tunggu sebelum spawn musuh
        yield return new WaitForSeconds(1f); // jeda setelah UI menghilang

        _activeEnemies = config.enemyCount;

        for (int i = 0; i < config.enemyCount; i++)
        {
            Enemy enemy = Instantiate(config.enemyPrefab);
            enemy.transform.position = new Vector3(
                Random.Range(-5f, 5f), 6f + Random.Range(0f, 2f), 0f
            );
        }

        Debug.Log($"Level {config.level} started with {config.enemyCount} enemies.");
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float from, float to, float duration)
    {
        float time = 0f;
        canvasGroup.alpha = from;
        canvasGroup.blocksRaycasts = false;

        while (time < duration)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, time / duration);
            yield return null;
        }

        canvasGroup.alpha = to;
    }

    public void AddScore(int value)
    {
        _score += value;
        Debug.Log("Score: " + _score);
    }

    public void GameOver()
    {
        Debug.Log("Game Over!");
        // Tambahkan logika game over di sini
        // Contoh: menampilkan UI game over, menghentikan permainan, dll.
    }
}