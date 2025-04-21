using UnityEngine;
using System.Collections;

public class AgentBlackHole : MonoBehaviour
{
    bool isInBlackHole = false;
    private void Update()
    {
        if (ZolaBossImitation.Instance == null) return;
        
        if (isInBlackHole)
        {
            ZolaBossImitation.Instance.enemyMovement.isMovementDisabled = true;
            ZolaBossImitation.Instance.enemyMovement.rb.constraints = RigidbodyConstraints2D.FreezeAll;
            // Continuously ensure animation is stopped when in black hole
            ZolaBossImitation.Instance.animator.SetFloat("Speed", 0f);
        }
        
        if (!isInBlackHole){
            Vector3 blackhole = transform.position;
            blackhole = new Vector3(blackhole.x, blackhole.y - 0.5f, 0);
            Vector2 direction = blackhole - ZolaBossImitation.Instance.centerOfEnemy.transform.position;
            ZolaBossImitation.Instance.enemyMovement.rb.AddForce(direction.normalized * 20f);
        }
    }

    private void OnDestroy()
    {
        if (ZolaBossImitation.Instance != null)
        {
            ZolaBossImitation.Instance.enemyMovement.isMovementDisabled = false;
            ZolaBossImitation.Instance.enemyMovement.rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {   
        if (ZolaBossImitation.Instance == null) return;
        
        if (other.gameObject.CompareTag("ZolaWalking"))
        {
            // Force animation to stop completely
            ZolaBossImitation.Instance.enemyMovement.isMovementDisabled = true;
            ZolaBossImitation.Instance.animator.SetFloat("Speed", 0f);
            ZolaBossImitation.Instance.animator.Rebind(); // Reset animation state
            
            isInBlackHole = true;
            StartCoroutine(StayInBlackHole());
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("ZolaWalking"))
        {
            isInBlackHole = false;
            StopAllCoroutines();
        }
    }
    
    IEnumerator StayInBlackHole()
    {
        yield return new WaitForSeconds(0.1f);
        ZolaBossImitation.Instance.TakeDamage(BaseStatsForZolaBoss.voidRift);
        
        // Ensure animation is stopped each time damage is applied
        if (ZolaBossImitation.Instance != null) {
            ZolaBossImitation.Instance.animator.SetFloat("Speed", 0f);
        }
        
        yield return new WaitForSeconds(0.55f); // Wait for a short duration before applying damage again
        StartCoroutine(StayInBlackHole()); // Restart the coroutine for continuous damage
    }
}
