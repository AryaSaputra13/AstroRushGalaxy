using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public Button startButton;
    public GameObject playerPlane;
    public float moveSpeed = 5f;
    public string sceneToLoad = "Gameplay";

    public CanvasGroup menuGroup;
    public CanvasGroup titleGroup;
    public CanvasGroup cloud1Group;
    public CanvasGroup cloud2Group;

    private bool isLaunching = false;
    private Camera mainCam;

    void Start()
    {
        startButton.onClick.AddListener(OnStartGame);
        mainCam = Camera.main;
    }

    void Update()
    {
        if (isLaunching)
        {
            playerPlane.transform.Translate(Vector3.up * moveSpeed * Time.deltaTime);

            Vector3 viewportPos = mainCam.WorldToViewportPoint(playerPlane.transform.position);

            if (viewportPos.y > 1.1f)
            {
                SceneManager.LoadScene(sceneToLoad);
            }
        }
    }

    void OnStartGame()
    {
        isLaunching = true;
        startButton.interactable = false;
        StartCoroutine(FadeOutUI());
    }

    IEnumerator FadeOutUI()
    {
        float duration = 1.5f;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, t / duration);

            menuGroup.alpha = alpha;
            titleGroup.alpha = alpha;
            cloud1Group.alpha = alpha;
            cloud2Group.alpha = alpha;

            yield return null;
        }

            menuGroup.alpha = 0;
            titleGroup.alpha = 0;
            cloud1Group.alpha = 0;
            cloud2Group.alpha = 0;

            menuGroup.interactable = false;
            menuGroup.blocksRaycasts = false;
            titleGroup.interactable = false;
            titleGroup.blocksRaycasts = false;
            cloud1Group.interactable = false;
            cloud1Group.blocksRaycasts = false;
            cloud2Group.interactable = false;
            cloud2Group.blocksRaycasts = false;
        }

}
