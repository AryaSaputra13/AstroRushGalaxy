using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Player : MonoBehaviour
{
    [SerializeField] private float _moveSpeed = 4f;
    [SerializeField] private float _shootDelay = 0.1f;
    [SerializeField] private PlayerBullet _bulletPrefab;
    [SerializeField] private GameObject _explosionEffect;
    [SerializeField] private int _maxLives = 5;
    [SerializeField] private float _invulnerabilityDuration = 2f;
    [SerializeField] private float _blinkInterval = 0.1f;
    [SerializeField] private AudioClip _shootSFX;
    [SerializeField] private AudioClip _explosionSFX;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI lifeText;

    private int _currentLives;
    private AudioSource _audioSource;
    private Rigidbody2D _rigidbody;
    private Vector2 _moveDirection;
    private float _shootDelayCounter;
    private bool _isInvulnerable = false;
    private SpriteRenderer _spriteRenderer;
    private Coroutine _blinkCoroutine;
    public GameObject[] bulletLevels;
    private int currentBulletLevel = 0;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _audioSource = GetComponent<AudioSource>();
        _currentLives = _maxLives;
        UpdateLifeUI();
    }

    private void FixedUpdate()
    {
        if (_currentLives <= 0) return;

        Vector2 moveTarget = _rigidbody.position + _moveDirection * _moveSpeed * Time.fixedDeltaTime;

        Vector3 viewPos = Camera.main.WorldToViewportPoint(moveTarget);
        viewPos.x = Mathf.Clamp01(viewPos.x);
        viewPos.y = Mathf.Clamp01(viewPos.y);
        moveTarget = Camera.main.ViewportToWorldPoint(viewPos);

        _rigidbody.MovePosition(moveTarget);

        _shootDelayCounter += Time.fixedDeltaTime;
        if (_isShootingHeld && _shootDelayCounter > _shootDelay)
        {
            _shootDelayCounter = 0f;
            Instantiate(bulletLevels[currentBulletLevel], transform.position, transform.rotation);

            if (_shootSFX != null) 
            {
                _audioSource.PlayOneShot(_shootSFX);
            }
        }
    }

    public int GetCurrentLives()
    {
        return _currentLives;
    }

    public void UpgradeBullet()
    {
        if (currentBulletLevel < bulletLevels.Length - 1)
        {
            currentBulletLevel++;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_isInvulnerable || _currentLives <= 0) return;

        if (collision.CompareTag("Enemy"))
        {
            TakeDamage(1);
            collision.GetComponent<Enemy>().DestroyEnemy();
        }
        
        if (collision.CompareTag("Boss"))
        {
            TakeDamage(100);
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
        _spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
        _isInvulnerable = false;
        _spriteRenderer.enabled = true;
    }

    private IEnumerator BlinkEffect()
    {
        while (true)
        {
            _spriteRenderer.enabled = !_spriteRenderer.enabled;

            if (_spriteRenderer.enabled)
            {
                _spriteRenderer.color = new Color(1f, 1f, 1f, 0.5f);
            }
            else
            {
                _spriteRenderer.color = new Color(1f, 1f, 1f, 1f);
            }

            yield return new WaitForSeconds(_blinkInterval);
        }
    }


    public void TakeDamage(int damageAmount)
    {
        if (_isInvulnerable) return;

        _currentLives -= damageAmount;
        _currentLives = Mathf.Max(_currentLives, 0);
        UpdateLifeUI();

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
        
        if (_explosionSFX != null)
        {
            GameObject tempAudio = new GameObject("TempExplosionAudio");
            tempAudio.transform.position = transform.position;

            AudioSource audioSource = tempAudio.AddComponent<AudioSource>();
            audioSource.clip = _explosionSFX;
            audioSource.Play();

            Destroy(tempAudio, _explosionSFX.length);
        }
        
        Destroy(gameObject);
        GameManager.Instance.GameOver();
    }

    private void UpdateLifeUI()
    {
        lifeText.text = "X " + _currentLives;
    }

    public void ResetLives()
    {
        _currentLives = _maxLives;
        UpdateLifeUI();
    }

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
