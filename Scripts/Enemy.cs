using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 3f;
    [SerializeField] private float _directionChangeInterval = 2f;
    [SerializeField] private float _randomMovementRange = 3f;

    [Header("Shooting Settings")]
    [SerializeField] private EnemyBullet _bulletPrefab;
    [SerializeField] private float _shootCooldown = 2f;
    [SerializeField] private Transform _shootPoint;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip _shootSound;
    private AudioSource _audioSource;

    [Header("Effects")]
    [SerializeField] private GameObject _explosionEffect;

    [Header("Health Settings")]
    [SerializeField] private int _maxHP = 3;

    [SerializeField] private AudioClip _hitSound;
    [SerializeField] private AudioClip _explosionSound;

    private int _currentHP;

    private bool _isInvincible = false;
    private Rigidbody2D _rigidbody;
    private Vector2 _moveDirection;
    private Camera _mainCamera;
    private float _screenWidth;
    private float _screenHeight;
    private float _nextDirectionChangeTime;
    private float _nextShootTime;
    private bool _hasEnteredScreen = false;
    private bool _isDead = false;
    private Transform _player;
    private Vector2 _targetPosition;
    public GameObject dropItemPrefab;
    private bool _itemDropped = false;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _audioSource = GetComponent<AudioSource>();
        _mainCamera = Camera.main;
        _player = GameObject.FindGameObjectWithTag("Player")?.transform;

        Vector2 screenBounds = _mainCamera.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
        _screenWidth = screenBounds.x;
        _screenHeight = screenBounds.y;

        _currentHP = _maxHP;
    }

    private void OnEnable()
    {
        SpawnAboveScreen();
        _hasEnteredScreen = false;
        SetRandomTargetPosition();
        _nextShootTime = Time.time + Random.Range(0.5f, _shootCooldown);
        _currentHP = _maxHP;
        _isDead = false;
    }

    private void FixedUpdate()
    {
        if (_isDead) return;

        if (!_hasEnteredScreen)
        {
            if (IsInsideScreen())
            {
                _hasEnteredScreen = true;
                SetRandomTargetPosition();
            }
        }
        else
        {
            if (Vector2.Distance(transform.position, _targetPosition) < 0.1f ||
                Time.time > _nextDirectionChangeTime)
            {
                SetRandomTargetPosition();
                _nextDirectionChangeTime = Time.time + _directionChangeInterval;
            }

            if (Time.time > _nextShootTime && _player != null)
            {
                ShootAtPlayer();
                _nextShootTime = Time.time + _shootCooldown;
            }
        }

        MoveEnemy();
    }

    private void MoveEnemy()
    {
        _moveDirection = (_targetPosition - (Vector2)transform.position).normalized;
        Vector2 moveAmount = _moveDirection * _moveSpeed * Time.fixedDeltaTime;
        _rigidbody.MovePosition(_rigidbody.position + moveAmount);

        Vector2 viewPos = _mainCamera.WorldToViewportPoint(_rigidbody.position);

        if (viewPos.x < 0.05f || viewPos.x > 0.95f)
        {
            _targetPosition.x = Mathf.Clamp(_targetPosition.x, -_screenWidth + 1, _screenWidth - 1);
            _targetPosition.y = transform.position.y + Random.Range(-1f, 1f);
        }

        if (viewPos.y < 0.05f || viewPos.y > 0.95f)
        {
            _targetPosition.y = Mathf.Clamp(_targetPosition.y, -_screenHeight + 1, _screenHeight - 1);
            _targetPosition.x = transform.position.x + Random.Range(-1f, 1f);
        }
    }

    private void ShootAtPlayer()
    {
        if (_bulletPrefab == null || _shootPoint == null || _player == null) return;

        EnemyBullet bullet = Instantiate(_bulletPrefab, _shootPoint.position, Quaternion.identity);

        Vector2 shootDirection = (_player.position - _shootPoint.position).normalized;
        bullet.SetDirection(shootDirection);

        if (_shootSound != null && _audioSource != null)
        {
            _audioSource.PlayOneShot(_shootSound);
        }
    }

    private void SpawnAboveScreen()
    {
        Vector2 spawnPosition = new Vector2(
            Random.Range(-_screenWidth, _screenWidth),
            _screenHeight + 1
        );
        transform.position = spawnPosition;
        _moveDirection = Vector2.down;
    }

    private void SetRandomTargetPosition()
    {
        _targetPosition = new Vector2(
            transform.position.x + Random.Range(-_randomMovementRange, _randomMovementRange),
            transform.position.y + Random.Range(-_randomMovementRange, _randomMovementRange)
        );

        _targetPosition.x = Mathf.Clamp(_targetPosition.x, -_screenWidth + 1, _screenWidth - 1);
        _targetPosition.y = Mathf.Clamp(_targetPosition.y, -_screenHeight + 1, _screenHeight - 1);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_isDead) return;

        if (collision.CompareTag("PlayerBullet"))
        {
            int damage = 1;

            var bullet = collision.GetComponent<PlayerBullet>();
            if (bullet != null)
                damage = bullet.Damage;

            TakeDamage(damage);

            Destroy(collision.gameObject);
        }
        else if (collision.CompareTag("Player"))
        {
            TakeDamage(_currentHP);
        }
    }

    private void TakeDamage(int damage)
    {
        if (_isInvincible || _isDead) return;

        _currentHP -= damage;

        if (_currentHP <= 0)
        {
            GameManager.Instance.EnemyDefeated();
            DestroyEnemy();
        }
        else
        {
            StartCoroutine(HandleInvincibility());
            if (_hitSound != null && _audioSource != null)
            {
                _audioSource.PlayOneShot(_hitSound);
            }
        }
    }

    private IEnumerator HandleInvincibility()
    {
        _isInvincible = true;

        var renderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (var renderer in renderers)
        {
            renderer.enabled = false;
        }

        yield return new WaitForSeconds(0.1f);

        foreach (var renderer in renderers)
        {
            renderer.enabled = true;
        }

        _isInvincible = false;
    }

    public void DestroyEnemy()
    {
        if (_isDead) return;

        _isDead = true;

        if (!_itemDropped && GameManager.Instance.CanDropItem())
        {
            Instantiate(dropItemPrefab, transform.position, Quaternion.identity);
            GameManager.Instance.MarkDropSpawned();
            _itemDropped = true;
        }

        if (_explosionEffect != null)
        {
            GameObject explosion = Instantiate(_explosionEffect, transform.position, Quaternion.identity);
            Destroy(explosion, 0.5f);
        }

        if (_explosionSound != null && _audioSource != null)
        {
            _audioSource.PlayOneShot(_explosionSound);
        }

        var renderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (var renderer in renderers)
        {
            renderer.enabled = false;
        }

        var colliders = GetComponents<Collider2D>();
        foreach (var col in colliders)
        {
            col.enabled = false;
        }

        StartCoroutine(DestroyAfterSound());
    }

    private IEnumerator DestroyAfterSound()
    {
        float delay = _explosionSound != null ? _explosionSound.length : 0.5f;
        yield return new WaitForSeconds(delay);
        Destroy(gameObject);
    }

    private bool IsInsideScreen()
    {
        Vector2 viewPos = _mainCamera.WorldToViewportPoint(transform.position);
        return viewPos.x > 0 && viewPos.x < 1 && viewPos.y > 0 && viewPos.y < 1;
    }
}
