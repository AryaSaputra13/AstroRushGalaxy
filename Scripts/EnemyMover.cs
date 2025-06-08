using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMover : MonoBehaviour
{
    public float verticalSpeed = 2f;
    public float horizontalSpeed = 1f;
    public float horizontalMoveRange = 3f;
    public float centerStopY = 0f;
    private bool hasReachedCenter = false;

    private float targetX;
    private float directionX;

    void Start()
    {
        directionX = Random.value < 0.5f ? -1f : 1f;

        directionX += Random.Range(-0.3f, 0.3f);
    }

    void Update()
    {
        if (hasReachedCenter) return;

        float horizontalMovement = Mathf.Sin(Time.time * horizontalSpeed) * directionX;

        transform.position += new Vector3(horizontalMovement, -verticalSpeed, 0f) * Time.deltaTime;

        if (transform.position.y <= centerStopY)
        {
            hasReachedCenter = true;
            BossCutSceneManager cutScene = FindObjectOfType<BossCutSceneManager>();
            if (cutScene != null)
            {
                cutScene.StartTransition();
            }
        }
    }
}
