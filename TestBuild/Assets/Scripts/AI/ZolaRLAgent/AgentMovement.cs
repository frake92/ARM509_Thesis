using System.Collections;
using UnityEngine;

public class AgentMovement : MonoBehaviour
{
    private ZolaRLAgent agent;
    private Transform player;
    public Transform centerOfThing;
    private Animator animator;
    public Rigidbody2D rb;
    
    [SerializeField] private float moveSpeed = 4.0f;
    
    private const float OPTIMAL_DISTANCE_MIN = 0.25f;
    private const float OPTIMAL_DISTANCE_MAX = 1.5f;
    [SerializeField] private float circlingStrafeSpeed = 1.8f;
    
    public void Initialize(ZolaRLAgent agent, Transform player, Transform centerOfThing)
    {
        this.agent = agent;
        this.player = player;
        this.centerOfThing = centerOfThing;
        this.animator = agent.animation.animator;
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        }
        ResetState();
    }
    
    public void ResetState()
    {
        rb.linearVelocity = Vector2.zero;
    }
    
    public void UpdateFacingDirection()
    {
        if (Player.Instance.centerOfPlayer.transform.position.x <= centerOfThing.transform.position.x)
        {
            animator.SetFloat("FacingDirectionX", -1);
            animator.GetComponent<SpriteRenderer>().flipX = false;
        }
        else
        {
            animator.SetFloat("FacingDirectionX", 1);
            animator.GetComponent<SpriteRenderer>().flipX = true;
        }
    }
    
    public void CircleAroundOpponent()
    {
        if (agent.IsMovementDisabled) return;
        Vector3 dirToPlayer = (player.position - transform.position).normalized;
        Vector3 perpendicular = new Vector3(-dirToPlayer.y, dirToPlayer.x, 0).normalized;
        if (Time.frameCount % 120 < 60)
            perpendicular *= -1;
        float currentDistance = Vector3.Distance(transform.position, player.position);
        float targetDistance = Mathf.Clamp(currentDistance, OPTIMAL_DISTANCE_MIN, OPTIMAL_DISTANCE_MAX);
        Vector3 idealPos = player.transform.position - dirToPlayer * targetDistance;
        Vector3 circlePos = idealPos + perpendicular * 3f;
        Vector2 direction = (circlePos - transform.position).normalized;
        UpdateFacingDirection();
        rb.linearVelocity = direction * moveSpeed * circlingStrafeSpeed;
    }
    
    public void MoveTowardsOpponent()
    {
        if (agent.IsMovementDisabled) return;
        Vector2 direction = (player.position - transform.position).normalized;
        UpdateFacingDirection();
        float distanceMultiplier = 1.0f;
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer > OPTIMAL_DISTANCE_MAX)
        {
            distanceMultiplier = 1.3f;
        }
        rb.linearVelocity = direction * moveSpeed * distanceMultiplier;
    }
    
    public void MoveAwayFromOpponent()
    {
        if (agent.IsMovementDisabled) return;
        Vector2 direction = (transform.position - player.position).normalized;
        UpdateFacingDirection();
        rb.linearVelocity = direction * moveSpeed;
    }
    
    public void StopMovement()
    {
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.Sleep();
            rb.WakeUp();
        }
        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
        }
    }
}