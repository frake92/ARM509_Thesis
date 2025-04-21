using UnityEngine;

public class AgentAnimation : MonoBehaviour
{
    private ZolaImitation agent;
    private Animator animator;
    
    [SerializeField] private float minMovementThreshold = 0.01f;
    [SerializeField] private float movementAnimationSpeed = 1.0f;
    [SerializeField] private float attackAnimationSpeed = 1.0f;
    [SerializeField] private float dimensionalWaveAnimationSpeed = 0.7f;
    
    public void Initialize(ZolaImitation agent)
    {
        this.agent = agent;
        this.animator = agent.animator;
        
        animator.speed = 1f;
        animator.SetFloat("Speed", 0f);
    }
    
    public void UpdateAnimations()
    {
        if (!agent.IsAttacking && !agent.IsMovementDisabled)
        {
            UpdateMovementAnimation();
        }
        else
        {
            SlowDownMovementAnimation();
        }
        
        agent.movement.UpdateLastFramePosition();
    }
    
    private void UpdateMovementAnimation()
    {
        Vector3 lastFramePosition = agent.movement.LastFramePosition;
        float frameMoveAmount = Vector3.Distance(transform.position, lastFramePosition);
        
        if (frameMoveAmount < minMovementThreshold)
        {
            int stationaryFrames = agent.movement.StationaryFrames + 1;
            agent.movement.SetStationaryFrames(stationaryFrames);
            
            if (stationaryFrames > 2)
            {
                float currentSpeed = animator.GetFloat("Speed");
                if (currentSpeed > 0)
                {
                    float newSpeed = Mathf.Lerp(currentSpeed, 0f, Time.deltaTime * 5f);
                    animator.SetFloat("Speed", newSpeed);
                }
                
                if (Time.time - agent.movement.LastActualMovementTime > 2.0f && agent.CanAttack && Time.frameCount % 60 == 0)
                {
                    agent.combat.PerformAttack(AttackType.DimensionalWave);
                }
            }
        }
        else
        {
            agent.movement.SetStationaryFrames(0);
            
            if (frameMoveAmount > minMovementThreshold)
            {
                float velocity = frameMoveAmount / Time.deltaTime;
                
                float normalizedSpeed = Mathf.Clamp(velocity / 3.5f, 0f, 0.7f);
                
                float currentSpeed = animator.GetFloat("Speed");
                float targetSpeed = normalizedSpeed;
                float newSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * 3f);
                
                animator.SetFloat("Speed", newSpeed);
            }
        }
    }
    
    private void SlowDownMovementAnimation()
    {
        if (animator.GetFloat("Speed") > 0 && !animator.GetCurrentAnimatorStateInfo(0).IsName("Run"))
        {
            float currentSpeed = animator.GetFloat("Speed");
            float newSpeed = Mathf.Lerp(currentSpeed, 0f, Time.deltaTime * 8f);
            animator.SetFloat("Speed", newSpeed);
        }
    }
    
    public void ResetMovementAnimation()
    {
        animator.SetFloat("Speed", 0f);
    }
}