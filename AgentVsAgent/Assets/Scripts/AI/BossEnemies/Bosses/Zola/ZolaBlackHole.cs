using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

public class ZolaBlackHole : MonoBehaviour
{

    bool isInBlackHole = false;
    public ZolaRLAgent opponent;

    private void Update()
    {
        if (opponent == null) return;
        
        if (isInBlackHole)
        {
            opponent.IsMovementDisabled = true;
            opponent.movement.rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        if (!isInBlackHole) {
            Vector3 blackhole = transform.position;
            blackhole = new Vector3(blackhole.x, blackhole.y - 0.5f, 0);
            Vector2 direction = blackhole - opponent.centerOfThing.transform.position;
            opponent.movement.rb.AddForce(direction.normalized * 20f);
        }
    }

    private void OnDestroy()
    {
        if (opponent != null)
        {
            opponent.CanJump = true;
            opponent.IsMovementDisabled = false;
            opponent.movement.rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if(opponent == null) return;
        opponent.CanJump = false;
        opponent.animation.animator.SetFloat("Speed", 0f);

        opponent.IsMovementDisabled = true;
        if (other.gameObject.CompareTag("PlayerWalkingTag"))
        {
            isInBlackHole = true;
            StartCoroutine(StayInBlackHole());
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("PlayerWalkingTag"))
        {
            isInBlackHole = false;
            StopAllCoroutines();
        }
    }
    
    IEnumerator StayInBlackHole()
    {
        yield return new WaitForSeconds(0.1f);
        opponent.TakeDamage(BaseStatsForZolaBoss.voidRift);
        yield return new WaitForSeconds(0.55f); // Wait for a short duration before applying damage again
        StartCoroutine(StayInBlackHole());
        // Additional logic if needed
    }
}
