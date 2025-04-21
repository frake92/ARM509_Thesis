using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DeviceChanged : MonoBehaviour
{
    public List<GameObject> keyboard;
    public List<GameObject> gamepad;

    private void Update()
    {

            OnKeyboardInput();
        

    }

    public void OnKeyboardInput()
    {
        foreach (GameObject obj in keyboard)
        {
            obj.SetActive(true);
        }

        foreach (GameObject obj in gamepad)
        {
            obj.SetActive(false);
        }

    }

    public void OnGamepadInput()
    {
        foreach (GameObject obj in gamepad)
        {
            obj.SetActive(true);
        }

        foreach (GameObject obj in keyboard)
        {
            obj.SetActive(false);
        }
    }
}
