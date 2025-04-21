using UnityEngine;

public class AgentReward : MonoBehaviour
{
    private ZolaRLAgent agent;

    private Vector3 lastActivityPosition;
    private float lastActivityTime;
    private float inactivityThreshold = 1.5f;
    private float inactivityCheckDistance = 0.5f;

    
    public void Initialize(ZolaRLAgent agent, Transform player)
    {
        this.agent = agent;

        ResetState();
    }
    
    public void ResetState()
    {
        lastActivityPosition = transform.position;
        lastActivityTime = Time.time;
    }
    
    public void UpdateRewards()
    {
        CheckInactivityPenalty();
    }
    
    
    private void CheckInactivityPenalty()
    {
        if(agent.IsMovementDisabled) return;
        if (agent.IsAttacking) 
        {
            lastActivityPosition = transform.position;
            lastActivityTime = Time.time;
            return;
        }
        
        float distanceMoved = Vector3.Distance(transform.position, lastActivityPosition);
        if (distanceMoved < 0.01f && agent.movement.rb != null && agent.movement.rb.linearVelocity.magnitude > 0.1f)
        {
            agent.AddReward(-0.4f); 
        }
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
    
   
}
