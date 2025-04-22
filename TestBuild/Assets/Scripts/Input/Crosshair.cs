using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Crosshair : MonoBehaviour
{
    public static Crosshair Instance;

    public GameObject crosshair;

    private Vector3 crosshairPosition;

    private void Awake()
    {
        Instance = this;
        crosshairPosition = crosshair.transform.position;
    }

    private void Update()
    {

            if (Mouse.current.delta.ReadValue().magnitude > 0.1f)
            {
                StopAllCoroutines();
                crosshair.SetActive(true);
                StartCoroutine(disable());
            }

            MoveCrosshairWithMouse();
        

        crosshairPosition = ClampToMainCameraBounds(crosshairPosition);

        crosshair.transform.position = crosshairPosition;
    }

    private void MoveCrosshairWithMouse()
    {
        Vector3 mousePosition = Mouse.current.position.ReadValue();

        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, Camera.main.nearClipPlane));

        worldPosition.z = 0f;

        crosshairPosition = worldPosition;
    }

    private void MoveCrosshairWithGamepad()
    {
        // Get the right stick input
        Vector2 rightStickInput = Gamepad.current.rightStick.ReadValue();

        float speed = 7.5f;

        crosshairPosition += new Vector3(rightStickInput.x, rightStickInput.y, 0f) * speed * Time.deltaTime;
    }

    private Vector3 ClampToMainCameraBounds(Vector3 position)
    {
        Camera cam = Camera.main;
        float camHeight = 2f * cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        float minX = cam.transform.position.x - camWidth / 2f;
        float maxX = cam.transform.position.x + camWidth / 2f;
        float minY = cam.transform.position.y - camHeight / 2f;
        float maxY = cam.transform.position.y + camHeight / 2f;

        float clampedX = Mathf.Clamp(position.x, minX, maxX);
        float clampedY = Mathf.Clamp(position.y, minY, maxY);

        return new Vector3(clampedX, clampedY, position.z);
    }

    IEnumerator disable()
    {
        yield return new WaitForSeconds(1.5f);
        crosshair.SetActive(false);
    }
}
