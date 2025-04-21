using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    public static Player Instance;
    public GameObject centerOfPlayer;
    public GameObject meleeCenter;

    public GameObject chronoStormObj;

    public Animator animator;

    public bool playerIsStunned = false;
    public bool isGettingHit = false;

    private void Awake()
    {
        Instance = this;
    }

    public IEnumerator SlowPlayer(float slowRate, float duration)
    {
        if (!PlayerHP.Instance.canTakeDamage)
            yield break;

        PlayerMovement.Instance.slowRate = slowRate;
        yield return new WaitForSeconds(duration);
        PlayerMovement.Instance.slowRate = 1;
    }

    public void DisablePlayerInputs()
    {
       

        //PlayerShooting.Instance.enabled = false;
        //PlayerMelee.Instance.enabled = false;
        PlayerMovement.Instance.enabled = false;
        PlayerMovement.Instance.rb.linearVelocity = Vector2.zero;
        PlayerMovement.Instance.animator.SetFloat("Speed", 0);
    }

    public void GetHit()
    {
        isGettingHit = true;
        //PlayerShooting.Instance.enabled = false;
        //PlayerMelee.Instance.enabled = false;
    }

    public void EnablePlayerInputs()
    {
        isGettingHit = false;

        PlayerMovement.Instance.enabled = true;
        PlayerShooting.Instance.enabled = true;
        PlayerMelee.Instance.enabled = true;
    }

    public IEnumerator StunPlayer()
    {
        if (!PlayerHP.Instance.canTakeDamage)
            yield break;

        Debug.Log("Player is stunned");
        playerIsStunned = true;
        DisablePlayerInputs();
        yield return new WaitForSeconds(1f);
        playerIsStunned = false;
        EnablePlayerInputs();
        Debug.Log("Player is unstunned");
    }
}
