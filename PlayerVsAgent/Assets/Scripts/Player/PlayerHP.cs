using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.MLAgents;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerHP : MonoBehaviour
{
    public static PlayerHP Instance;
    public bool IsPlayerInBlackHole = false;
    public Slider hpBar;
   
    public int maxHP = 150;
    public int currentHP = 150;

    public float damageToTake = 1f;

    public bool canTakeDamage = true;

    public SimpleFlash simpleFlash;


    

    private void Awake()
    {
        Instance = this;
    }
    public void TakeDamage(int damage)
    {
        if(!canTakeDamage)
        {
            return;
        }

        PlayerPrefs.SetInt("damage", 1);
        currentHP -= (int)(damage * damageToTake);
        hpBar.value = currentHP / (float)maxHP;
       
        simpleFlash.Flash();

        if (currentHP <= 0)
        {
            PlayerPrefs.SetInt("deaths", 1);
            currentHP = 0;
            hpBar.value = 0;
            StartCoroutine(startGameOver());
        }
    }
    public IEnumerator startGameOver()
    {
        // Find all SimpleAgent instances and call EndEpisode on them
        var agent = ZolaRLAgent.Instance;

        agent.EndEpisode();

        
        // Then reload the scene
        SceneManager.LoadScene("Test-train");
        yield break;
    }

    public IEnumerator disableInputs()
    {
        if (Player.Instance.playerIsStunned)
            yield break;

        Player.Instance.GetHit();

        if (!PlayerShooting.Instance.isShooting && !PlayerMelee.Instance.isAttacking && !PlayerMovement.Instance.isDashing)
        {
            Player.Instance.animator.SetTrigger("hit");
        }

        yield return new WaitForSeconds(0.25f);
        if (Player.Instance.playerIsStunned)
            yield break;
        Player.Instance.EnablePlayerInputs();
    }
}
