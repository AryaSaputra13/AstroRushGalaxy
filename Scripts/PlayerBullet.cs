using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    [SerializeField] private float _speed = 8f;
    [SerializeField] private float _maxDistance = 8f;
    [SerializeField] private int _damage = 1;
        
    private Rigidbody2D _rigidbody;

    public int Damage => _damage;
        
    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    public void SetDamage(int damage)
    {
        _damage = damage;
    }

    private void FixedUpdate()
    {
        Vector2 moveTarget = _rigidbody.position + Vector2.up * (_speed * Time.fixedDeltaTime);
        _rigidbody.MovePosition(moveTarget);

        if (transform.position.y > _maxDistance)
        {
            Destroy(gameObject);
        }
    }  
}
