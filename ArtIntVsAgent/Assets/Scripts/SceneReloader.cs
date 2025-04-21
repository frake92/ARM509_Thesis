using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneReloader : MonoBehaviour
{
    private static SceneReloader instance;
    private static bool isReloading = false;
    private float delay = 0;
    private string sceneToLoad = "";
    private float timer = 0;

    public static bool IsReloading => isReloading;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        if (isReloading)
        {
            timer += Time.unscaledDeltaTime;
            if (timer >= delay)
            {
                PerformReload();
            }
        }
    }

    private void PerformReload()
    {
        Time.timeScale = 1.0f;
        SceneManager.LoadScene(sceneToLoad);
        isReloading = false;
        timer = 0;
    }

    // Static method to initiate scene reload with delay
    public static void ReloadScene(string sceneName, float delayTime)
    {
        if (isReloading) return;

        if (instance == null)
        {
            GameObject reloaderObject = new GameObject("SceneReloader");
            instance = reloaderObject.AddComponent<SceneReloader>();
            DontDestroyOnLoad(reloaderObject);
        }

        instance.sceneToLoad = sceneName;
        instance.delay = delayTime;
        instance.timer = 0;
        isReloading = true;

        // Pause the game during the reload delay
        Time.timeScale = 0.0f;
        
        Debug.Log($"Scene reload initiated: {sceneName} with delay {delayTime}s");
    }
}