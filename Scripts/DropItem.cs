using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DropItem : MonoBehaviour
{
    public float lifeTime = 2f;
    public float blinkTime = 1f;

    private SpriteRenderer spriteRenderer;
    private bool isBlinking = false;

    private void Start()
    {
        Debug.Log("DropItem spawned");
        spriteRenderer = GetComponent<SpriteRenderer>();
        StartCoroutine(DropLifeCycle());
    }

    private IEnumerator DropLifeCycle()
    {
        yield return new WaitForSeconds(lifeTime);
        StartCoroutine(BlinkAndDestroy());
    }

    private IEnumerator BlinkAndDestroy()
    {
        isBlinking = true;
        float blinkRate = 0.2f;
        float timer = 0f;

        while (timer < blinkTime)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(blinkRate);
            timer += blinkRate;
        }

        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            collision.GetComponent<Player>().UpgradeBullet();
            Destroy(gameObject);
        }
    }
}
