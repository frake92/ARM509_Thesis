using System.Collections;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
public class ZolaRLAgent : Agent
{


    #region Component References
    [Header("Agent Components")]
    [SerializeField] public AgentMovement movement;
    [SerializeField] public AgentCombat combat;
    [SerializeField] public AgentAnimation animation;
    [SerializeField] public AgentSensors sensors;
    [SerializeField] public AgentReward rewards;
    
    [Header("Game References")]
    [SerializeField] public ZolaRLAgent opponent;
    [SerializeField] public Transform centerOfThing;
    [SerializeField] private Slider hpBar;
    public SimpleFlash simpleFlash;
    public bool isOpponentInBlackHole = false;
    #endregion

    #region State Properties
    public int HP { get; private set; } = 500;
    public int MAXHP = 750;
    public bool CanAttack { get; set; } = true;
    public bool IsMovementDisabled { get; set; } = false;
    public bool IsAttacking { get; set; } = false;
    private bool opponentIsDead = false;
    
    // Idle tracking properties
    private Vector3 lastPosition;
    private float lastIdleCheckTime;
    private const float IDLE_PENALTY_INTERVAL = 1.0f;
    private const float IDLE_POSITION_THRESHOLD = 0.1f;
    private const float IDLE_PENALTY = -0.1f;

    public bool CanJump { get; set; } = true;
    #endregion
    
    #region Initialization
    private new void Awake()
    {
        Application.targetFrameRate = 60;
        
        if (movement == null) movement = gameObject.AddComponent<AgentMovement>();
        if (combat == null) combat = gameObject.AddComponent<AgentCombat>();
        if (animation == null) animation = gameObject.AddComponent<AgentAnimation>();
        if (sensors == null) sensors = gameObject.AddComponent<AgentSensors>();
        if (rewards == null) rewards = gameObject.AddComponent<AgentReward>();
        
        movement.Initialize(this, opponent, centerOfThing);
        combat.Initialize(this, opponent);
        animation.Initialize(this);
        sensors.Initialize(this, opponent);
        rewards.Initialize(this);
    }

    private void Start()
    {
        Application.targetFrameRate = 60;
        CanJump = true;
        HP = MAXHP;
        hpBar.value = 1f;
        lastPosition = transform.position;
        lastIdleCheckTime = Time.unscaledTime;
    }
    #endregion
    
    #region Agent Lifecycle Methods
    public override void OnEpisodeBegin()
    {
        HP = MAXHP;
        CanAttack = true;
        IsMovementDisabled = false;
        IsAttacking = false;
        opponentIsDead = false;
        
        movement.ResetState();
        combat.ResetState();
        rewards.ResetState();
        
        StopAllCoroutines();
        
        lastPosition = transform.position;
        lastIdleCheckTime = Time.unscaledTime;
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensors.CollectObservations(sensor);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int action = actions.DiscreteActions[0];
        
        if (IsAttacking) return;
        
        action = combat.ValidateAction(action);
        
        ExecuteAction(action);
    }
    #endregion
    
    #region Update Methods
    private void Update()
    {
        combat.UpdateCooldowns();
        animation.UpdateAnimations();
        rewards.UpdateRewards();
        CheckOpponentStatus();
        CheckIdlePenalty();

        if (Time.frameCount % 100 == 0)
        {
            Debug.Log($"[REWARD DEBUG] Current cumulative reward: {GetCumulativeReward()}");
        }
    }
    
    private void FixedUpdate()
    {
        if (IsAttacking || IsMovementDisabled || opponent == null) return;
        movement.UpdateFacingDirection();

    }
    #endregion
    
    #region Game Flow Methods
    private void CheckOpponentStatus()
    {
        if (!opponentIsDead && opponent != null )
        {
            if (opponent.HP <= 0)
            {
                opponentIsDead = true;
                
                float healthBonus = (float)HP / MAXHP * 2.0f;
                AddReward(5.0f + healthBonus);
            }
        }
    }

    private void CheckIdlePenalty()
    {
        if (IsMovementDisabled) return;
        if (Time.unscaledTime - lastIdleCheckTime > IDLE_PENALTY_INTERVAL)
        {
            float distanceMoved = Vector3.Distance(transform.position, lastPosition);
            if (distanceMoved < IDLE_POSITION_THRESHOLD)
            {
                AddReward(IDLE_PENALTY);
            }
            lastPosition = transform.position;
            lastIdleCheckTime = Time.unscaledTime;
        }
    }

    public IEnumerator WaitBeforeEndEpisode()
    {
        yield return new WaitForSeconds(1.0f);
    }
    public void TakeDamage(int damage)
    {
        HP -= damage;
        hpBar.value = (float)HP / MAXHP;
        
        float damagePercentage = (float)damage / MAXHP;
        float penalty = -0.01f - damagePercentage * 0.05f;
        
        AddReward(penalty);
        
        simpleFlash.Flash();
        if (HP <= 0)
        {
            float opponentHealthPercent = opponent ?
                (float)opponent.HP / opponent.MAXHP : 0;
            float defeatPenalty = -5.0f - (opponentHealthPercent * 2.0f);
            
            AddReward(defeatPenalty);
            StartCoroutine(WaitBeforeEndEpisode());
            EndEpisode();
            SceneManager.LoadScene("Test-train");

        }
    }
    #endregion
    
    #region Action Execution
    private void ExecuteAction(int action)
    {
        switch (action)
        {
            case 1:
                combat.PerformAttack(AttackType.Melee);
                break;
            case 2:
                combat.PerformAttack(AttackType.SpatialEruption);
                break;
            case 3:
                combat.PerformAttack(AttackType.TemporalSurge);
                break;
            case 4:
                combat.PerformAttack(AttackType.DimensionalWave);
                break;
            case 5:
                combat.PerformAttack(AttackType.VoidRift);
                break;
            case 6:
                movement.MoveTowardsOpponent();
                break;
            case 7:
                movement.MoveAwayFromOpponent();
                break;
            case 8:
                movement.CircleAroundOpponent();
                break;
        }
    }
    #endregion
    
    public void Heal(int amount)
    {
        HP = Mathf.Min(HP + amount, MAXHP); 
    }
}
