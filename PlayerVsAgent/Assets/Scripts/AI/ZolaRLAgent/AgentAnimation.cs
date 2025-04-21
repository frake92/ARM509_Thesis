using UnityEngine;

public class AgentAnimation : MonoBehaviour
{
    private ZolaRLAgent agent;
    public Animator animator;
    private AgentMovement movement;
    private Rigidbody2D rb;
    
    [SerializeField] private float minMovementThreshold = 0.01f;
    [SerializeField] private float movementAnimationSpeed = 1.0f;
    [SerializeField] private float attackAnimationSpeed = 1.0f;
    [SerializeField] private float dimensionalWaveAnimationSpeed = 0.7f;
    
    public void Initialize(ZolaRLAgent agent)
    {
        this.agent = agent;
        this.movement = agent.GetComponent<AgentMovement>();
        this.rb = agent.GetComponent<Rigidbody2D>();
        
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
        
    }
    
    private void UpdateMovementAnimation()
    {
        float frameMoveAmount = rb != null ? rb.linearVelocity.magnitude * Time.deltaTime : 0f;
        float currentSpeed = animator.GetFloat("Speed");
        if (frameMoveAmount < minMovementThreshold)
        {
            if (currentSpeed > 0)
            {
                float newSpeed = Mathf.Lerp(currentSpeed, 0f, Time.deltaTime * 5f);
                animator.SetFloat("Speed", newSpeed);
            }
        }
        else
        {
            if (frameMoveAmount > minMovementThreshold)
            {
                float velocity = rb != null ? rb.linearVelocity.magnitude : 0f;
                float normalizedSpeed = Mathf.Clamp(velocity / 3.5f, 0f, 0.7f);
                float newSpeed = Mathf.Lerp(currentSpeed, normalizedSpeed, Time.deltaTime * 3f);
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
