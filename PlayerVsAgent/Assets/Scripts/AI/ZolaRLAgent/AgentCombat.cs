using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum AttackType
{
    Melee = 1,
    SpatialEruption = 2,
    TemporalSurge = 3,
    DimensionalWave = 4,
    VoidRift = 5
}
public class AgentCombat : MonoBehaviour
{
    private ZolaRLAgent agent;
    private Transform player;
    private Animator animator;
    private AgentMovement movement;
    
    private float lastAttackTime = 0f;
    private float attackCooldown = 1.5f;
    private Dictionary<string, bool> attackHasDealtDamage = new Dictionary<string, bool>();
    private float damageResetTime = 0.1f;
    public Dictionary<int, float> lastAttackTimeByType = new Dictionary<int, float>();
    private Dictionary<int, float> attackCooldownsByType = new Dictionary<int, float>();

    private AttackType? lastAttackType = null;
    private Queue<float> voidRiftTimestamps = new Queue<float>();
    private const float VOID_RIFT_SPAM_WINDOW = 10f;
    private const int VOID_RIFT_SPAM_LIMIT = 1;
    private const float VOID_RIFT_DUPLICATE_PENALTY = -0.3f; 
    private const float DIVERSITY_REWARD = 0.1f;
    private bool canGetReward = true;

    private Queue<float> spatialEruptionTimestamps = new Queue<float>();
    private const float SPATIAL_ERUPTION_SPAM_WINDOW = 10f;
    private const int SPATIAL_ERUPTION_SPAM_LIMIT = 2;
    private const float SPATIAL_ERUPTION_SPAM_PENALTY = -0.7f;

    [Header("Attack Prefabs")]
    public GameObject voidRiftPrefab;
    public GameObject spatialEruptionPrefab;
    public GameObject temporalSurgePrefab;
    public GameObject dimensionalStartPrefab;
    public GameObject dimensionalEndPrefab;

    [Header("Attack Transform Points")]
    public Transform temporalPoint;
    public Transform spatialPoint;
    public Transform dimensionalPoint;

    [Header("Boss Component References")]
    public DimensionalWave dimensionalWave;
    public AethericStrike aethericStrike;
    
    public void Initialize(ZolaRLAgent agent, Transform player)
    {
        this.agent = agent;
        this.player = player;
        this.animator = agent.animation.animator;
        this.movement = agent.GetComponent<AgentMovement>();
        InitializeDictionaries();
    }
    
    private void InitializeDictionaries()
    {
        attackHasDealtDamage = new Dictionary<string, bool>
        {
            { "aethericStrike", false },
            { "spatialEruption", false },
            { "dimensionalWave", false },
            { "voidRift", false },
            { "temporalSurge", false }
        };

        lastAttackTimeByType = new Dictionary<int, float>();
        for (int i = 1; i <= 5; i++)
        {
            lastAttackTimeByType[i] = 0f;
        }

        attackCooldownsByType = new Dictionary<int, float>
        {
            { 1, 1.5f },
            { 2, 2.0f },
            { 3, 10.0f },
            { 4, 5.0f },
            { 5, 5.0f }
        };
    }
    
    public void ResetState()
    {
        List<string> keys = new List<string>(attackHasDealtDamage.Keys);
        
        foreach (var key in keys)
        {
            attackHasDealtDamage[key] = false;
        }
    }
    
    public void UpdateCooldowns()
    {
        if (!agent.CanAttack && Time.time - lastAttackTime > attackCooldown && !agent.IsAttacking)
        {
            agent.CanAttack = true;
        }
    }
    
    public int ValidateAction(int action)
    {
        if (action >= 1 && action <= 5)
        {
            if (!agent.CanAttack || agent.IsAttacking)
                return 6;
            float cooldown = attackCooldownsByType[action];
            if (Time.time - lastAttackTimeByType[action] < cooldown)
                return 6;
            lastAttackTimeByType[action] = Time.time;
        }
        return action;
    }
    
