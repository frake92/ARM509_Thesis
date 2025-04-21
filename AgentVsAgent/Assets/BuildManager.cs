using UnityEngine;

public class BuildManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Application.targetFrameRate = 60;
        Screen.SetResolution(1280, 720, FullScreenMode.Windowed);
        Application.runInBackground = true;
        
    }


}
