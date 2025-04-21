using UnityEngine;

public class AgentSpacialEruption : MonoBehaviour
{
   bool inRange = false;



    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("ZolaWalking"))
        {
            inRange = true;
        }
        {
            inRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("ZolaWalking"))
        {
            inRange = false;
        }
        {
            inRange = false;
        }
    }

    public void Boom()
    {
        if (inRange)
        {
            ZolaBossImitation.Instance.TakeDamage(BaseStatsForZolaBoss.spatialEruptionDamage);
        }
    }
}
