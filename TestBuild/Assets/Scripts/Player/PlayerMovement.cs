using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement Instance;

    public float basicMoveSpeed = 3.5f;
    public float currentMoveSpeed = 3.5f;
    public float dashSpeed = 6f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    [HideInInspector]
    public bool isDashing = false;
    [HideInInspector]
    public bool canDash = true;
    [HideInInspector]
    public bool canMove = true;

    public Rigidbody2D rb;
    [HideInInspector]
    public Vector2 movement;
    [HideInInspector]
    public bool isWalking = false;
    public Animator animator;
    public Vector2 facingDirection;

    public GameObject walkingHitbox;

    public bool canTpByDoor = true;
    public float slowRate = 1;

    private void Awake()
    {
        Instance = this;
        Cursor.visible = false;
    }

    void Update()
    {
        if (!PlayerShooting.Instance.isShooting)
        {
            animator.SetFloat("Horizontal", movement.x);
            animator.SetFloat("Vertical", movement.y);
        }
        animator.SetFloat("Speed", movement.sqrMagnitude);
        movement = Vector2.zero;
        float movementSpeed = currentMoveSpeed * slowRate;

        if (!isDashing && canMove)
        {
            // Player movement input
            movement = new Vector2(0, 0);

            //fel
            if (Keyboard.current.wKey.isPressed ||
                (Gamepad.current != null && Gamepad.current.leftStick.ReadValue().y > 0.2))
            {
                //horizont�lisan n�z
                if (Gamepad.current != null && Mathf.Abs(Gamepad.current.leftStick.ReadValue().x) >= Mathf.Abs(Gamepad.current.leftStick.ReadValue().y) - 0.2f)
                {
                    movement.y = movementSpeed - 0.001f;
                }
                else 
                    movement.y = movementSpeed;

                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            }
            //le
            else if (Keyboard.current.sKey.isPressed || (Gamepad.current != null && Gamepad.current.leftStick.ReadValue().y < -0.2))
            {
                if (Gamepad.current != null && Mathf.Abs(Gamepad.current.leftStick.ReadValue().x) >= Mathf.Abs(Gamepad.current.leftStick.ReadValue().y) - 0.2f)
                {
                    movement.y = -movementSpeed + 0.001f;
                }
                else
                    movement.y = -movementSpeed;
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            }
            else
            {
                movement.y = 0;
            }

            //jobbra
            if (Keyboard.current.dKey.isPressed || 
                (Gamepad.current != null && Gamepad.current.leftStick.ReadValue().x > 0.2))
            {
                movement.x = movementSpeed;
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            }
            //balra
            else if (Keyboard.current.aKey.isPressed || 
                (Gamepad.current != null && Gamepad.current.leftStick.ReadValue().x < -0.2))
            {
                movement.x = -movementSpeed;
                rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            }
            //idle
            else 
            {
                movement.x = 0;
            }

            if ((Keyboard.current.leftShiftKey.wasPressedThisFrame && canDash && movement != Vector2.zero))
            {
                StartCoroutine(Dash());
            }
        }

        if (!isDashing)
            rb.linearVelocity = movement.normalized * movementSpeed;

        if (movement != Vector2.zero)
        {
            

            if (Mathf.Abs(movement.x) > Mathf.Abs(movement.y) && !PlayerShooting.Instance.isShooting)
            {
                facingDirection = new Vector2(Mathf.Sign(movement.x), 0f);
                animator.SetFloat("FacingDirectionX", movement.x);
                animator.SetFloat("FacingDirectionY", 0);
            }
            else
            {
                if (!PlayerShooting.Instance.isShooting)
                {
                    facingDirection = new Vector2(0f, Mathf.Sign(movement.y));
                    animator.SetFloat("FacingDirectionX", 0);
                    animator.SetFloat("FacingDirectionY", movement.y);
                }
            }
        }
            
    }

    private IEnumerator Dash()
    {
        PlayerHP.Instance.canTakeDamage = false;
        PlayerMelee.Instance.canMelee = false;
        PlayerShooting.Instance.canShoot = false;

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        canDash = false;
        isDashing = true;
        currentMoveSpeed = dashSpeed;
        rb.linearVelocity = movement.normalized * currentMoveSpeed;

        walkingHitbox.layer = LayerMask.NameToLayer("IgnoreEnemy");

        animator.SetTrigger("dash");

        yield return new WaitForSeconds(dashDuration);
        yield return new WaitForSeconds(0.25f);

        currentMoveSpeed = basicMoveSpeed;
        isDashing = false;
        walkingHitbox.layer = LayerMask.NameToLayer("PlayerWalking");
        PlayerHP.Instance.canTakeDamage = true;
        PlayerMelee.Instance.canMelee = true;
        PlayerShooting.Instance.canShoot = true;
        yield return new WaitForSeconds(dashCooldown);

        canDash = true;
    }

    public void StopMovement()
    {
        rb.linearVelocity = Vector2.zero;
        animator.SetFloat("Speed", 0);
    }

    public IEnumerator ResetTpByDoor()
    {
        yield return new WaitForSeconds(0.5f);
        canTpByDoor = true;
    }
}
