using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ZolaBossImitation : Enemy, BossBehaviours
{
    #region fields
    private static ZolaBossImitation instance;
    public static ZolaBossImitation Instance => instance;
    public Slider hpBar;
    public EnemyMovement enemyMovement;
    public bool canJump = true;
    double randomNUM = 0;
    System.Random rnd = new System.Random();
    List<GameObject> gamobjectsToDestroyAfterDeath = new List<GameObject>();
    int wasBuffed = 0;

    private float agentHealthThreshold = 0.5f;
    private bool isAgentLowHealth = false;
    public LayerMask agentLayer;

    [Header("Target Agent")]
    [SerializeField] private ZolaImitation targetAgent;
    [Header("Attack Prefabs")]
    public GameObject voidRiftPrefab;
    public GameObject spatialEruptionPrefabCyberpunk;
    public GameObject temporalSurgePrefabCzberpunk;
    public Transform temporalPoint;
    public Transform spatialPoint;
    public GameObject healEffect;
    public GameObject shadowPrefab;
    public BoxCollider2D walkingCollider;
    public BoxCollider2D normalCollider;
    public AethericStrike aethericStrike;
    public DimensionalWave dimensionalWave;

    public Transform dimensionalPoint;

    public GameObject dimensionalStartCyberpunk;
    public GameObject dimensionalEndCyberpunk;
    #endregion

    #region Help methods
    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
    }
    private void UpdateFacingDirection()
    {
        
        if (ZolaImitation.Instance.centerOfThing.transform.position.x <= centerOfEnemy.position.x)
        {
            animator.SetFloat("FacingDirectionX", -1);
            animator.GetComponent<SpriteRenderer>().flipX = false;
        }
        else
        {
            animator.SetFloat("FacingDirectionX", 1);
            animator.GetComponent<SpriteRenderer>().flipX = true;
        }
    }
    public IEnumerator ResetCanAttack()
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(0.2f, 1.5f));
        canAttack = true;
    }
    
    IEnumerator RandomNum()
    {
        randomNUM = rnd.NextDouble();
        yield return new WaitForSeconds(0.2f);
        StartCoroutine(RandomNum());
    }
    
    public void CheckPlayerHealth()
    {
        if (targetAgent == null) return;
        
        float agentHealthPercentage = targetAgent.HP / 500f; 
        isAgentLowHealth = agentHealthPercentage <= agentHealthThreshold;
    }
    #endregion
    
    public override void Logic()
    {
        if (wasBuffed == 0)
        {
            BaseStatsForZolaBoss.BuffDamages(5);
            base.MovementSpeed = 3;
            base.constantMovementSpeed = 3;
            wasBuffed = 1;
        }
        NormalLogic();
    }
    
    public void AgressiveLogic()
    {
        if (wasBuffed == 0)
        {
            BaseStatsForZolaBoss.BuffDamages(5);
            base.MovementSpeed = 3;
            base.constantMovementSpeed = 3;
            wasBuffed = 1;
        }
        NormalLogic();
    }
    
    public void NormalLogic()
    {
        if (!isAgentLowHealth)
        {
            wasBuffed = 0;
            base.constantMovementSpeed = 2.5f;
            base.MovementSpeed = 2.5f;
            BaseStatsForZolaBoss.BuffDamages(-5);
        }

        if (canAttack && targetAgent != null)
        {
            float currentDistance = Vector3.Distance(transform.position, targetAgent.transform.position);
            if (currentDistance <= enemyMovement.meleeDistance)
            {
                if (randomNUM >= 0.0 && randomNUM < 0.30)
                {
                    StartCoroutine(SpatialEruption());
                }
                else if (randomNUM >= 0.30 && randomNUM < 0.65)
                {
                    StartCoroutine(Attack());
                }
                else if (randomNUM >= 0.65 && randomNUM < 0.80)
                {
                    StartCoroutine(TemporalSurge());
                }
                else if (randomNUM >= 0.80)
                {
                    if (randomNUM <= 0.81 && canJump)
                    {
                        StartCoroutine(DimensionalWave());
                    }

                }
            }
            else if (currentDistance > enemyMovement.meleeDistance && currentDistance <= enemyMovement.rangedDistance)
            {
                if (randomNUM >= 0 && randomNUM < 0.30)
                {
                    StartCoroutine(VoidRift());
                }
                else if (randomNUM >= 0.30 && randomNUM < 0.40 && canJump)
                {
                    StartCoroutine(DimensionalWave());
                }

            }
            else if (currentDistance > enemyMovement.rangedDistance)
            {
                if (randomNUM >= 0.8)
                {
                    if (randomNUM <= 0.81 && canJump)
                    {
                        StartCoroutine(DimensionalWave());
                    }

                }
            }
        }
    }

    #region Moveset
    public override IEnumerator Attack()
    {
        base.canAttack = false;

        enemyMovement.StopEnemyMovement();
        animator.SetTrigger("aethericStrike");
        yield return new WaitForSeconds(0.15f);
        
        yield return new WaitForSeconds(0.45f);

        
        float distanceToAgent = Vector3.Distance(transform.position, targetAgent.transform.position);
        
        if ((aethericStrike.inRange || distanceToAgent < 2.5f) && targetAgent != null)
        {
            targetAgent.TakeDamage(BaseStatsForZolaBoss.aethricStirkeDamage);
            Debug.Log($"ZolaBoss: Aetheric Strike hit agent at distance {distanceToAgent:F2}");
        }
        else
        {
            Debug.Log($"ZolaBoss: Aetheric Strike missed! inRange={aethericStrike.inRange}, distance={distanceToAgent:F2}");
        }

        yield return new WaitForSeconds(1f);
        enemyMovement.isMovementDisabled = false;
        Debug.Log("Aetheric strike over");
        StartCoroutine(ResetCanAttack());
    }
    public IEnumerator SpatialEruption()
    {
        base.canAttack = false;
        enemyMovement.StopEnemyMovement();

        
        animator.SetTrigger("spatialEruption");
        yield return new WaitForSeconds(0.55f);
       
        GameObject eruption;
        eruption = Instantiate(spatialEruptionPrefabCyberpunk, spatialPoint.position, Quaternion.identity);

        gamobjectsToDestroyAfterDeath.Add(eruption);

        yield return new WaitForSeconds(0.1f);
         yield return new WaitForSeconds(0.35f);
        enemyMovement.isMovementDisabled = false;
        StartCoroutine(ResetCanAttack());

        yield return new WaitForSeconds(0.2f);
        eruption.GetComponent<SpatialEruption>().Boom();

        if (targetAgent != null)
        {
            float distanceToAgent = Vector3.Distance(transform.position, targetAgent.transform.position);
            if (distanceToAgent < 3f) 
            {
                targetAgent.TakeDamage(BaseStatsForZolaBoss.spatialEruptionDamage);
            }
        }

        yield return new WaitForSeconds(0.25f);
        Destroy(eruption);
    }

    public IEnumerator TemporalSurge()
    {

        base.canAttack = false;
        enemyMovement.StopEnemyMovement();

        animator.SetTrigger("temporalSurge");
        yield return new WaitForSeconds(0.55f);

        GameObject surge;
        surge = Instantiate(temporalSurgePrefabCzberpunk, temporalPoint.position, Quaternion.identity);

        yield return new WaitForSeconds(0.4f);
        healEffect.SetActive(true);
        base.HP += (int)(base.MaxHp * 0.025f);
        if (base.HP > base.maxHP)
            base.HP = base.maxHP;
        yield return new WaitForSeconds(0.5f);
        base.HP += (int)(base.MaxHp * 0.025f);
        if (base.HP > base.maxHP)
            base.HP = base.maxHP;
        yield return new WaitForSeconds(0.5f);
        base.HP += (int)(base.MaxHp * 0.025f);
        if (base.HP > base.maxHP)
            base.HP = base.maxHP;
        yield return new WaitForSeconds(0.5f);
        healEffect.SetActive(false);

        surge.transform.GetChild(0).GetComponent<Animator>().SetTrigger("ki");
        surge.transform.GetChild(1).GetComponent<Animator>().SetTrigger("ki");

        yield return new WaitForSeconds(0.35f);
        Destroy(surge);

        yield return new WaitForSeconds(0.25f);

        enemyMovement.isMovementDisabled = false;
        StartCoroutine(ResetCanAttack());
    }

    public IEnumerator VoidRift()
    {
        base.canAttack = false;
        enemyMovement.StopEnemyMovement();

        animator.SetTrigger("voidRift");

        yield return new WaitForSeconds(0.45f);

        Vector3 targetPosition;
        if (targetAgent != null)
        {
            targetPosition = targetAgent.transform.position + new Vector3(Random.Range(-2f, 2f), Random.Range(-2f, 2f), 0);
        }
        else
        {
            Transform target = RoomManager.Instance != null ? 
                RoomManager.Instance.currentRoom.voidRiftPoints[Random.Range(0, RoomManager.Instance.currentRoom.voidRiftPoints.Count)].transform :
                transform;
            targetPosition = target.position;
        }

        GameObject blackHole = Instantiate(voidRiftPrefab, targetPosition, Quaternion.identity);
        gamobjectsToDestroyAfterDeath.Add(blackHole);
        
        Destroy(blackHole.GetComponent<ZolaBlackHole>()); 
        blackHole.AddComponent<ZolaBlackHole>(); 

        yield return new WaitForSeconds(1.5f);
        enemyMovement.isMovementDisabled = false;
        StartCoroutine(ResetCanAttack());
        Debug.Log("BlackHole over");

        yield return new WaitForSeconds(2.5f);
        if (blackHole != null && blackHole.transform.childCount > 0)
        {
            PolygonCollider2D collider = blackHole.transform.GetChild(0).GetComponent<PolygonCollider2D>();
            if (collider != null)
                collider.enabled = false;
                
            Animator animator = blackHole.GetComponent<Animator>();
            if (animator != null)
                animator.SetTrigger("over");
        }
        
        yield return new WaitForSeconds(0.25f);
        Destroy(blackHole);
    }
    public IEnumerator DimensionalWave()
    {
        if (!canJump) yield break;  

        ZolaImitation.Instance.CanJump = false;
        base.canAttack = false;
        enemyMovement.StopEnemyMovement();
        walkingCollider.enabled = false;
        normalCollider.enabled = false;
        enemyMovement.rb.constraints = RigidbodyConstraints2D.FreezeAll;

        GameObject shadow = Instantiate(shadowPrefab, centerOfEnemy.transform.position, Quaternion.identity);
        shadow.SetActive(false);

        animator.SetTrigger("dimensionalKi");
        GameObject start;

        start = Instantiate(dimensionalStartCyberpunk, dimensionalPoint.position, Quaternion.identity);

        yield return new WaitForSeconds(0.45f);

        Vector3 targetJumpOff = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        targetJumpOff.y += 15;
        while (Vector2.Distance(transform.position, targetJumpOff) > 0.2f)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetJumpOff, 0.2f);
            enemyMovement.rb.position = transform.position;
            yield return null;
        }

        yield return new WaitForSeconds(0.2f);
        shadow.SetActive(true);

        yield return new WaitForSeconds(0.1f);

        float time = 0;
        Vector3 targetPos = targetAgent != null ? targetAgent.transform.position : transform.position;
        
        while (time < 2)
        {
            targetPos = targetAgent != null ? targetAgent.transform.position : targetPos;
            targetPos.y -= 0.35f;
            time += Time.deltaTime;
            shadow.transform.position = Vector3.Lerp(shadow.transform.position, targetPos, time / 4);
            yield return null;
        }

        yield return new WaitForSeconds(1f);

        animator.SetTrigger("dimensionalBe");
        yield return new WaitForSeconds(0.55f);

        transform.position = new Vector3(shadow.transform.position.x, transform.position.y, transform.position.z);

        while (Vector2.Distance(transform.position, shadow.transform.position) > 0.1f)
        {
            transform.position = Vector2.MoveTowards(transform.position, shadow.transform.position, 0.3f);
            enemyMovement.rb.position = transform.position;
            yield return null;
        }

        GameObject end;
        end = Instantiate(dimensionalEndCyberpunk, dimensionalPoint.position, Quaternion.identity);

        enemyMovement.rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        Destroy(shadow);


        if (dimensionalWave.inRange && targetAgent != null)
            targetAgent.TakeDamage(BaseStatsForZolaBoss.dimensionalWaveDamage);


        walkingCollider.enabled = true;
        normalCollider.enabled = true;
        yield return new WaitForSeconds(0.8f);
        ZolaImitation.Instance.CanJump = true;
        enemyMovement.isMovementDisabled = false;
        StartCoroutine(ResetCanAttack());
        yield return new WaitForSeconds(0.5f);
        Destroy(end);
        Destroy(start);
    }

    public override IEnumerator afterSpawn()
    {

        yield return new WaitForSeconds(0.01f);

    }

 
    #endregion
    
    new void Start()
    {
        base.Start();
        base.maxHP = BaseStatsForZolaBoss.zolaMaxHP;
        base.HP = maxHP;
        enemyMovement = gameObject.GetComponent<EnemyMovement>();
        StartCoroutine(RandomNum());

        if (targetAgent == null)
        {
            targetAgent = FindObjectOfType<ZolaImitation>();
            if (targetAgent == null)
            {
                Debug.LogWarning("ZolaBossImitation could not find a ZolaImitation agent in the scene!");
            }
        }
    }

    new void Update()
    {
        base.Update();
        CheckPlayerHealth();
        if (!isDead)
            UpdateFacingDirection();
        
    }

    public override void TakeDamage(int damage)
    {
        if (isDead)
            return;

        Debug.Log($"ZolaBoss took {damage} dmg");
        simpleFlash.Flash();
        HP -= damage;
        hpBar.value = (float)HP / maxHP;
        if (HP <= 0)
        {
            ZolaImitation.Instance.allSteps += ZolaImitation.Instance.StepCount;
            PlayerPrefs.SetInt("ImitationStepsCounter", ZolaImitation.Instance.allSteps);

            foreach (GameObject obj in gamobjectsToDestroyAfterDeath)
            {
                Destroy(obj);
            }
            StopAllCoroutines();
            Die();
        }
    }

    public override void Die()
    {
        isDead = true;
        enemyMovement.StopEnemyMovement();
        ZolaImitation.Instance.AddReward(2f);
        StartCoroutine(startDeath());
    }

    IEnumerator startDeath()
    {
        animator.SetTrigger("die");
        foreach (GameObject obj in gamobjectsToDestroyAfterDeath)
        {
            Destroy(obj);
        }
        Debug.Log("Boss died");

        if (targetAgent != null)
        {
            targetAgent.EndEpisodeFromExternal(true);
        }
        
        yield return new WaitForSeconds(5f);
        if (RoomManager.Instance != null)
            RoomManager.Instance.BossAct2Killed();
        else
            Destroy(gameObject);
    }
}