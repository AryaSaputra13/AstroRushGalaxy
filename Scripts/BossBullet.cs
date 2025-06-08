using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossBullet : MonoBehaviour
{
   [SerializeField] private float _speed = 5f;
   [SerializeField] private float _lifetime = 5f;
   [SerializeField] private int _damage = 2;

   private Vector2 _direction;
   private Rigidbody2D _rigidbody;
   private float _spawnTime;

   private void Awake()
   {
       _rigidbody = GetComponent<Rigidbody2D>();
       _spawnTime = Time.time;
   }

   public void SetDirection(Vector2 direction)
   {
       _direction = direction.normalized;
   }

   private void FixedUpdate()
   {
       _rigidbody.velocity = _direction * _speed;

       if (Time.time > _spawnTime + _lifetime)
       {
           Destroy(gameObject);
       }
   }

   private void OnTriggerEnter2D(Collider2D collision)
   {
       if (collision.CompareTag("Player"))
       {
           Player player = collision.GetComponent<Player>();
           if (player != null)
           {
               player.TakeDamage(_damage);
           }
           Destroy(gameObject);
       }
    }
}
