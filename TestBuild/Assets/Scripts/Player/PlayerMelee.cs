using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMelee : MonoBehaviour
{
    public static PlayerMelee Instance;
    public bool canMelee = true;
    public bool isAttacking = false;
    public LayerMask enemyLayer;

    private void Awake()
    {
        isAttacking = false;
        Instance = this;
    }

    void Update()
    {
        if ((Mouse.current.leftButton.wasPressedThisFrame || (Gamepad.current != null && Gamepad.current.buttonWest.wasPressedThisFrame)) && canMelee &&!PlayerShooting.Instance.isShooting && !PlayerMovement.Instance.isDashing && !Player.Instance.isGettingHit && !Player.Instance.playerIsStunned)
        {
            StartCoroutine(Melee());
        }
    }

    IEnumerator Melee()
    {
        isAttacking = true;
        canMelee = false;
        PlayerMovement.Instance.canDash = false;
        Player.Instance.animator.SetTrigger("melee");

        yield return new WaitForSeconds(0.15f);

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(Player.Instance.meleeCenter.transform.position, 1.15f, enemyLayer);

        foreach (Collider2D agent in hitEnemies)
        {
            if (agent.CompareTag("Enemy"))
            {
                agent.GetComponent<ZolaRLAgent>().TakeDamage(BaseStatsForPlayer.meleeDamage);
            }
        }

        StartCoroutine(MeleeCooldown());
    }

    IEnumerator MeleeCooldown()
    {
        yield return new WaitForSeconds(0.05f);
        isAttacking = false;
        PlayerMovement.Instance.canDash = true;
        yield return new WaitForSeconds(0.2f);
        canMelee = true;
    }

    private void OnDrawGizmosSelected()
    {
        if (Player.Instance == null)
        {
            return;
        }

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(Player.Instance.meleeCenter.transform.position, 1.15f);
    }
}
