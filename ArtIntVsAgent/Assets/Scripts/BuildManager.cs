using UnityEngine;

public class BuildManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Application.runInBackground = true;
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0; // Disable VSync
        Screen.SetResolution(1280, 720, false); // Set resolution to 1280x720
    }

    public void Exit()
    {
        Application.Quit();
    }


}
