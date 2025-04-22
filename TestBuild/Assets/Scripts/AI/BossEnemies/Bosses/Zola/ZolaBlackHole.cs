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


    private void Update()
    {
        PlayerMovement.Instance.canMove = false;
        Vector3 blackhole = transform.position;

        blackhole = new Vector3(blackhole.x, blackhole.y - 0.5f, 0);

        Vector2 direction = blackhole - Player.Instance.centerOfPlayer.transform.position;
        PlayerMovement.Instance.rb.AddForce(direction.normalized * 20f);
    }

    private void OnDestroy()
    {
        PlayerMovement.Instance.canMove = true;
        PlayerHP.Instance.IsPlayerInBlackHole = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("PlayerWalkingTag"))
        {
            PlayerHP.Instance.IsPlayerInBlackHole = true;
            StartCoroutine(StayInBlackHole());
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("PlayerWalkingTag"))
        {
            StopAllCoroutines();
        }
    }

    IEnumerator StayInBlackHole()
    {
        yield return new WaitForSeconds(0.1f);
        PlayerHP.Instance.IsPlayerInBlackHole = true;
        PlayerHP.Instance.TakeDamage(BaseStatsForZolaBoss.voidRift);
        Player.Instance.StartCoroutine(Player.Instance.SlowPlayer(0.8f, 0.3f));
        yield return new WaitForSeconds(0.4f);
        StartCoroutine(StayInBlackHole());
    }
}
