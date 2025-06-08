using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : MonoBehaviour
{
   [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 2f;
    [SerializeField] private float _directionChangeInterval = 3f;
    [SerializeField] private float _randomMovementRange = 2f;

    [Header("Shooting Settings")]
    [SerializeField] private GameObject _bulletPrefab;

    [SerializeField] private Transform[] _shootPoints;
    [SerializeField] private float _shootCooldown = 3f;
    [SerializeField] private float _delayBetweenBullets = 0.3f;

    [Header("Enemy Spawning")]
    [SerializeField] private GameObject[] _spawnEnemies;
    [SerializeField] private float _spawnInterval = 5f;
    [SerializeField] private int _maxSpawnCount = 5; // tambahkan ini

    private int _spawnedCount = 0;


    [Header("Audio Settings")]
    [SerializeField] private AudioClip _shootSound;
    [SerializeField] private AudioClip _hitSound;
    [SerializeField] private AudioClip _explosionSound;
    private AudioSource _audioSource;

    [Header("Effects")]
    [SerializeField] private GameObject _explosionEffect;

    [Header("Health Settings")]
    [SerializeField] private int _maxHP = 20;
    private int _currentHP;
    private bool _isInvincible = false;
    private bool _isDead = false;

    private Rigidbody2D _rigidbody;
    private Transform _player;
    private Vector2 _targetPosition;
    private Camera _mainCamera;
    private float _screenWidth;
    private float _screenHeight;
    private float _nextDirectionChangeTime;
    private float _nextShootTime;
    private float _nextSpawnTime;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _audioSource = GetComponent<AudioSource>();
        _player = GameObject.FindGameObjectWithTag("Player")?.transform;
        _mainCamera = Camera.main;

        Vector2 screenBounds = _mainCamera.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
        _screenWidth = screenBounds.x;
        _screenHeight = screenBounds.y;

        _currentHP = _maxHP;
    }

    private void OnEnable()
    {
        _spawnedCount = 0;
        transform.position = new Vector2(0, _screenHeight - 1f);
        _currentHP = _maxHP;
        _isDead = false;
        SetRandomTargetPosition();
        _nextDirectionChangeTime = Time.time + _directionChangeInterval;
        _nextShootTime = Time.time + 1f;
        _nextSpawnTime = Time.time + _spawnInterval;
    }

    private void FixedUpdate()
    {
        if (_isDead) return;

        if (Time.time > _nextDirectionChangeTime)
        {
            SetRandomTargetPosition();
            _nextDirectionChangeTime = Time.time + _directionChangeInterval;
        }

        if (Time.time > _nextShootTime)
        {
            StartCoroutine(ShootMultiplePoints());
            _nextShootTime = Time.time + _shootCooldown;
        }

        if (Time.time > _nextSpawnTime)
        {
            SpawnEnemies();
            _nextSpawnTime = Time.time + _spawnInterval;
        }

        MoveBoss();
    }

    private void MoveBoss()
    {
        Vector2 moveDir = (_targetPosition - (Vector2)transform.position).normalized;
        Vector2 moveAmount = moveDir * _moveSpeed * Time.fixedDeltaTime;
        _rigidbody.MovePosition(_rigidbody.position + moveAmount);
    }

    private void SetRandomTargetPosition()
    {
        float halfScreen = _screenHeight / 2;

        _targetPosition = new Vector2(
            Random.Range(-_screenWidth + 1, _screenWidth - 1),
            Random.Range(halfScreen, _screenHeight - 1)
        );
    }

    private IEnumerator ShootMultiplePoints()
    {
        if (_bulletPrefab == null || _shootPoints.Length == 0) yield break;

        Vector2 dir = Vector2.down;

        foreach (var point in _shootPoints)
        {
            if (point == null) continue;

            GameObject bulletGO = Instantiate(_bulletPrefab, point.position, Quaternion.identity);
            BossBullet bullet = bulletGO.GetComponent<BossBullet>();
            if (bullet != null)
            {
                bullet.SetDirection(dir);
            }

            if (_shootSound != null)
                _audioSource.PlayOneShot(_shootSound);

            yield return new WaitForSeconds(_delayBetweenBullets);
        }
    }


   private void SpawnEnemies()
    {
        if (_spawnEnemies.Length == 0) return;
        if (_spawnedCount >= _maxSpawnCount) return;

        foreach (var enemyPrefab in _spawnEnemies)
        {
            if (enemyPrefab == null) continue;

            Vector2 spawnPos = transform.position + new Vector3(Random.Range(-1.5f, 1.5f), -1f);
            Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            GameManager.Instance?.RegisterEnemy();
            _spawnedCount++;
            if (_spawnedCount >= _maxSpawnCount) break;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_isDead || _isInvincible) return;

        if (collision.CompareTag("PlayerBullet"))
        {
            int damage = 1;
            var bullet = collision.GetComponent<PlayerBullet>();
            if (bullet != null) damage = bullet.Damage;

            TakeDamage(damage);
            Destroy(collision.gameObject);
        }
    }

    private void TakeDamage(int damage)
    {
        if (_isInvincible) return;

        _currentHP -= damage;

        if (_currentHP <= 0)
        {
            DestroyBoss();
        }
        else
        {
            StartCoroutine(HandleInvincibility());

            if (_hitSound != null)
                _audioSource.PlayOneShot(_hitSound);
        }
    }

    private IEnumerator HandleInvincibility()
    {
        _isInvincible = true;

        var renderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (var r in renderers) r.enabled = false;
        yield return new WaitForSeconds(0.1f);
        foreach (var r in renderers) r.enabled = true;

        _isInvincible = false;
    }

    public void DestroyBoss()
    {
        if (_isDead) return;

        _isDead = true;

        if (_explosionEffect != null)
        {
            Debug.Log(1);
            GameObject explosion = Instantiate(_explosionEffect, transform.position, Quaternion.identity);
            Destroy(explosion, 1f);

            StartCoroutine(DelayedEnemyDefeated(0.5f));
        }
        else
        {
            GameManager.Instance.EnemyDefeated();
        }

        if (_explosionSound != null)
        {
            Debug.Log("Noss");
            _audioSource.PlayOneShot(_explosionSound);
        }
        Destroy(gameObject, 1f);
    }
    
    private IEnumerator DelayedEnemyDefeated(float delay)
    {
        Debug.Log(2);
        yield return new WaitForSeconds(delay);
        GameManager.Instance.BossCutScene();
    }
}