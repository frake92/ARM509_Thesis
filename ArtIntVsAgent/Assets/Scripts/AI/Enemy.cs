using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

public abstract class Enemy : MonoBehaviour
{

    public GameObject healthBar;
    public Image healthBarFill;

    [HideInInspector]
    public int maxHP;
    private int hp;
    public float constantMovementSpeed = 4f;
    private float movementSpeed = 4f;
    public AIType AItype;
    public bool canAttack = true;
    public float attackDelay = 1.5f;

    public Transform centerOfEnemy;

    public SimpleFlash simpleFlash;

    public int HP { get => hp; set => hp = value; }
    public float MovementSpeed { get => movementSpeed; set => movementSpeed = value; }

    public int MaxHp { get => maxHP; set => maxHP = value; }

    public abstract IEnumerator Attack();

    public Animator animator;

    private void OnEnable()
    {
        StartCoroutine(afterSpawn());
    }
    public virtual IEnumerator afterSpawn()
    {
        canAttack = false;
        GetComponent<EnemyMovement>().StopEnemyMovement();
        yield return new WaitForSeconds(2f);
        canAttack = true;
        GetComponent<EnemyMovement>().isMovementDisabled = false;
    }


    //virtual hogy a bossokn�l overrideolni lehessen ott m�shogy kell majd
    public virtual void TakeDamage(int damage)
    {
        if (isDead)
            return;

        StartCoroutine(showHealthBar());
        simpleFlash.Flash();

        //mini enemy hit anim
        StartCoroutine(getHit());

        Debug.Log($"Enemy took {damage} dmg");
        HP -= damage;
        if (HP <= 0)
        {
            Die();
        }
    }
    public abstract void Logic();

    public bool isDead = false;
    public virtual void Die()
    {
        isDead = true;
        StopAllCoroutines();
        GetComponent<EnemyMovement>().StopEnemyMovement();
        StartCoroutine(startDeath());
    }

    IEnumerator startDeath()
    {
        animator.SetTrigger("die");
        Debug.Log("Enemy died");
        yield return new WaitForSeconds(0.75f);
       
        Destroy(gameObject);
    }

    public void Movement()
    {
        if(AItype == AIType.ENEMY)
        {
            constantMovementSpeed = 2f;
        }
        else if(AItype == AIType.MINIBOSS)
        {
            constantMovementSpeed = 2.5f;
        }
        else
        {
            constantMovementSpeed = 3f;
        }

    }

    public void Start()
    {
        HP = MaxHp;

    }
    public void Update()
    {
        updateHealthBar();
        //Debug.Log(canAttack);
    }

    IEnumerator showHealthBar()
    {
        StopCoroutine(showHealthBar());
        healthBar.gameObject.SetActive(true);
        yield return new WaitForSeconds(3f);
        healthBar.gameObject.SetActive(false);
    }

    IEnumerator getHit()
    {
        gameObject.GetComponent<EnemyMovement>().StopEnemyMovement();
        gameObject.GetComponent<Enemy>().canAttack = false;
        yield return new WaitForSeconds(0.25f);
        gameObject.GetComponent<EnemyMovement>().isMovementDisabled = false;
        gameObject.GetComponent<Enemy>().canAttack = true;

    }

    private void updateHealthBar()
    {
        healthBarFill.fillAmount = (float)HP / (float)MaxHp;
    }
}
