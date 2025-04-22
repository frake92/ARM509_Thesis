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
   

    public int hpPotions = 2;
    public TextMeshProUGUI hpPotionsText;
    public int maxHP = 150;
    public int currentHP = 150;

    public float damageToTake = 1f;

    public bool canTakeDamage = true;

    public SimpleFlash simpleFlash;
    
    private void Awake()
    {
        Instance = this;
        hpPotionsText.text = ":" + hpPotions.ToString();
    }

    void Update()
    {
            if(Keyboard.current.hKey.wasPressedThisFrame)
            {
                if(hpPotions > 0)
                {
                    hpPotions--;
                    currentHP += maxHP / 2;
                    if (currentHP > maxHP)
                    {
                        currentHP = maxHP;
                    }
                    hpBar.value = currentHP / (float)maxHP;
                    hpPotionsText.text = ":" + hpPotions.ToString();
                }
            }
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
            BuildManager.Instance.GameOver();
            BuildManager.Instance.gameOverText.text = "Game Over! Zola legyőzött!";
        }
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
