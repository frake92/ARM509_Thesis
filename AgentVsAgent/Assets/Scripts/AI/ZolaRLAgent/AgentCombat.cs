using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
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

    public ZolaRLAgent opponent;
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
    private const int VOID_RIFT_SPAM_LIMIT = 2;
    private const float VOID_RIFT_SPAM_PENALTY = -0.6f;
    private const float VOID_RIFT_BASE_REWARD = 0.05f;
    private const float VOID_RIFT_DUPLICATE_PENALTY = -0.2f;
    private const float DIVERSITY_REWARD = 0.1f;

    private Queue<float> spatialEruptionTimestamps = new Queue<float>();
    private const float SPATIAL_ERUPTION_SPAM_WINDOW = 10f;
    private const int SPATIAL_ERUPTION_SPAM_LIMIT = 2;
    private const float SPATIAL_ERUPTION_SPAM_PENALTY = -0.7f;
    private const float SPATIAL_ERUPTION_BASE_REWARD = 0.05f;

    private float lastDiversityRewardTime = -10f;
    private float diversityRewardCooldown = 3f;
    private float lastProximityRewardTime = -10f;
    private float proximityRewardCooldown = 3f;
    private float lastTemporalSurgeRewardTime = -10f;
    private const float TEMPORAL_SURGE_REWARD_COOLDOWN = 2.0f;

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
    
    public void Initialize(ZolaRLAgent agent, ZolaRLAgent opponent)
    {
        this.agent = agent;
        this.opponent = opponent;
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
        if (!agent.CanAttack && Time.unscaledTime - lastAttackTime > attackCooldown && !agent.IsAttacking)
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
            if (Time.unscaledTime - lastAttackTimeByType[action] < cooldown)
                return 6;
            lastAttackTimeByType[action] = Time.unscaledTime;
        }
        return action;
    }
    
    public void PerformAttack(AttackType attackType)
    {
       
        if (lastAttackType.HasValue && lastAttackType.Value != attackType && Time.unscaledTime - lastDiversityRewardTime > diversityRewardCooldown)
        {
            agent.AddReward(DIVERSITY_REWARD);
            lastDiversityRewardTime = Time.unscaledTime;
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
        lastAttackTime = Time.unscaledTime;
        agent.IsMovementDisabled = true;
        agent.GetComponent<AgentMovement>().StopMovement();
        animator.SetTrigger("aethericStrike");
        movement.StopMovement();
        attackHasDealtDamage["aethericStrike"] = false;
        
        yield return new WaitForSeconds(0.8f);

        if (aethericStrike.inRange && !attackHasDealtDamage["aethericStrike"])
        {
            opponent.TakeDamage(BaseStatsForZolaBoss.aethricStirkeDamage);
            float damagePercentage = (float)BaseStatsForZolaBoss.aethricStirkeDamage / opponent.MAXHP;
            float reward = 0.4f + damagePercentage * 0.6f;
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


        float now = Time.unscaledTime;
        spatialEruptionTimestamps.Enqueue(now);
        while (spatialEruptionTimestamps.Count > 0 && now - spatialEruptionTimestamps.Peek() > SPATIAL_ERUPTION_SPAM_WINDOW)
            spatialEruptionTimestamps.Dequeue();
        if (spatialEruptionTimestamps.Count > SPATIAL_ERUPTION_SPAM_LIMIT)
        {
            agent.AddReward(SPATIAL_ERUPTION_SPAM_PENALTY * 1.5f);
        }

        agent.IsAttacking = true;
        agent.CanAttack = false;
        lastAttackTime = Time.unscaledTime;
        agent.IsMovementDisabled = true;
        animator.SetTrigger("spatialEruption");
        movement.StopMovement();
        attackHasDealtDamage["spatialEruption"] = false;
        
        yield return new WaitForSeconds(1.1f);
        
        GameObject spatial = null;
       
        if (spatialEruptionPrefab != null && spatialPoint != null) {
            spatial = Instantiate(spatialEruptionPrefab, spatialPoint.position, Quaternion.identity);
            spatial.GetComponent<SpatialEruption>().hitOpponent = opponent;
        }
        yield return new WaitForSeconds(0.25f);
        if (spatial != null && spatial.GetComponent<SpatialEruption>().inRange && !attackHasDealtDamage["spatialEruption"])
        {
            opponent.TakeDamage(BaseStatsForZolaBoss.spatialEruptionDamage);
            float damagePercentage = (float)BaseStatsForZolaBoss.spatialEruptionDamage / opponent.MAXHP;
            float reward = SPATIAL_ERUPTION_BASE_REWARD + damagePercentage * 1.0f;
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
    
        lastAttackTime = Time.unscaledTime;
        agent.IsAttacking = true;
        agent.CanAttack = false;
        lastAttackTime = Time.unscaledTime;
        agent.IsMovementDisabled = true;
        agent.movement.StopMovement();
        animator.SetTrigger("temporalSurge");
        bool placedByZola1;

            
        GameObject surge = null;
        if (temporalSurgePrefab != null) {
            surge = Instantiate(temporalSurgePrefab, temporalPoint.position, Quaternion.identity);
            surge.GetComponentInChildren<TemporalSurge>().opponent = opponent;
        }
        
        yield return new WaitForSeconds(0.25f);
        
        int previousHP = agent.HP;
        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForSeconds(0.25f);
            int healAmount = (int)(500 * 0.025f);
            agent.Heal(healAmount);
        }
        int healedAmount = agent.HP - previousHP;
        if (
            previousHP < agent.MAXHP * 0.3f &&
            agent.HP < (opponent != null ? opponent.HP * 0.5f : agent.MAXHP) &&
            healedAmount > 0 &&
            (Time.unscaledTime - lastTemporalSurgeRewardTime > TEMPORAL_SURGE_REWARD_COOLDOWN)
        )
        {
            agent.AddReward(0.7f);
            lastTemporalSurgeRewardTime = Time.unscaledTime;
        }
        else if (previousHP < agent.MAXHP * 0.3f && healedAmount > 0 && (Time.unscaledTime - lastTemporalSurgeRewardTime > TEMPORAL_SURGE_REWARD_COOLDOWN))
        {
            agent.AddReward(0.5f);
            lastTemporalSurgeRewardTime = Time.unscaledTime;
        }
        else if (healedAmount > 0 && (Time.unscaledTime - lastTemporalSurgeRewardTime > TEMPORAL_SURGE_REWARD_COOLDOWN))
        {
            agent.AddReward(0.1f);
            lastTemporalSurgeRewardTime = Time.unscaledTime;
        }

        if (surge != null) {
            surge.transform.GetChild(0).GetComponent<Animator>().SetTrigger("ki");
            surge.transform.GetChild(1).GetComponent<Animator>().SetTrigger("ki");
            yield return new WaitForSeconds(0.55f);
            Destroy(surge);
        }

        float hpPercent = (float)agent.HP / BaseStatsForZolaBoss.zolaMaxHP;
        if (hpPercent > 0.6f) {
            agent.AddReward(-0.5f);
        } else if (hpPercent < 0.4f) {
            
            previousHP = agent.HP;
            int actualHealAmount = agent.HP - previousHP;
            float healPercentage = (float)actualHealAmount / BaseStatsForZolaBoss.zolaMaxHP;
            float healReward = 0.2f + healPercentage * 2.0f;
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
        if (!agent.CanAttack || agent.IsAttacking || agent.CanJump == false) yield break;

        opponent.CanJump = false;
        agent.IsAttacking = true;
        agent.CanAttack = false;
        lastAttackTime = Time.unscaledTime;
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
        
        Vector3 targetPosition = opponent.transform.position;
        
        yield return new WaitForSeconds(1.0f);
        
        animator.SetTrigger("dimensionalBe");
        yield return new WaitForSeconds(0.55f);
        
        transform.position = targetPosition;
        
        if (dimensionalEndPrefab != null) {
           Instantiate(dimensionalEndPrefab, targetPosition, Quaternion.identity);

        }    
        yield return new WaitForSeconds(0.25f);
        if (dimensionalWave.inRange && !attackHasDealtDamage["dimensionalWave"])
        {
            opponent.TakeDamage(BaseStatsForZolaBoss.dimensionalWaveDamage);
            float damagePercentage = (float)BaseStatsForZolaBoss.dimensionalWaveDamage /opponent.MAXHP;
            float reward = 0.4f + damagePercentage * 1.5f;
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
        opponent.CanJump = true;
        agent.GetComponent<AgentAnimation>().ResetMovementAnimation();
        
    }

    IEnumerator VoidRift()
    {
        if (!agent.CanAttack || agent.IsAttacking) yield break;

        float now = Time.unscaledTime;
        voidRiftTimestamps.Enqueue(now);
        while (voidRiftTimestamps.Count > 0 && now - voidRiftTimestamps.Peek() > VOID_RIFT_SPAM_WINDOW)
            voidRiftTimestamps.Dequeue();
        if (voidRiftTimestamps.Count > VOID_RIFT_SPAM_LIMIT)
        {
            agent.AddReward(VOID_RIFT_SPAM_PENALTY * 1.5f);
        }

        agent.IsAttacking = true;
        agent.CanAttack = false;
        lastAttackTime = Time.unscaledTime;
        agent.IsMovementDisabled = true;
        animator.SetTrigger("voidRift");
        movement.StopMovement();
        yield return new WaitForSeconds(0.45f);
        
        Vector3 targetPosition = GetValidRiftPosition();

        GameObject riftObject = null;
        if (voidRiftPrefab != null)
        {
            riftObject = Instantiate(voidRiftPrefab, targetPosition, Quaternion.identity);
            
            riftObject.GetComponentInChildren<ZolaBlackHole>().opponent = opponent;
            Destroy(riftObject, 5f);
        }

        float distanceToPlayer = Vector3.Distance(targetPosition, opponent.transform.position);
        float proximityBonus = Mathf.Clamp01(1.0f - (distanceToPlayer / 3.0f));

        int activeRifts = GameObject.FindGameObjectsWithTag("BlackHole").Length;
     
        float reward = 0f;
       if (Time.unscaledTime - lastProximityRewardTime > proximityRewardCooldown && distanceToPlayer < 2.0f)
        {
            reward += VOID_RIFT_BASE_REWARD + proximityBonus * 0.5f;
            lastProximityRewardTime = Time.unscaledTime;
        }
        if (activeRifts > 1)
        {
            reward += VOID_RIFT_DUPLICATE_PENALTY * (activeRifts - 1); 
            }
        agent.AddReward(reward);
        
        yield return new WaitForSeconds(1.5f);
        agent.IsMovementDisabled = false;
        agent.CanAttack = true;
        agent.IsAttacking = false;
        
        agent.GetComponent<AgentAnimation>().ResetMovementAnimation();
    }
    
    private Vector3 GetValidRiftPosition()
    {
        Vector3 targetPosition = opponent.transform.position;
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
