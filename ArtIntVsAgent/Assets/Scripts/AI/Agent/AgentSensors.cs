using Unity.MLAgents.Sensors;
using UnityEngine;

public class AgentSensors : MonoBehaviour
{
    private ZolaImitation agent;
    private Transform bossTarget;
    private ZolaBossImitation targetBoss;
    private Animator animator;
    
    public void Initialize(ZolaImitation agent, Transform bossTarget, ZolaBossImitation targetBoss)
    {
        this.agent = agent;
        this.bossTarget = bossTarget;
        this.targetBoss = targetBoss;
        this.animator = agent.animator;
    }
    
    public void CollectObservations(VectorSensor sensor)
    {
        if (bossTarget == null)
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
        float distanceToBoss = Vector3.Distance(transform.position, bossTarget.position);
        sensor.AddObservation(distanceToBoss / 20f);

        Vector3 directionToBoss = (bossTarget.position - transform.position).normalized;
        sensor.AddObservation(directionToBoss);

        Vector2 facingDirection = animator.GetFloat("FacingDirectionX") > 0 ? Vector2.right : Vector2.left;
        float dotProduct = Vector2.Dot(facingDirection, new Vector2(directionToBoss.x, directionToBoss.y));
        sensor.AddObservation(dotProduct);
    }
    
    private void CollectHealthObservations(VectorSensor sensor)
    {
        sensor.AddObservation((float)agent.HP / 500);
        
        sensor.AddObservation(targetBoss != null ? 
                             (float)targetBoss.HP / targetBoss.maxHP : 0);
    }
    
    private void CollectStatusObservations(VectorSensor sensor)
    {
        sensor.AddObservation(agent.CanAttack ? 1.0f : 0.0f);
        sensor.AddObservation(agent.IsAttacking ? 1.0f : 0.0f);
        sensor.AddObservation(agent.movement.IsStuck ? 1.0f : 0.0f);
        sensor.AddObservation(agent.movement.StuckTime / 3.0f);
    }
    
    private void CollectAttackTimingObservations(VectorSensor sensor)
    {
        for (int i = 1; i <= 5; i++)
        {
            float timeSinceAttack = Mathf.Min(10f, Time.time - (agent.combat.lastAttackTimeByType.ContainsKey(i) ? 
                                                               agent.combat.lastAttackTimeByType[i] : 0));
            sensor.AddObservation(timeSinceAttack / 10f);
        }
    }
}