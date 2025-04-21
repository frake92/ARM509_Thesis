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
    #region Singleton
    private static ZolaRLAgent instance;
    public static ZolaRLAgent Instance => instance;
    #endregion

    #region Component References
    [Header("Agent Components")]
    [SerializeField] public AgentMovement movement;
    [SerializeField] public AgentCombat combat;
    [SerializeField] public AgentAnimation animation;
    [SerializeField] public AgentSensors sensors;
    [SerializeField] public AgentReward rewards;
    
    [Header("Game References")]
    [SerializeField] private Transform player;
    [SerializeField] private Transform centerOfThing;
    [SerializeField] private Slider hpBar;
    public SimpleFlash simpleFlash;
    #endregion

    #region State Properties
    public int HP { get; private set; } = 500;
    private const int MAXHP = 750;
    public bool CanAttack { get; set; } = true;
    public bool IsMovementDisabled { get; set; } = false;
    public bool IsAttacking { get; set; } = false;
    private bool playerIsDead = false;
    
    // Idle tracking properties
    private Vector3 lastPosition;
    private float lastIdleCheckTime;
    private const float IDLE_PENALTY_INTERVAL = 1.0f;
    private const float IDLE_POSITION_THRESHOLD = 0.1f;
    private const float IDLE_PENALTY = -0.1f;
    #endregion
    
    #region Initialization
    private new void Awake()
    {
        Application.targetFrameRate = 60;

        if(Instance == null)
        {
            instance = this;
        }
        
        if (movement == null) movement = gameObject.AddComponent<AgentMovement>();
        if (combat == null) combat = gameObject.AddComponent<AgentCombat>();
        if (animation == null) animation = gameObject.AddComponent<AgentAnimation>();
        if (sensors == null) sensors = gameObject.AddComponent<AgentSensors>();
        if (rewards == null) rewards = gameObject.AddComponent<AgentReward>();
        
        movement.Initialize(this, player, centerOfThing);
        combat.Initialize(this, player);
        animation.Initialize(this);
        sensors.Initialize(this, player);
        rewards.Initialize(this, player);
    }

    private void Start()
    {
        HP = MAXHP;
        hpBar.value = 1f;
        lastPosition = transform.position;
        lastIdleCheckTime = Time.time;
    }
    #endregion
    
    #region Agent Lifecycle Methods
    public override void OnEpisodeBegin()
    {
        HP = MAXHP;
        CanAttack = true;
        IsMovementDisabled = false;
        IsAttacking = false;
        playerIsDead = false;
        
        movement.ResetState();
        combat.ResetState();
        rewards.ResetState();
        
        StopAllCoroutines();
        
        lastPosition = transform.position;
        lastIdleCheckTime = Time.time;
        Console.Clear();
        Debug.Log("[AGENT] Episode started. Resetting state.");
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
        CheckPlayerStatus();
        CheckIdlePenalty();

        if (Time.frameCount % 100 == 0)
        {
            Debug.Log($"[REWARD DEBUG] Current cumulative reward: {GetCumulativeReward()}");
        }
    }
    
    private void FixedUpdate()
    {
        if (IsAttacking || IsMovementDisabled || player == null) return;
        movement.UpdateFacingDirection();

    }
    #endregion
    
    #region Game Flow Methods
    private void CheckPlayerStatus()
    {
        if (!playerIsDead && player != null && PlayerHP.Instance != null)
        {
            if (PlayerHP.Instance.currentHP <= 0)
            {
                playerIsDead = true;
                
                float healthBonus = (float)HP / MAXHP * 2.0f;
                AddReward(5.0f + healthBonus);
                                
                StartCoroutine(RestartAfterDelay());
            }
        }
    }

    private void CheckIdlePenalty()
    {
        if (IsMovementDisabled) return;
        if (Time.time - lastIdleCheckTime > IDLE_PENALTY_INTERVAL)
        {
            float distanceMoved = Vector3.Distance(transform.position, lastPosition);
            if (distanceMoved < IDLE_POSITION_THRESHOLD)
            {
                AddReward(IDLE_PENALTY);
            }
            lastPosition = transform.position;
            lastIdleCheckTime = Time.time;
        }
    }

    IEnumerator RestartAfterDelay()
    {
        IsMovementDisabled = true;
        CanAttack = false;
        
        yield return new WaitForSeconds(1f);
        
        playerIsDead = false;
        
        if (player != null && PlayerHP.Instance != null)
        {
            PlayerHP.Instance.StartCoroutine(PlayerHP.Instance.startGameOver());
        }
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
            float playerHealthPercent = PlayerHP.Instance ?
                (float)PlayerHP.Instance.currentHP / PlayerHP.Instance.maxHP : 0;
            float defeatPenalty = -3.0f - (playerHealthPercent * 2.0f);
            
            AddReward(defeatPenalty);
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
                movement.MoveTowardsPlayer();
                break;
            case 7:
                movement.MoveAwayFromPlayer();
                break;
            case 8:
                movement.CircleAroundPlayer();
                break;
        }
    }
    #endregion
    
    public Transform PlayerTransform => player;
    public Animator Animator => GetComponent<Animator>();

    public void Heal(int amount)
    {
        HP = Mathf.Min(HP + amount, MAXHP);
    }
}
