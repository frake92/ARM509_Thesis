using UnityEngine;
using System.Collections.Generic;

public class VoidRiftController : MonoBehaviour
{
    private float slowFactor = 0.5f; // Player moves at 50% normal speed
    private List<GameObject> affectedPlayers = new List<GameObject>();
    private Dictionary<GameObject, float> originalSlowRates = new Dictionary<GameObject, float>();

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerMovement playerMovement = collision.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                // Save the current slow rate before modifying it
                originalSlowRates[collision.gameObject] = playerMovement.slowRate;
                
                // Apply slow effect directly to player's slowRate
                playerMovement.slowRate = slowFactor;
                affectedPlayers.Add(collision.gameObject);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerMovement playerMovement = collision.GetComponent<PlayerMovement>();
            if (playerMovement != null)
            {
                // Restore original slow rate when player exits
                if (originalSlowRates.ContainsKey(collision.gameObject))
                {
                    playerMovement.slowRate = originalSlowRates[collision.gameObject];
                    originalSlowRates.Remove(collision.gameObject);
                }
                else
                {
                    // Fallback to normal speed if we somehow don't have the original
                    playerMovement.slowRate = 1f;
                }
                
                affectedPlayers.Remove(collision.gameObject);
            }
        }
    }

    private void OnDestroy()
    {
        // Restore original slow rates for all affected players
        foreach (GameObject player in affectedPlayers)
        {
            if (player != null)
            {
                PlayerMovement playerMovement = player.GetComponent<PlayerMovement>();
                if (playerMovement != null)
                {
                    // Restore original slow rate
                    if (originalSlowRates.ContainsKey(player))
                    {
                        playerMovement.slowRate = originalSlowRates[player];
                    }
                    else
                    {
                        // Fallback to normal speed
                        playerMovement.slowRate = 1f;
                    }
                }
            }
        }
        
        // Clear collections
        affectedPlayers.Clear();
        originalSlowRates.Clear();
    }
}