using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Level Settings")]
    [SerializeField] private List<LevelConfig> _levels;
    [SerializeField] private float _delayBetweenLevels = 2f;

    [Header("UI References")]
    [SerializeField] private CanvasGroup levelTitleCanvasGroup;
    [SerializeField] private TextMeshProUGUI levelTitleText;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private float levelDisplayDuration = 1.5f;

    [Header("Game Over UI")]
    [SerializeField] private CanvasGroup gameOverCanvasGroup;

    [Header("Audio")]
    [SerializeField] private AudioClip gameOverSound;

    private bool isGameOver = false;
    private int _currentLevel = 0;
    private int _activeEnemies = 0;
    private bool dropSpawned = false;
    private AudioSource audioSource;

    [System.Serializable]
    public class LevelConfig
    {
        public int level;
        public Enemy enemyPrefab;
        public int enemyCount;
        public Boss bossPrefab;
        public bool isBossLevel = false;
    }

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        Time.timeScale = 1f;

        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    private void Start()
    {
        isGameOver = false;
        _currentLevel = 0;
        dropSpawned = false;
        _activeEnemies = 0;
        if (gameOverCanvasGroup != null)
        {
            gameOverCanvasGroup.gameObject.SetActive(false);
        }
        
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
            dropSpawned = false;

            _currentLevel++;

            StartCoroutine(StartLevel(_currentLevel));
        }
    }

    private IEnumerator StartLevel(int levelIndex)
    {
        if (levelIndex >= _levels.Count)
        {
            // Debug.Log("All levels completed!");
            GameOver();
            yield break;
        }

        var config = _levels[levelIndex];

        levelTitleCanvasGroup.gameObject.SetActive(true);
        levelTitleText.text = $"Level {levelIndex + 1}";

        yield return StartCoroutine(FadeCanvasGroup(levelTitleCanvasGroup, 0, 1, fadeDuration));
        yield return new WaitForSeconds(levelDisplayDuration);
        yield return StartCoroutine(FadeCanvasGroup(levelTitleCanvasGroup, 1, 0, fadeDuration));

        levelTitleText.text = "";
        levelTitleCanvasGroup.gameObject.SetActive(false);

        yield return new WaitForSeconds(1f);

        _activeEnemies = config.enemyCount;

        for (int i = 0; i < config.enemyCount; i++)
        {
            Enemy enemy = Instantiate(config.enemyPrefab);
            enemy.transform.position = new Vector3(
                Random.Range(-5f, 5f), 6f + Random.Range(0f, 2f), 0f
            );
        }

        if (config.isBossLevel && config.bossPrefab != null)
        {
            Boss boss = Instantiate(config.bossPrefab);
            boss.transform.position = new Vector3(0f, 6f, 0f);
            _activeEnemies++;
        }
    }

    private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float from, float to, float duration)
    {
        float time = 0f;
        canvasGroup.alpha = from;
        canvasGroup.blocksRaycasts = false;

        if (from == 0 && to > 0)
            canvasGroup.gameObject.SetActive(true);

        while (time < duration)
        {
            time += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Lerp(from, to, time / duration);
            yield return null;
        }

        canvasGroup.alpha = to;

        if (to == 0)
            canvasGroup.gameObject.SetActive(false);
    }

    public void RegisterEnemy()
    {
        _activeEnemies++;
    }

    public void GameOver()
    {
        if (isGameOver) return;
        isGameOver = true;

        Time.timeScale = 0f;
        //Debug.Log("Game Over!");

        if (gameOverSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(gameOverSound);
        }

        if (gameOverCanvasGroup != null)
        {
            gameOverCanvasGroup.gameObject.SetActive(true);
            StartCoroutine(FadeCanvasGroup(gameOverCanvasGroup, 0, 1, 0.5f));
            gameOverCanvasGroup.interactable = true;
            gameOverCanvasGroup.blocksRaycasts = true;
        }
    }

    public void BossCutScene()
    {
        SceneManager.LoadScene("CutScene");
    }

    public void restartGame()
    {
        Time.timeScale = 1f;
        isGameOver = false;
        _currentLevel = 0;
        dropSpawned = false;
        _activeEnemies = 0;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}