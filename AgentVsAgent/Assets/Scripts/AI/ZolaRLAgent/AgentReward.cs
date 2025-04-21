using UnityEngine;

public class AgentReward : MonoBehaviour
{
    private ZolaRLAgent agent;

    private Vector3 lastActivityPosition;
    private float lastActivityTime;
    private float inactivityThreshold = 1.5f;
    private float inactivityCheckDistance = 0.5f;
    private const float INACTIVITY_PENALTY = -0.08f;

    private float lastSurvivalRewardTime = 0f;
    private float lastRetreatDistance = 0f;
    private float lastRetreatCheckTime = 0f;

    public void Initialize(ZolaRLAgent agent)
    {
        this.agent = agent;

        ResetState();
    }
    
    public void ResetState()
    {
        lastActivityPosition = transform.position;
        lastActivityTime = Time.unscaledTime;
        lastSurvivalRewardTime = Time.unscaledTime;
        lastRetreatDistance = 0f;
        lastRetreatCheckTime = Time.unscaledTime;
    }
    
    public void UpdateRewards()
    {
        CheckInactivityPenalty();
        GiveLowHPSurvivalReward();
        GiveRetreatReward();
    }

    private void CheckInactivityPenalty()
    {
        if (agent.IsAttacking) 
        {
            lastActivityPosition = transform.position;
            lastActivityTime = Time.unscaledTime;
            return;
        }
        
        float distanceMoved = Vector3.Distance(transform.position, lastActivityPosition);
        
        if (distanceMoved > inactivityCheckDistance)
        {
            lastActivityPosition = transform.position;
            lastActivityTime = Time.unscaledTime;
            return;
        }
        
        if (Time.unscaledTime - lastActivityTime > inactivityThreshold)
        {
            agent.AddReward(INACTIVITY_PENALTY);
            lastActivityTime = Time.unscaledTime;
        }
    }

    private void GiveLowHPSurvivalReward()
    {
        if (agent.HP < agent.MAXHP * 0.3f)
        {
            if (Time.unscaledTime - lastSurvivalRewardTime > 0.5f)
            {
                agent.AddReward(0.03f);
                lastSurvivalRewardTime = Time.unscaledTime;
            }
        }
    }

    private void GiveRetreatReward()
    {
        if (agent.HP < agent.MAXHP * 0.3f && agent.opponent != null)
        {
            if (Time.unscaledTime - lastRetreatCheckTime > 0.5f)
            {
                float currentDistance = Vector3.Distance(agent.transform.position, agent.opponent.transform.position);
                if (currentDistance > lastRetreatDistance + 0.2f)
                {
                    agent.AddReward(0.05f);
                }
                lastRetreatDistance = currentDistance;
                lastRetreatCheckTime = Time.unscaledTime;
            }
        }
    }
}
