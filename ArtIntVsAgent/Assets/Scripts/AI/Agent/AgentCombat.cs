using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents.Actuators;

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
    private ZolaImitation agent;
    private Transform bossTarget;
    private ZolaBossImitation targetBoss;
    private Animator animator;
    
    private float lastAttackTime = 0f;
    private float attackCooldown = 1.5f;
    public Dictionary<int, float> lastAttackTimeByType = new Dictionary<int, float>();
    private Dictionary<int, float> attackCooldownsByType = new Dictionary<int, float>();
    
    private Dictionary<string, bool> attackHasDealtDamage = new Dictionary<string, bool>();
    private float damageResetTime = 0.1f;
    private float lastForcedAttackTime = 0f;
    private const float FORCE_ATTACK_COOLDOWN = 3.0f;
    private const float MAX_TIME_WITHOUT_ATTACK = 4.0f;

    public int maxBlackHoles = 1;

    public int currentBlackHoles = 0;
    
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
    public AgentDimensionaé dimensionalWave;
    public AgentAethericStrke aethericStrike;
    
    public void Initialize(ZolaImitation agent, Transform bossTarget, ZolaBossImitation targetBoss)
    {
        this.agent = agent;
        this.bossTarget = bossTarget;
        this.targetBoss = targetBoss;
        this.animator = agent.animator;
        
        if (temporalPoint == null) temporalPoint = agent.transform;
        if (spatialPoint == null) spatialPoint = agent.transform;
        if (dimensionalPoint == null) dimensionalPoint = agent.transform;
        
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
            { 3, 8.0f },    
            { 4, 5.0f },    
            { 5, 7.0f }     
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
        
        CheckForceAttack();
        
        if (agent.CanAttack && !agent.IsAttacking && !agent.IsMovementDisabled)
        {
            TryProactiveAttack();
        }
    }
    
    private void TryProactiveAttack()
    {
        if (Time.time - lastAttackTime < 2.0f) return;
        
        float timeSinceLastAttack = Time.time - lastAttackTime;
        float attackProbability = Mathf.Clamp01(timeSinceLastAttack / MAX_TIME_WITHOUT_ATTACK) * 0.5f;
        
        if (bossTarget != null)
        {
            float distanceToBoss = Vector3.Distance(transform.position, bossTarget.position);
            
            if (distanceToBoss < 2.5f)
                attackProbability += 0.15f;
            else if (distanceToBoss < 5.0f)
                attackProbability += 0.1f;
            else if (distanceToBoss < 8.0f)
                attackProbability += 0.05f;
            
            attackProbability *= 0.6f;
            
            if (UnityEngine.Random.value < attackProbability)
            {
                AttackType lastAttackType = GetLastAttackType();
                SelectAndPerformAttack(distanceToBoss, lastAttackType);
            }
        }
    }
    
    private AttackType GetLastAttackType()
    {
        float mostRecentTime = 0f;
        int mostRecentAttack = 0;
        
        for (int i = 1; i <= 5; i++)
        {
            if (lastAttackTimeByType.ContainsKey(i) && lastAttackTimeByType[i] > mostRecentTime)
            {
                mostRecentTime = lastAttackTimeByType[i];
                mostRecentAttack = i;
            }
        }
        
        return (AttackType)mostRecentAttack;
    }
    
    private void SelectAndPerformAttack(float distanceToBoss, AttackType lastAttackType)
    {
        List<AttackType> availableAttacks = new List<AttackType>();
        
        if (distanceToBoss < 2.5f)
        {
            if (IsAttackAvailable(AttackType.Melee) && lastAttackType != AttackType.Melee)
                availableAttacks.Add(AttackType.Melee);
                
            if (IsAttackAvailable(AttackType.SpatialEruption))
                availableAttacks.Add(AttackType.SpatialEruption);
        }
        else if (distanceToBoss < 5.0f)
        {
            if (agent.CanJump && IsAttackAvailable(AttackType.DimensionalWave))
                availableAttacks.Add(AttackType.DimensionalWave);
                
            if (IsAttackAvailable(AttackType.SpatialEruption))
                availableAttacks.Add(AttackType.SpatialEruption);
                
            if (IsAttackAvailable(AttackType.VoidRift))
                availableAttacks.Add(AttackType.VoidRift);
        }
        else
        {
            if (agent.CanJump && IsAttackAvailable(AttackType.DimensionalWave))
                availableAttacks.Add(AttackType.DimensionalWave);
                
            if (IsAttackAvailable(AttackType.VoidRift))
                availableAttacks.Add(AttackType.VoidRift);
        }
        
        float agentHealthPercentage = (float)agent.HP / 500;
        if (agentHealthPercentage < 0.4f && IsAttackAvailable(AttackType.TemporalSurge))
        {
            if (agentHealthPercentage < 0.25f || UnityEngine.Random.value < 0.7f)
                availableAttacks.Add(AttackType.TemporalSurge);
        }
        
        if (availableAttacks.Count > 0)
        {
            AttackType selectedAttack = availableAttacks[UnityEngine.Random.Range(0, availableAttacks.Count)];
            PerformAttack(selectedAttack);
        }
    }
    
    private bool IsAttackAvailable(AttackType attackType)
    {
        int attackTypeInt = (int)attackType;
        return Time.time - lastAttackTimeByType[attackTypeInt] >= attackCooldownsByType[attackTypeInt];
    }
    
    private void CheckForceAttack()
    {
        if (agent.IsAttacking || !agent.CanAttack || agent.IsMovementDisabled || 
            Time.time - lastForcedAttackTime < FORCE_ATTACK_COOLDOWN)
            return;
            
        if (Time.time - lastAttackTime > MAX_TIME_WITHOUT_ATTACK)
        {
            ForceAttackBasedOnDistance();
            lastForcedAttackTime = Time.time;
        }
    }
    
    private void ForceAttackBasedOnDistance()
    {
        if (bossTarget == null) return;
        
        float distanceToBoss = Vector3.Distance(transform.position, bossTarget.position);
        AttackType forcedAttack;
        
        if (distanceToBoss < 2.5f)
            forcedAttack = AttackType.Melee;
        else if (distanceToBoss < 5.0f)
        {
            forcedAttack = (Time.frameCount % 2 == 0) ? 
                AttackType.SpatialEruption : AttackType.DimensionalWave;
        }
        else if (distanceToBoss < 8.0f)
            forcedAttack = AttackType.VoidRift;
        else
            forcedAttack = AttackType.DimensionalWave;
            
        Debug.LogError($"===FORCING ATTACK {(int)forcedAttack} after {Time.time - lastAttackTime:F1}s of no attacks===");
        PerformAttack(forcedAttack);
    }
    
    public int ValidateAction(int action)
    {
        if (action >= 1 && action <= 5)
        {
            float specificCooldown = attackCooldownsByType[action];
            if (Time.time - lastAttackTimeByType[action] < specificCooldown)
            {
                return 6;
            }
        }
        
        if (agent.movement.IsStuck && agent.movement.StuckTime > 1.5f && action != 4) 
        {
            return 4;
        }
        else if (!agent.CanAttack && action <= 5)
        {
            return 6;
        }
        
        if (action <= 5)
        {
            lastAttackTimeByType[action] = Time.time;
        }
        
        return action;
    }
    
    public void PerformAttack(AttackType attackType)
    {
        if (!agent.CanAttack) 
        {
            Debug.LogError("Cannot perform attack: Agent's attack is on cooldown!");
            return;
        }
        
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
    
  
    #region Attack Implementations
    
    IEnumerator Attack()
    {
        if (!agent.CanAttack || agent.IsAttacking) yield break;

        agent.IsAttacking = true;
        agent.CanAttack = false;
        lastAttackTime = Time.time;
        agent.IsMovementDisabled = true;
        agent.movement.StopMovement();
        animator.SetTrigger("aethericStrike");
        
        attackHasDealtDamage["aethericStrike"] = false;
        
        yield return new WaitForSeconds(0.8f);

        float distanceToBoss = Vector3.Distance(transform.position, targetBoss.transform.position);
        
        if ((aethericStrike.inRange || distanceToBoss < 2.5f) && !attackHasDealtDamage["aethericStrike"] && targetBoss != null)
        {
            targetBoss.TakeDamage(BaseStatsForZolaBoss.aethricStirkeDamage);
            float damagePercentage = (float)BaseStatsForZolaBoss.aethricStirkeDamage / targetBoss.maxHP;
            float reward = 0.1f + damagePercentage * 0.2f; 
            agent.AddReward(reward);
            Debug.LogError($"___Aetheric Strike hit at distance {distanceToBoss:F2}, reward applied: {reward:F2}___");
            attackHasDealtDamage["aethericStrike"] = true;
            
            StartCoroutine(ResetDamageFlag("aethericStrike"));
        }
        else
        {
            Debug.LogError($"___Aetheric Strike missed! inRange={aethericStrike.inRange}, distance={distanceToBoss:F2}___");
            agent.AddReward(-0.2f);
        }

        yield return new WaitForSeconds(0.7f);

        agent.IsMovementDisabled = false;
        agent.CanAttack = true;
        agent.IsAttacking = false;
        agent.animation.ResetMovementAnimation();
    }

    IEnumerator SpatialEruption()
    {
        if (!agent.CanAttack || agent.IsAttacking) yield break;

        agent.IsAttacking = true;
        agent.CanAttack = false;
        lastAttackTime = Time.time;
        agent.IsMovementDisabled = true;
        agent.movement.StopMovement();
        animator.SetTrigger("spatialEruption");
        
        attackHasDealtDamage["spatialEruption"] = false;
        
        yield return new WaitForSeconds(1.1f);

        spatialPoint.position = transform.position + transform.forward * 1.5f;
        
        if (spatialEruptionPrefab != null) {
            Instantiate(spatialEruptionPrefab, spatialPoint.position, Quaternion.identity);
        }

        if (targetBoss != null)
        {
            float distanceToBoss = Vector3.Distance(transform.position, targetBoss.transform.position);
            if (distanceToBoss < 3f && !attackHasDealtDamage["spatialEruption"])
            {
                targetBoss.TakeDamage(BaseStatsForZolaBoss.spatialEruptionDamage);
                float damagePercentage = (float)BaseStatsForZolaBoss.spatialEruptionDamage / targetBoss.maxHP;
                float reward = 0.2f + damagePercentage * 1.0f;
                agent.AddReward(reward);
                Debug.LogError($"___Spatial Eruption hit, reward applied: {reward:F2}___");
                attackHasDealtDamage["spatialEruption"] = true;
                
                StartCoroutine(ResetDamageFlag("spatialEruption"));
            }
            else 
            {
                agent.AddReward(-0.1f);
                Debug.LogError("___Spatial Eruption missed___");
            }
        }
        
        yield return new WaitForSeconds(0.5f);
        
        agent.IsMovementDisabled = false;
        agent.CanAttack = true;
        agent.IsAttacking = false;
        agent.animation.ResetMovementAnimation();
    }

    IEnumerator TemporalSurge()
    {
        if (!agent.CanAttack || agent.IsAttacking) yield break;
    
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

        yield return new WaitForSeconds(0.55f);
        
        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForSeconds(0.5f);
            
            int healAmount = (int)(500 * 0.025f);
            agent.Heal(healAmount);
        }
        
        if (surge != null) {
            surge.transform.GetChild(0).GetComponent<Animator>().SetTrigger("ki");
            surge.transform.GetChild(1).GetComponent<Animator>().SetTrigger("ki");
            yield return new WaitForSeconds(1.4f);
            yield return new WaitForSeconds(0.35f);
            Destroy(surge);
        }
        
        yield return new WaitForSeconds(0.6f);
        
        agent.IsMovementDisabled = false;
        agent.CanAttack = true;
        agent.IsAttacking = false;
        agent.animation.ResetMovementAnimation();
    }

    IEnumerator DimensionalWave()
    {
        if (!agent.CanJump) yield break;

        ZolaBossImitation.Instance.canJump = false;
        if (!agent.CanAttack || agent.IsAttacking) yield break;

        agent.IsAttacking = true;
        agent.CanAttack = false;
        lastAttackTime = Time.time;
        agent.IsMovementDisabled = true;
        agent.movement.StopMovement();
        
        attackHasDealtDamage["dimensionalWave"] = false;
        
        animator.SetTrigger("dimensionalKi");
        
        Vector3 originalPosition = transform.position;
        Vector3 lastKnownBossPosition = bossTarget != null ? bossTarget.position : transform.position + Vector3.right * 5f;
        
        yield return new WaitForSeconds(0.45f);
        transform.position = new Vector3(transform.position.x, transform.position.y + 15, transform.position.z);
        
        if (dimensionalStartPrefab != null) {
            Instantiate(dimensionalStartPrefab, originalPosition, Quaternion.identity);
        }
        
        yield return new WaitForSeconds(0.3f);
        
        Vector3 targetPosition;
        
        if (bossTarget != null && bossTarget.gameObject.activeInHierarchy) {
            targetPosition = bossTarget.position;
        } else {
            targetPosition = lastKnownBossPosition + new Vector3(
                UnityEngine.Random.Range(-2f, 2f), 
                UnityEngine.Random.Range(-2f, 2f), 
                0);
        }
        
        yield return new WaitForSeconds(1.0f);
        
        animator.SetTrigger("dimensionalBe");
        yield return new WaitForSeconds(0.55f);
        
        transform.position = targetPosition;
        
        if (dimensionalEndPrefab != null) {
            Instantiate(dimensionalEndPrefab, targetPosition, Quaternion.identity);
        }
        
        if (targetBoss != null && !attackHasDealtDamage["dimensionalWave"])
        {
            float distanceToBoss = Vector3.Distance(transform.position, targetBoss.transform.position);
            if (distanceToBoss < 2f)
            {
                targetBoss.TakeDamage(BaseStatsForZolaBoss.dimensionalWaveDamage);
                float damagePercentage = (float)BaseStatsForZolaBoss.dimensionalWaveDamage / targetBoss.maxHP;
                float reward = 0.3f + damagePercentage * 1.0f;
                agent.AddReward(reward);
                Debug.LogError($"___Dimensional Wave hit, reward applied: {reward:F2}___");
                attackHasDealtDamage["dimensionalWave"] = true;
                
                StartCoroutine(ResetDamageFlag("dimensionalWave"));
            }
            else
            {
                agent.AddReward(-0.2f);
                Debug.LogError("___Dimensional Wave missed___");
            }
        }
        
        ZolaBossImitation.Instance.canJump = true;
        
        yield return new WaitForSeconds(0.8f);
        agent.IsMovementDisabled = false;
        agent.CanAttack = true;
        agent.IsAttacking = false;
        agent.animation.ResetMovementAnimation();
        
        agent.movement.ResetStuckState();
    }

    IEnumerator VoidRift()
    {
        if (!agent.CanAttack || agent.IsAttacking) yield break;
        
        if (currentBlackHoles >= maxBlackHoles)
        {
            Debug.LogWarning("Maximum number of black holes already reached. Cannot spawn more.");
            agent.AddReward(-0.05f); 
            yield break;
        }
        
        agent.IsAttacking = true;
        agent.CanAttack = false;
        lastAttackTime = Time.time;
        agent.IsMovementDisabled = true;
        agent.movement.StopMovement();
        animator.SetTrigger("voidRift");
        
        yield return new WaitForSeconds(0.45f);
        
        Vector3 targetPosition = GetValidRiftPosition();
        currentBlackHoles++;
        
        if (voidRiftPrefab != null) {
            GameObject riftObject = Instantiate(voidRiftPrefab, targetPosition, Quaternion.identity);
            
            BlackHoleTracker tracker = riftObject.AddComponent<BlackHoleTracker>();
            tracker.combatSystem = this;
            
            Destroy(riftObject, 5f);
        }
        
        if (targetBoss != null)
        {
            float distanceToBoss = Vector3.Distance(targetPosition, targetBoss.transform.position);
            float proximityBonus = Mathf.Clamp01(1.0f - (distanceToBoss / 3.0f));
            
            int activeRifts = GameObject.FindGameObjectsWithTag("BlackHole").Length;
            
            float reward;
            if (activeRifts > maxBlackHoles) {
                reward = -0.1f;
                Debug.LogError($"___Too many Void Rifts active ({activeRifts}), penalty applied___");
            } else {
                reward = 0.1f + proximityBonus * 0.3f;
            }
            agent.AddReward(reward);
        }
        
        yield return new WaitForSeconds(1.5f);
        agent.IsMovementDisabled = false;
        agent.CanAttack = true;
        agent.IsAttacking = false;
        agent.animation.ResetMovementAnimation();
        
    }
    
    private Vector3 GetValidRiftPosition()
    {
        Vector3 targetPosition = bossTarget != null ? bossTarget.position : transform.position;
        targetPosition.z = transform.position.z;
        
        GridSystem grid = UnityEngine.Object.FindFirstObjectByType<GridSystem>();
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
    
    public float LastAttackTime => lastAttackTime;
}

public class BlackHoleTracker : MonoBehaviour
{
    public AgentCombat combatSystem;

    private void OnDestroy()
    {
        if (combatSystem != null)
        {
            combatSystem.currentBlackHoles--;
        }
    }
}