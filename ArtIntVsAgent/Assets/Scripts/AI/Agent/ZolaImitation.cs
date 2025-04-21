using System.Collections;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class ZolaImitation : Agent
{
    #region Singleton
    private static ZolaImitation instance;
    public static ZolaImitation Instance => instance;
    #endregion

    #region Component References
    [Header("Agent Components")]
    public AgentMovement movement;
    public AgentCombat combat;
    public AgentAnimation animation;
    public AgentSensors sensors;
    public AgentReward rewards;
    
    [Header("Game References")]
    public ZolaBossImitation zolaBoss;
    public SimpleFlash simpleFlash;
    [SerializeField] private Transform bossTarget;
    [SerializeField] public GameObject centerOfThing;
    [SerializeField] private ZolaBossImitation targetBoss;
    public Rigidbody2D rb;
    public Animator animator;
    public Slider hpBar;
    public TextMeshProUGUI stepCounter;
    public TextMeshProUGUI allStepCounter;
    #endregion

    #region State Properties
    public int HP { get; private set; } = 500;
    private const int MAXHP = 500;
    public bool CanAttack { get; set; } = true;
    public bool IsMovementDisabled { get; set; } = false;
    public bool IsAttacking { get; set; } = false;
    public bool CanJump { get; set; } = true;
    private bool bossIsDead = false;
    public int allSteps;

    private float observationTime = 0f;
    private float lastActionTime = 0f;
    private int lastBossAction = 0;
    private float bossActionCooldown = 0.5f;
    private int consecutiveAttacksWithoutMoving = 0;
    
    private bool isResetting = false;
    #endregion
    
    #region Initialization
    private void Awake()
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
        
        movement.Initialize(this, bossTarget, centerOfThing);
        combat.Initialize(this, bossTarget, targetBoss);
        animation.Initialize(this);
        sensors.Initialize(this, bossTarget, targetBoss);
        rewards.Initialize(this, bossTarget, targetBoss);
    }

    private void Start()
    {

        HP = MAXHP;
        hpBar.value = 1f;
        CanJump = true;

        movement.ResetState();
        combat.ResetState();
        rewards.ResetState();
    }
    #endregion
    
    #region Agent Lifecycle Methods
    public override void OnEpisodeBegin()
    {
        Debug.Log($"[{Time.time}] Episode has begun.");
        

        HP = MAXHP;
        CanAttack = true;
        IsMovementDisabled = false;
        IsAttacking = false;
        bossIsDead = false;
        
        movement.ResetState();
        combat.ResetState();
        rewards.ResetState();
        
        StopAllCoroutines();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensors.CollectObservations(sensor);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        int action = actions.DiscreteActions[0];
        
        if (IsAttacking) {
            
            return;
        }
        
        
        action = combat.ValidateAction(action);
        
        ExecuteAction(action);
    }
    
    #endregion
    
    #region Update Methods
    private void Update()
    {
        UpdateUI();
        combat.UpdateCooldowns();
        animation.UpdateAnimations();
        rewards.UpdateRewards();
        ObserveBossActions();
        if(combat.currentBlackHoles > combat.maxBlackHoles)
        {
            AddReward(-0.1f);
        }

        if (Time.frameCount % 100 == 0)
        {
        Debug.Log($"[REWARD DEBUG] Current cumulative reward: {GetCumulativeReward()}");
}
    }
    
    private void FixedUpdate()
    {
        if (IsAttacking || IsMovementDisabled || bossTarget == null) return;
        
        movement.UpdateFacingDirection();
        movement.CheckIfStuck();
        movement.AutoMoveBasedOnDistance();
    }
    #endregion
    
    #region UI and State Updates
    private void UpdateUI()
    {
        hpBar.value = (float)HP / MAXHP;
    }
    
    private void ObserveBossActions()
    {
        if (targetBoss == null || Time.time - lastActionTime < bossActionCooldown)
            return;

        Animator bossAnimator = targetBoss.GetComponent<Animator>();
        if (bossAnimator == null) return;

        int bossAction = 0;

        AnimatorStateInfo stateInfo = bossAnimator.GetCurrentAnimatorStateInfo(0);
        if (stateInfo.IsName("aethericStrike"))
            bossAction = 1;
        else if (stateInfo.IsName("spatialEruption"))
            bossAction = 2;
        else if (stateInfo.IsName("temporalSurge"))
            bossAction = 3;
        else if (stateInfo.IsName("dimensionalKi") || stateInfo.IsName("dimensionalBe"))
            bossAction = 4;
        else if (stateInfo.IsName("voidRift"))
            bossAction = 5;

        if (bossAction > 0 && bossAction != lastBossAction)
        {
            AddReward(0.05f); 
            lastActionTime = Time.time;
            lastBossAction = bossAction;
            if (Time.time - observationTime > 3.0f && UnityEngine.Random.value > 0.3f)
            {
                ExecuteAction(bossAction);
                observationTime = Time.time;
            }
        }
    }

   private void EndEpisodeAndRestart(bool isVictory = false)
    {
        IsMovementDisabled = true;
        CanAttack = false;

        float finalReward = isVictory ? 
            0.5f + ((float)HP / MAXHP * 0.5f) : 0.2f;                           
            
        AddReward(finalReward);
        Debug.Log($"[REWARD DEBUG] Adding END OF EPISODE reward: {finalReward}. Total cumulative reward: {GetCumulativeReward()}");
        
        Debug.Log("ENDED EPISODE");
        EndEpisode();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

   public void EndEpisodeFromExternal(bool isVictory = false)
    {
        IsMovementDisabled = true;
        CanAttack = false;
        
        float finalReward = isVictory ? 
            0.5f + ((float)HP / MAXHP * 0.5f) : 0.2f;    
        AddReward(finalReward);
        Debug.Log($"[REWARD DEBUG] Adding END OF EPISODE reward: {finalReward}. Total cumulative reward: {GetCumulativeReward()}");
        
        Debug.Log("ENDED EPISODE");
        EndEpisode();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name); 
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
                movement.MoveTowardsBoss();
                break;
            case 7:
                movement.MoveAwayFromBoss();
                break;
            case 8:
                movement.CircleAroundBoss();
                break;
            case 9:
                movement.RepositionToOptimalRange();
                break;
        }
    }
    #endregion
    
    #region Health Management
    public void TakeDamage(int damage)
    {
        HP -= damage;
        hpBar.value = (float)HP / MAXHP;
        
        float damagePercentage = (float)damage / MAXHP;
        float penalty = -0.1f - damagePercentage * 0.2f;
        AddReward(penalty);
        simpleFlash.Flash();
        
        if(HP <= 0)
        {
             float bossHealthPercent = targetBoss ? 
                (float)targetBoss.HP / targetBoss.maxHP : 0;
            float defeatPenalty = -0.5f - (bossHealthPercent * 0.5f); 
            
            AddReward(defeatPenalty);            
            EndEpisodeAndRestart(false);
        }
    }
    
    public void Heal(int amount)
    {
        int previousHP = HP;
        HP = Mathf.Min(HP + amount, MAXHP);
        int actualHealAmount = HP - previousHP;
        
        if (actualHealAmount > 0)
        {
            float healPercentage = (float)actualHealAmount / MAXHP;
            float healReward = 0.2f + healPercentage * 2.0f;
            AddReward(healReward);
        }
    }
    
    #endregion
}