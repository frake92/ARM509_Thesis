using UnityEngine;

public class AgentDimensionaé : MonoBehaviour
{
public bool inRange = false;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            inRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            inRange = false;
        }
    }
}