    public void PerformAttack(AttackType attackType)
    {
        if (lastAttackType.HasValue && lastAttackType.Value != attackType)
        {
            agent.AddReward(DIVERSITY_REWARD);
        }
        lastAttackType = attackType;

        if (!agent.CanAttack) return;
        
        switch (attackType)
        {
            case AttackType.Melee:
                StartCoroutine(Attack());
                break;
            case AttackType.SpatialEruption:
                StartCoroutine(SpatialEruption());
                break;
            case AttackType.TemporalSurge:
                StartCoroutine(TemporalSurge());
                break;
            case AttackType.DimensionalWave:
                StartCoroutine(DimensionalWave());
                break;
            case AttackType.VoidRift:
                StartCoroutine(VoidRift());
                break;
        }
    }
    
    #region Attack Methods
    IEnumerator Attack()
    {
        if (!agent.CanAttack || agent.IsAttacking) yield break;

        agent.IsAttacking = true;
        agent.CanAttack = false;
        lastAttackTime = Time.time;
        agent.IsMovementDisabled = true;
        agent.GetComponent<AgentMovement>().StopMovement();
        animator.SetTrigger("aethericStrike");
        movement.StopMovement();
        attackHasDealtDamage["aethericStrike"] = false;
        
        yield return new WaitForSeconds(0.8f);

        if (aethericStrike.inRange && !attackHasDealtDamage["aethericStrike"])
        {
            PlayerHP.Instance.TakeDamage(BaseStatsForZolaBoss.aethricStirkeDamage);
            float damagePercentage = (float)BaseStatsForZolaBoss.aethricStirkeDamage / PlayerHP.Instance.maxHP;
            float reward = 1.0f + damagePercentage * 1.2f; 
            agent.AddReward(reward);
            
            attackHasDealtDamage["aethericStrike"] = true;
            
            StartCoroutine(ResetDamageFlag("aethericStrike"));
        }
        else
        {
            agent.AddReward(-0.4f);
        }

        yield return new WaitForSeconds(1f);

        agent.IsMovementDisabled = false;
        agent.CanAttack = true;
        agent.IsAttacking = false;
        agent.GetComponent<AgentAnimation>().ResetMovementAnimation();
    }

    IEnumerator SpatialEruption()
    {
        if (!agent.CanAttack || agent.IsAttacking) yield break;

        float now = Time.time;
        spatialEruptionTimestamps.Enqueue(now);
        while (spatialEruptionTimestamps.Count > 0 && now - spatialEruptionTimestamps.Peek() > SPATIAL_ERUPTION_SPAM_WINDOW)
            spatialEruptionTimestamps.Dequeue();
        if (spatialEruptionTimestamps.Count > SPATIAL_ERUPTION_SPAM_LIMIT)
        {
            agent.AddReward(SPATIAL_ERUPTION_SPAM_PENALTY);
        }

        agent.IsAttacking = true;
        agent.CanAttack = false;
        lastAttackTime = Time.time;
        agent.IsMovementDisabled = true;
        animator.SetTrigger("spatialEruption");
        movement.StopMovement();
        attackHasDealtDamage["spatialEruption"] = false;
        
        yield return new WaitForSeconds(1.1f);

        if (spatialEruptionPrefab != null && spatialPoint != null) {
            Instantiate(spatialEruptionPrefab, spatialPoint.position, Quaternion.identity);
        }

        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(transform.position, 3f, LayerMask.GetMask("Player"));
        if (hitPlayers.Length > 0 && !attackHasDealtDamage["spatialEruption"])
        {
            PlayerHP.Instance.TakeDamage(BaseStatsForZolaBoss.spatialEruptionDamage);
            float damagePercentage = (float)BaseStatsForZolaBoss.spatialEruptionDamage / PlayerHP.Instance.maxHP;
            float reward = 0.7f + damagePercentage * 1.0f; 
            agent.AddReward(reward);
            attackHasDealtDamage["spatialEruption"] = true;
            
            StartCoroutine(ResetDamageFlag("spatialEruption"));
        }
        else 
        {
            agent.AddReward(-0.2f);
        }
        
        yield return new WaitForSeconds(0.5f);
        
        agent.IsMovementDisabled = false;
        agent.CanAttack = true;
        agent.IsAttacking = false;
        agent.GetComponent<AgentAnimation>().ResetMovementAnimation();
    }

