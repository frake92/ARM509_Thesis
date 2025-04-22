using Unity.MLAgents.Sensors;
using UnityEngine;

public class AgentSensors : MonoBehaviour
{
    private ZolaRLAgent agent;
    private Transform player;
    private Animator animator;
    private AgentMovement movement;
    
    public void Initialize(ZolaRLAgent agent, Transform player)
    {
        this.agent = agent;
        this.player = player;
        this.animator = agent.animation.animator;
        this.movement = agent.GetComponent<AgentMovement>();
    }
    
    public void CollectObservations(VectorSensor sensor)
    {
        if (player == null)
        {
            sensor.AddObservation(new float[16]);
            return;
        }

        CollectPositionObservations(sensor);
        CollectHealthObservations(sensor);
        CollectStatusObservations(sensor);
        CollectAttackTimingObservations(sensor);
    }
    
    private void CollectPositionObservations(VectorSensor sensor)
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        sensor.AddObservation(distanceToPlayer / 20f);
        
        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        sensor.AddObservation(directionToPlayer);
        
        Vector2 facingDirection = animator.GetFloat("FacingDirectionX") > 0 ? Vector2.right : Vector2.left;
        float dotProduct = Vector2.Dot(facingDirection, new Vector2(directionToPlayer.x, directionToPlayer.y));
        sensor.AddObservation(dotProduct);
    }
    
    private void CollectHealthObservations(VectorSensor sensor)
    {
        sensor.AddObservation((float)agent.HP / 500);
        sensor.AddObservation(player != null ? 
                            (float)PlayerHP.Instance.currentHP / PlayerHP.Instance.maxHP : 0);
    }
    
    private void CollectStatusObservations(VectorSensor sensor)
    {
        sensor.AddObservation(agent.CanAttack ? 1.0f : 0.0f);
        sensor.AddObservation(agent.IsAttacking ? 1.0f : 0.0f);

    }
    
    private void CollectAttackTimingObservations(VectorSensor sensor)
    {
        AgentCombat combat = agent.GetComponent<AgentCombat>();
        for (int i = 1; i <= 5; i++)
        {
            float timeSinceAttack = Mathf.Min(10f, Time.unscaledTime - (combat.lastAttackTimeByType != null && combat.lastAttackTimeByType.ContainsKey(i) ? combat.lastAttackTimeByType[i] : 0));
            sensor.AddObservation(timeSinceAttack / 10f);
        }
    }

    public bool IsPlayerInBlackHole()
    {
        return player != null && PlayerHP.Instance.IsPlayerInBlackHole;
    }
}
