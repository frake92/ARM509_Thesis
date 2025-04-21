using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AgentVoidRift : MonoBehaviour
{
    private float slowFactor = 0.5f; // Player moves at 50% normal speed
    private List<GameObject> affectedPlayers = new List<GameObject>();
    private Dictionary<GameObject, float> originalSlowRates = new Dictionary<GameObject, float>();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            EnemyMovement enemyMovement = collision.GetComponent<EnemyMovement>();
            if (enemyMovement != null)
            {
                // Save the current slow rate before modifying it
                //originalSlowRates[collision.gameObject] = enemyMovement.slowRate;
                
                // Apply slow effect directly to player's slowRate
                //enemyMovement.slowRate = slowFactor;
                affectedPlayers.Add(collision.gameObject);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            EnemyMovement enemyMovement = collision.GetComponent<EnemyMovement>();
            if (enemyMovement != null)
            {
                // Restore original slow rate when player exits
                if (originalSlowRates.ContainsKey(collision.gameObject))
                {
                    //playerMovement.slowRate = originalSlowRates[collision.gameObject];
                    originalSlowRates.Remove(collision.gameObject);
                }
                /*
                else
                {
                    // Fallback to normal speed if we somehow don't have the original
                    playerMovement.slowRate = 1f;
                }*/
                
                affectedPlayers.Remove(collision.gameObject);
            }
        }
    }

    private void OnDestroy()
    {
        // Clear collections
        affectedPlayers.Clear();
        originalSlowRates.Clear();
    }
}
