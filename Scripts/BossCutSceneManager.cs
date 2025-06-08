using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class BossCutSceneManager : MonoBehaviour
{
    public GameObject boss;
    public GameObject player;
    public GameObject explosionEffect;
    public GameObject chatBubble1;
    public GameObject chatBubble2;
    public GameObject enemyPrefab;
    public Transform enemySpawnArea;
    public Image blackOverlay;
    public AudioClip explosionSound;
    private AudioSource audioSource;


    private bool transitionStarted = false;

    void Start()
    {
        chatBubble1.SetActive(false);
        chatBubble2.SetActive(false);
        blackOverlay.color = new Color(0, 0, 0, 0);
        audioSource = GetComponent<AudioSource>();
        StartCoroutine(PlayCutScene());
    }

    IEnumerator PlayCutScene()
    {
        yield return StartCoroutine(ShakeObject(boss, 1.5f, 0.1f));
        GameObject explosion = Instantiate(explosionEffect, boss.transform.position, Quaternion.identity);
        Destroy(explosion, 2f);

        if (explosionSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(explosionSound);
        }

        yield return new WaitForSeconds(0.3f);
        Destroy(boss);

        yield return ShowChatBubble(chatBubble1, 2f);
        yield return ShowChatBubble(chatBubble2, 2f);

        // Spawn enemies
        for (int i = 0; i < 70; i++)
        {
            Vector2 spawnPos = (Vector2)enemySpawnArea.position + Random.insideUnitCircle * 5f;
            GameObject enemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            //enemy.GetComponent<EnemyMover>().target = player.transform;
        }
    }

    IEnumerator ShowChatBubble(GameObject bubble, float duration)
    {
        bubble.SetActive(true);
        yield return new WaitForSeconds(duration);
        bubble.SetActive(false);
    }

    IEnumerator ShakeObject(GameObject obj, float duration, float magnitude)
    {
        Vector3 originalPos = obj.transform.position;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            obj.transform.position = originalPos + new Vector3(x, y, 0);
            elapsed += Time.deltaTime;

            yield return null;
        }

        obj.transform.position = originalPos;
    }

    public void StartTransition()
    {
        if (!transitionStarted)
        {
            transitionStarted = true;
            StartCoroutine(TransitionToMenu());
        }
    }

    IEnumerator TransitionToMenu()
    {
        float duration = 1.5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float alpha = Mathf.Lerp(0, 3, elapsed / duration);
            blackOverlay.color = new Color(0, 0, 0, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }

        SceneManager.LoadScene("Menu");
    }
}
