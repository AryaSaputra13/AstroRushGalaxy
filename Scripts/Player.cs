using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 4f;
    [SerializeField] private float _shootDelay = 0.1f;
    [SerializeField] private PlayerBullet _bulletPrefab;
    [SerializeField] private GameObject _explosionEffect;
    [SerializeField] private int _maxLives = 5;
    [SerializeField] private float _invulnerabilityDuration = 2f;
    [SerializeField] private float _blinkInterval = 0.1f;
    
    private Rigidbody2D _rigidbody;
    private Vector2 _moveDirection;
    private float _shootDelayCounter;
    private int _currentLives;
    private bool _isInvulnerable = false;
    private SpriteRenderer _spriteRenderer;
    private Coroutine _blinkCoroutine;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _currentLives = _maxLives;
    }

    private void FixedUpdate()
    {
        if (_isInvulnerable || _currentLives <= 0) return;

        Vector2 moveTarget = _rigidbody.position + _moveDirection * _moveSpeed * Time.fixedDeltaTime;

        // Batasi agar player tidak keluar layar
        Vector3 viewPos = Camera.main.WorldToViewportPoint(moveTarget);
        viewPos.x = Mathf.Clamp01(viewPos.x);
        viewPos.y = Mathf.Clamp01(viewPos.y);
        moveTarget = Camera.main.ViewportToWorldPoint(viewPos);

        _rigidbody.MovePosition(moveTarget);

        _shootDelayCounter += Time.fixedDeltaTime;
        if (_isShootingHeld && _shootDelayCounter > _shootDelay)
        {
            _shootDelayCounter = 0f;
            Instantiate(_bulletPrefab, transform.position, transform.rotation);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_isInvulnerable || _currentLives <= 0) return;

        if (collision.CompareTag("Enemy"))
        {
            TakeDamage();
            collision.GetComponent<Enemy>().DestroyEnemy();
        }
    }

    private void StartInvulnerability()
    {
        _isInvulnerable = true;
        _blinkCoroutine = StartCoroutine(BlinkEffect());
        Invoke(nameof(EndInvulnerability), _invulnerabilityDuration);
    }

    private void EndInvulnerability()
    {
        if (_blinkCoroutine != null)
        {
            StopCoroutine(_blinkCoroutine);
        }
        _isInvulnerable = false;
        _spriteRenderer.enabled = true; // Pastikan sprite visible di akhir
    }

    private IEnumerator BlinkEffect()
    {
        while (true)
        {
            _spriteRenderer.enabled = !_spriteRenderer.enabled;
            yield return new WaitForSeconds(_blinkInterval);
        }
    }

    public void TakeDamage()
    {
        if (_isInvulnerable) return;
    
        _currentLives--;
        LifeManager.Instance?.UpdateLives(_currentLives);
    
        if (_currentLives <= 0)
        {
            DestroyPlayer();
        }
        else
        {
            StartInvulnerability();
        }
    }

    public void DestroyPlayer()
    {
        if (_explosionEffect != null)
        {
            Instantiate(_explosionEffect, transform.position, Quaternion.identity);
        }
        Destroy(gameObject);
        GameManager.Instance.GameOver();
    }

    // Fungsi publik agar bisa dipanggil dari Button UI
    public void SetMoveDirectionX(float x)
    {
        _moveDirection.x = x;
    }

    public void SetMoveDirectionY(float y)
    {
        _moveDirection.y = y;
    }

    public void StopMovingX()
    {
        _moveDirection.x = 0f;
    }

    public void StopMovingY()
    {
        _moveDirection.y = 0f;
    }

    private bool _isShootingHeld = false;

    public void StartShooting()
    {
        _isShootingHeld = true;
    }

    public void StopShooting()
    {
        _isShootingHeld = false;
    }
}
