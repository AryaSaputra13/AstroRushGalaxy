using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [SerializeField] private Enemy _enemyPrefab;
    [SerializeField] private int _enemyCount;
    [SerializeField] private float _enemySpawnRange;

    private int _score;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        SpawnEnemies();
    } 
    
    private void SpawnEnemies()
    {
        for (int i = 0; i < _enemyCount; i++)
        {
            Enemy enemy = Instantiate(_enemyPrefab);
            enemy.transform.position = new Vector3(
                Random.Range(-_enemySpawnRange, _enemySpawnRange), 4f, 0f
                );
        }
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