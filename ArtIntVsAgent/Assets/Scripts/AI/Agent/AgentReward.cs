using UnityEngine;

public class AgentReward : MonoBehaviour
{
    private ZolaImitation agent;
    private Transform bossTarget;
    private ZolaBossImitation targetBoss;
    
    private float lastProximityRewardTime = 0f;
    private Vector3 lastActivityPosition;
    private float lastActivityTime;
    private float lastMovementRewardTime = 0f;
    
    private const float PROXIMITY_REWARD_INTERVAL = 1.0f;
    private const float OPTIMAL_DISTANCE_MIN = 2.5f;
    private const float OPTIMAL_DISTANCE_MAX = 5.0f;
    private const float TOO_FAR_DISTANCE = 10.0f;
    private float inactivityThreshold = 1.5f; 
    private float inactivityCheckDistance = 0.5f;
    private const float MOVEMENT_REWARD_INTERVAL = 0.5f;
    private const float MOVEMENT_REWARD_DISTANCE = 0.3f;
    
    public void Initialize(ZolaImitation agent, Transform bossTarget, ZolaBossImitation targetBoss)
    {
        this.agent = agent;
        this.bossTarget = bossTarget;
        this.targetBoss = targetBoss;
        ResetState();
    }
    
    public void ResetState()
    {
        lastActivityPosition = transform.position;
        lastActivityTime = Time.time;
        lastProximityRewardTime = Time.time;
        lastMovementRewardTime = Time.time;
    }
    
    public void UpdateRewards()
    {
        CheckProximityReward();
        CheckInactivityPenalty();
        CheckMovementReward();
    }
    
    private void CheckProximityReward()
    {
        if (Time.time - lastProximityRewardTime < PROXIMITY_REWARD_INTERVAL) return;
        
        lastProximityRewardTime = Time.time;
        
        if (bossTarget == null) return;
        
        float distanceToBoss = Vector3.Distance(transform.position, bossTarget.position);
        
        if (distanceToBoss > OPTIMAL_DISTANCE_MIN && distanceToBoss <= OPTIMAL_DISTANCE_MAX)
        {
            agent.AddReward(0.01f);
            
        }
        else if (distanceToBoss <= OPTIMAL_DISTANCE_MIN)
        {
            agent.AddReward(0.005f);
        }
        else if (distanceToBoss > TOO_FAR_DISTANCE)
        {
            agent.AddReward(-0.005f);
            
        }
    }
    
    private void CheckInactivityPenalty()
    {
        if (agent.IsAttacking) 
        {
            lastActivityPosition = transform.position;
            lastActivityTime = Time.time;
            return;
        }
        
        float distanceMoved = Vector3.Distance(transform.position, lastActivityPosition);

        if (distanceMoved > inactivityCheckDistance)
        {
            lastActivityPosition = transform.position;
            lastActivityTime = Time.time;
            return;
        }
        
        if (Time.time - lastActivityTime > inactivityThreshold)
        {
            agent.AddReward(-0.03f);
            lastActivityTime = Time.time;
        }
    }
    
    private void CheckMovementReward()
    {
        if (Time.time - lastMovementRewardTime < MOVEMENT_REWARD_INTERVAL) return;
        
        lastMovementRewardTime = Time.time;

        float recentMovement = Vector3.Distance(transform.position, lastActivityPosition);
        
        if (recentMovement > MOVEMENT_REWARD_DISTANCE && agent.CanAttack)
        {
            agent.AddReward(0.003f);
            
            if (bossTarget != null)
            {
                float distanceToBoss = Vector3.Distance(transform.position, bossTarget.position);
                
                if ((distanceToBoss > OPTIMAL_DISTANCE_MIN && distanceToBoss < OPTIMAL_DISTANCE_MAX) ||
                    (distanceToBoss > TOO_FAR_DISTANCE && recentMovement > MOVEMENT_REWARD_DISTANCE * 2))
                {
                    agent.AddReward(0.003f);
                }
            }
        }
    }
}