    IEnumerator TemporalSurge()
    {
        if (!agent.CanAttack || agent.IsAttacking) yield break;
    
        lastAttackTime = Time.time;
        agent.IsAttacking = true;
        agent.CanAttack = false;
        lastAttackTime = Time.time;
        agent.IsMovementDisabled = true;
        agent.movement.StopMovement();
        animator.SetTrigger("temporalSurge");
        
        GameObject surge = null;
        if (temporalSurgePrefab != null) {
            surge = Instantiate(temporalSurgePrefab, temporalPoint.position, Quaternion.identity);
        }

        yield return new WaitForSeconds(0.25f);
        
        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForSeconds(0.25f);
            
            int healAmount = (int)(500 * 0.025f);
            agent.Heal(healAmount);
        }
        
        if (surge != null) {
            surge.transform.GetChild(0).GetComponent<Animator>().SetTrigger("ki");
            surge.transform.GetChild(1).GetComponent<Animator>().SetTrigger("ki");
            yield return new WaitForSeconds(0.55f);
            Destroy(surge);
        }

        float hpPercent = (float)agent.HP / BaseStatsForZolaBoss.zolaMaxHP;
        if (hpPercent > 0.8f) {
            agent.AddReward(-0.5f);
        } else if (hpPercent < 0.4f) {
            float healReward = 0.7f;
            agent.AddReward(healReward);
        } else {
            agent.AddReward(-0.1f);
        }
        
        agent.IsMovementDisabled = false;
        agent.CanAttack = true;
        agent.IsAttacking = false;
        
