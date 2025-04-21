using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

public class PlayerShooting : MonoBehaviour
{
    public static PlayerShooting Instance;

    public GameObject bulletPrefab;
    public float bulletSpeed = 10f;

    public bool isShooting = false;

    public float cooldown = 2f;
    public bool isCooldown = false;

    public bool canShoot = true;

    private void Awake()
    {
        isShooting = false;
        Instance = this;
    }

    void Update()
    {
        if ((Mouse.current.rightButton.wasPressedThisFrame || (Gamepad.current != null && Gamepad.current.rightTrigger.wasPressedThisFrame)) && !isCooldown && !PlayerMelee.Instance.isAttacking && !PlayerMovement.Instance.isDashing && canShoot && !Player.Instance.isGettingHit && !Player.Instance.playerIsStunned)
        {
            isCooldown = true;
            StartCoroutine(Shoot());
        }
    }


    IEnumerator Shoot()
    {
        isShooting = true;
        PlayerMovement.Instance.canDash = false;

        Vector2 facingDirByCrosshair = Crosshair.Instance.crosshair.transform.position - Player.Instance.centerOfPlayer.transform.position;
        facingDirByCrosshair.Normalize();

        Player.Instance.animator.SetFloat("FacingDirectionX", facingDirByCrosshair.x);
        Player.Instance.animator.SetFloat("FacingDirectionY", facingDirByCrosshair.y);

        yield return new WaitForSeconds(0.1f);

        Player.Instance.animator.SetTrigger("shoot");

        yield return new WaitForSeconds(0.1f);
        Vector2 direction = Crosshair.Instance.crosshair.transform.position - Player.Instance.centerOfPlayer.transform.position;
        GameObject bullet = Instantiate(bulletPrefab, Player.Instance.centerOfPlayer.transform.position, Quaternion.identity);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.linearVelocity = direction.normalized * bulletSpeed;

        yield return new WaitForSeconds(0.2f);
        isShooting = false;
        isCooldown = false;
    }

    
}
