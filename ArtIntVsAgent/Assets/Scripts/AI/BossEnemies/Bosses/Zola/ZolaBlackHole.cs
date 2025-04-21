using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.MLAgents;
using Unity.VisualScripting;
using UnityEngine;

public class ZolaBlackHole : MonoBehaviour
{
   bool isInBlackHole = false;
   private void Update()
    {
        if (ZolaImitation.Instance == null) return;
        
        if (isInBlackHole)
        {
            ZolaImitation.Instance.IsMovementDisabled = true;
            ZolaImitation.Instance.rb.constraints = RigidbodyConstraints2D.FreezeAll;
        }

        if (!isInBlackHole) {
            Vector3 blackhole = transform.position;
            blackhole = new Vector3(blackhole.x, blackhole.y - 0.5f, 0);
            Vector2 direction = blackhole - ZolaImitation.Instance.centerOfThing.transform.position;
            ZolaImitation.Instance.rb.AddForce(direction.normalized * 20f);
        }
    }

    private void OnDestroy()
    {
        if (ZolaImitation.Instance != null)
        {
            ZolaImitation.Instance.IsMovementDisabled = false;
            ZolaImitation.Instance.rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }
    
     private void OnTriggerEnter2D(Collider2D other)
    {
        ZolaImitation.Instance.animator.SetFloat("Speed", 0f);

        ZolaImitation.Instance.IsMovementDisabled = true;
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
        ZolaImitation.Instance.TakeDamage(BaseStatsForZolaBoss.voidRift);
        yield return new WaitForSeconds(0.55f); // Wait for a short duration before applying damage again
        StartCoroutine(StayInBlackHole());
        // Additional logic if needed
    }
}
