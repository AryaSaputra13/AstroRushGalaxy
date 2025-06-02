using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 3f;
    [SerializeField] private float _directionChangeInterval = 2f;
    [SerializeField] private float _randomMovementRange = 3f; // Jarak sebelum ganti arah
    
    [Header("Shooting Settings")]
    [SerializeField] private EnemyBullet _bulletPrefab;
    [SerializeField] private float _shootCooldown = 2f;
    [SerializeField] private Transform _shootPoint;
    
    [Header("Effects")]
    [SerializeField] private GameObject _explosionEffect;
    
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
    private Vector2 _targetPosition; // Target posisi acak
    private float _distanceToTarget; // Jarak ke target

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _mainCamera = Camera.main;
        _player = GameObject.FindGameObjectWithTag("Player")?.transform;
        
        Vector2 screenBounds = _mainCamera.ScreenToWorldPoint(new Vector2(Screen.width, Screen.height));
        _screenWidth = screenBounds.x;
        _screenHeight = screenBounds.y;
    }

    private void OnEnable()
    {
        SpawnAboveScreen();
        _hasEnteredScreen = false;
        SetRandomTargetPosition();
        _nextShootTime = Time.time + Random.Range(0.5f, _shootCooldown);
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
            // Cek jika sudah mencapai target atau waktunya ganti arah
            if (Vector2.Distance(transform.position, _targetPosition) < 0.1f || 
                Time.time > _nextDirectionChangeTime)
            {
                SetRandomTargetPosition();
                _nextDirectionChangeTime = Time.time + _directionChangeInterval;
            }

            // Shooting logic
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
        // Bergerak menuju target position
        _moveDirection = (_targetPosition - (Vector2)transform.position).normalized;
        Vector2 moveAmount = _moveDirection * _moveSpeed * Time.fixedDeltaTime;
        _rigidbody.MovePosition(_rigidbody.position + moveAmount);

        // Screen bounds check dengan pantulan
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

        Vector2 direction = (_player.position - _shootPoint.position).normalized;
        EnemyBullet bullet = Instantiate(_bulletPrefab, _shootPoint.position, Quaternion.identity);
        bullet.SetDirection(direction);
    }

    private void SpawnAboveScreen()
    {
        // Selalu spawn dari atas layar
        Vector2 spawnPosition = new Vector2(
            Random.Range(-_screenWidth, _screenWidth), 
            _screenHeight + 1
        );
        transform.position = spawnPosition;
        _moveDirection = Vector2.down;
    }

    private void SetRandomTargetPosition()
    {
        // Set target position baru dengan jarak minimal 3 unit
        _targetPosition = new Vector2(
            transform.position.x + Random.Range(-_randomMovementRange, _randomMovementRange),
            transform.position.y + Random.Range(-_randomMovementRange, _randomMovementRange)
        );

        // Pastikan target tetap dalam layar
        _targetPosition.x = Mathf.Clamp(_targetPosition.x, -_screenWidth + 1, _screenWidth - 1);
        _targetPosition.y = Mathf.Clamp(_targetPosition.y, -_screenHeight + 1, _screenHeight - 1);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_isDead) return;

        if (collision.CompareTag("PlayerBullet"))
        {
            DestroyEnemy();
            Destroy(collision.gameObject);
            GameManager.Instance.AddScore(10);
        }
    }

    public void DestroyEnemy()
    {
        _isDead = true;
        if (_explosionEffect != null)
        {
            Instantiate(_explosionEffect, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
    }

    private bool IsInsideScreen()
    {
        Vector2 viewPos = _mainCamera.WorldToViewportPoint(transform.position);
        return viewPos.x > 0 && viewPos.x < 1 && viewPos.y > 0 && viewPos.y < 1;
    }

}