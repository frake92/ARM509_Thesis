using UnityEngine;

public class AgentTemporal : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Enemy"))
        {
            ZolaImitation.Instance.TakeDamage(BaseStatsForZolaBoss.temporalSurgeDamage);
        }
    }
}