        agent.GetComponent<AgentAnimation>().ResetMovementAnimation();
    }

    IEnumerator DimensionalWave()
    {
        if (!agent.CanAttack || agent.IsAttacking) yield break;
    
        agent.IsAttacking = true;
        agent.CanAttack = false;
        lastAttackTime = Time.time;
        agent.IsMovementDisabled = true;
        
        attackHasDealtDamage["dimensionalWave"] = false;
        
        animator.SetTrigger("dimensionalKi");
        movement.StopMovement();
        Vector3 originalPosition = transform.position;
        
        yield return new WaitForSeconds(0.45f);
        transform.position = new Vector3(transform.position.x, transform.position.y + 15, transform.position.z);
        
        if (dimensionalStartPrefab != null) {
            Instantiate(dimensionalStartPrefab, originalPosition, Quaternion.identity);
        }
        
        yield return new WaitForSeconds(0.3f);
        
        Vector3 targetPosition = player.position;
        
        yield return new WaitForSeconds(1.0f);
        
        animator.SetTrigger("dimensionalBe");
        yield return new WaitForSeconds(0.55f);
        
        transform.position = targetPosition;
        
        if (dimensionalEndPrefab != null) {
            Instantiate(dimensionalEndPrefab, targetPosition, Quaternion.identity);
        }
        
        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(transform.position, 2f, LayerMask.GetMask("Player"));
        if (hitPlayers.Length > 0 && !attackHasDealtDamage["dimensionalWave"])
        {
            PlayerHP.Instance.TakeDamage(BaseStatsForZolaBoss.dimensionalWaveDamage);
            float damagePercentage = (float)BaseStatsForZolaBoss.dimensionalWaveDamage / PlayerHP.Instance.maxHP;
            float reward = 1.2f + damagePercentage * 1.5f;
            agent.AddReward(reward);
            attackHasDealtDamage["dimensionalWave"] = true;
            
            StartCoroutine(ResetDamageFlag("dimensionalWave"));
        }
        else
        {
            agent.AddReward(-0.05f);
        }  
        
        yield return new WaitForSeconds(0.8f);
        agent.IsMovementDisabled = false;
        agent.CanAttack = true;
        agent.IsAttacking = false;
        agent.GetComponent<AgentAnimation>().ResetMovementAnimation();
        
    }

    IEnumerator VoidRift()
    {
        if (!agent.CanAttack || agent.IsAttacking) yield break;


        float now = Time.time;
        voidRiftTimestamps.Enqueue(now);
        while (voidRiftTimestamps.Count > 0 && now - voidRiftTimestamps.Peek() > VOID_RIFT_SPAM_WINDOW)
            voidRiftTimestamps.Dequeue();
        if (voidRiftTimestamps.Count > VOID_RIFT_SPAM_LIMIT)
        {
            canGetReward = false;
            agent.AddReward(-1f);
            Debug.LogError("Void Rift spam penalty applied.");
        }

        agent.IsAttacking = true;
        agent.CanAttack = false;
        lastAttackTime = Time.time;
        agent.IsMovementDisabled = true;
        animator.SetTrigger("voidRift");
        movement.StopMovement();
        yield return new WaitForSeconds(0.45f);
        
        Vector3 targetPosition = GetValidRiftPosition();
        
        if (voidRiftPrefab != null)
        {
            GameObject riftObject = Instantiate(voidRiftPrefab, targetPosition, Quaternion.identity);
            Destroy(riftObject, 5f);
        }

        float distanceToPlayer = Vector3.Distance(targetPosition, player.position);
        float proximityBonus = Mathf.Clamp01(1.0f - (distanceToPlayer / 3.0f));

        int activeRifts = GameObject.FindGameObjectsWithTag("BlackHole").Length;
     
        float reward = 0.7f + proximityBonus * 0.4f;
        if (activeRifts > 1)
        {
            reward += VOID_RIFT_DUPLICATE_PENALTY * (activeRifts - 1); 
        }
        if(canGetReward)
        {
            agent.AddReward(reward);
        }

        
        yield return new WaitForSeconds(1.5f);
        agent.IsMovementDisabled = false;
        agent.CanAttack = true;
        agent.IsAttacking = false;
        canGetReward = true;

        agent.GetComponent<AgentAnimation>().ResetMovementAnimation();
    }
    
    private Vector3 GetValidRiftPosition()
    {
        Vector3 targetPosition = player.position;
        targetPosition.z = transform.position.z;
        
        GridSystem grid = FindFirstObjectByType<GridSystem>();
        if (grid != null)
        {
            int cellX, cellY;
            if (grid.WorldToCell(targetPosition, out cellX, out cellY))
            {
                Vector3 validPosition = grid.CellToWorld(cellX, cellY);
                targetPosition = new Vector3(validPosition.x, validPosition.y, targetPosition.z);
            }
            else
            {
                Vector3 roomCenter = grid.transform.position;
                Vector3 direction = (targetPosition - roomCenter).normalized;
                
                for (float dist = 0.5f; dist < grid.roomSize.x/2; dist += 0.5f)
                {
                    Vector3 testPos = roomCenter + direction * dist;
                    if (grid.WorldToCell(testPos, out cellX, out cellY) && grid.IsWalkable(testPos))
                    {
                        targetPosition = grid.CellToWorld(cellX, cellY);
                        break;
                    }
                }
            }
        }
        
        return targetPosition;
    }
    
    IEnumerator ResetDamageFlag(string attackName)
    {
        yield return new WaitForSeconds(damageResetTime);
        attackHasDealtDamage[attackName] = false;
    }
    #endregion
}
