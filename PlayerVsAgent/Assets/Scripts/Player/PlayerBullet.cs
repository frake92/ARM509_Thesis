using System.Collections;
using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    public float lifeTime = 5f;

    bool isValid = false;
    void Start()
    {
        StartCoroutine(delay());
        Destroy(gameObject, lifeTime);
    }

    IEnumerator delay()
    {
        yield return new WaitForSeconds(0.015f);
        isValid = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isValid)
            return;

        if (other.gameObject.CompareTag("Player") || other.gameObject.CompareTag("BlackHole") || other.gameObject.CompareTag("LoreTrigger") || other.gameObject.CompareTag("ClockOBomb") || other.gameObject.CompareTag("DoorTrigger") || other.gameObject.CompareTag("resource") || other.gameObject.CompareTag("roomTrigger") || other.gameObject.CompareTag("enemyBullet"))
        {
            return;
        }

        Destroy(gameObject);

        if (other.gameObject.CompareTag("Enemy"))
        {
            other.gameObject.GetComponent<ZolaRLAgent>().TakeDamage(BaseStatsForPlayer.rangedDamage);
        }
    }
}
