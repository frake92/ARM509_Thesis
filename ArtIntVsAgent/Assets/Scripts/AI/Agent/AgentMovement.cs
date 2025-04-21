using System.Collections;
using UnityEngine;

public class AgentMovement : MonoBehaviour
{
    private ZolaImitation agent;
    private Transform bossTarget;
    private GameObject centerOfThing;
    private Animator animator;
    
    [SerializeField] private float moveSpeed = 3.5f;
    [SerializeField] private float minMovementThreshold = 0.01f;
    
    [SerializeField] private float optimalMinDistance = 2.5f;
    [SerializeField] private float optimalMaxDistance = 5.0f;
    [SerializeField] private float circlingStrafeSpeed = 1.8f;
    [SerializeField] private float repositionCooldown = 3.0f;
    [SerializeField] [Range(0.5f, 5f)] private float movementAggressiveness = 3f;
    private Vector3 lastPosition;
    private float lastPositionTime;
    private bool isDefinitelyStuck = false;
    private float stuckTime = 0f;
    private float lastActualMovementTime = 0f;
    private Vector3 lastFramePosition;
    private int stationaryFrames = 0;
    
    private float lastRepositionTime = 0f;
    private Vector3 repositionTargetPoint;
    private bool isRepositioning = false;
    
    private const float STUCK_CHECK_INTERVAL = 0.5f;
    private const float STUCK_MOVEMENT_THRESHOLD = 0.05f;
    private const float TOO_FAR_DISTANCE = 10.0f;
    
    public void Initialize(ZolaImitation agent, Transform bossTarget, GameObject centerOfThing)
    {
        this.agent = agent;
        this.bossTarget = bossTarget;
        this.centerOfThing = centerOfThing;
        this.animator = agent.animator;
        
        ResetState();
    }
    
    public void ResetState()
    {
        lastPosition = transform.position;
        lastPositionTime = Time.time;
        lastFramePosition = transform.position;
        lastActualMovementTime = Time.time;
        stationaryFrames = 0;
        isDefinitelyStuck = false;
        stuckTime = 0f;
        
        lastRepositionTime = 0f;
        isRepositioning = false;
    }
    
    public void UpdateFacingDirection()
    {
        if (bossTarget == null) return;
        
        if (bossTarget.position.x <= centerOfThing.transform.position.x)
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
    
    public void CheckIfStuck()
    {
        float timeSinceLastPositionCheck = Time.time - lastPositionTime;
        if (timeSinceLastPositionCheck > STUCK_CHECK_INTERVAL)
        {
            float distanceMoved = Vector3.Distance(transform.position, lastPosition);
            float distanceToBoss = Vector3.Distance(transform.position, bossTarget.position);
            
            isDefinitelyStuck = distanceMoved < STUCK_MOVEMENT_THRESHOLD && 
                              animator.GetFloat("Speed") > 0 && 
                              distanceToBoss > 1.5f;
            
            lastPosition = transform.position;
            lastPositionTime = Time.time;
            
            if (isDefinitelyStuck)
            {
                stuckTime += STUCK_CHECK_INTERVAL;
                if (stuckTime > 1.0f && agent.CanAttack)
                {
                    agent.combat.PerformAttack(AttackType.DimensionalWave);
                }
            }
            else
            {
                stuckTime = 0f;
            }
        }
    }
    
    public void AutoMoveBasedOnDistance()
    {
        if (agent.IsAttacking || agent.IsMovementDisabled || bossTarget == null)
            return;
            
        float distanceToBoss = Vector3.Distance(transform.position, bossTarget.position);
        int frameCount = Time.frameCount;
        int movementDecisionFrame = (frameCount % 120);

        if (distanceToBoss > TOO_FAR_DISTANCE)
        {
            MoveTowardsBoss();
            return;
        }
        
        if (distanceToBoss < optimalMinDistance)
        {
            if (movementDecisionFrame < 80)
            {
                MoveAwayFromBoss();
            }
            else if (movementDecisionFrame < 100) 
            {
                CircleAroundBoss();
            }
            else
            {
                RepositionToOptimalRange();
            }
            return;
        }
        
        if (distanceToBoss <= optimalMaxDistance)
        {
            if (movementDecisionFrame < 30)
            {
                CircleAroundBoss();
            }
            else if (movementDecisionFrame < 40)
            {
                MoveAwayFromBoss();
            }
            else if (movementDecisionFrame < 50)
            {
                MoveTowardsBoss();
            }
            else if (movementDecisionFrame < 70)
            {
                RepositionToOptimalRange();
            }

            return;
        }
        
        if (movementDecisionFrame < 80)
        {
            MoveTowardsBoss();
        }
        else if (movementDecisionFrame < 100)
        {
            RepositionToOptimalRange();
        }
        else
        {
            CircleAroundBoss();
        }
        
        if (Time.time - lastActualMovementTime > 0.8f && !isDefinitelyStuck && !agent.IsAttacking)
        {
            if (distanceToBoss < 1.8f)
            {
                MoveAwayFromBoss();
            }
            else if (distanceToBoss > 6f)
            {
                MoveTowardsBoss();
            }
            else
            {
                if (frameCount % 2 == 0)
                {
                    CircleAroundBoss();
                }
                else
                {
                    RepositionToOptimalRange();
                }
            }
        }
    }
    
    public void MoveTowardsBoss()
    {
        if (bossTarget == null || agent.IsMovementDisabled) return;
        
        Vector2 direction = (bossTarget.position - transform.position).normalized;
        UpdateFacingDirection();
        
        Vector3 beforePosition = transform.position;
        float previousDistance = Vector3.Distance(transform.position, bossTarget.position);
        
        float distanceMultiplier = previousDistance > 12f ? 1.5f : 1.2f;
        
        transform.position += (Vector3)direction * moveSpeed * distanceMultiplier * Time.deltaTime;
        
        ProcessMovementResults(beforePosition, previousDistance);
    }
    
    public void MoveAwayFromBoss()
    {
        if (bossTarget == null || agent.IsMovementDisabled) return;
            
        Vector2 direction = (transform.position - bossTarget.position).normalized;
        UpdateFacingDirection();
        
        Vector3 beforePosition = transform.position;
        
        float distanceToBoss = Vector3.Distance(transform.position, bossTarget.position);
        float speedMultiplier;

        speedMultiplier = 1.2f;
            
        transform.position += (Vector3)direction * moveSpeed * speedMultiplier * Time.deltaTime;
        
        ProcessMovementResults(beforePosition, 0f);
    }
    
    public void CircleAroundBoss()
    {
        if (bossTarget == null || agent.IsMovementDisabled) return;
        
        Vector3 dirToBoss = (bossTarget.position - transform.position).normalized;
        Vector3 perpendicular = new Vector3(-dirToBoss.y, dirToBoss.x, 0).normalized;
        
        if (Time.frameCount % 120 < 60)
            perpendicular *= -1;
        
        float currentDistance = Vector3.Distance(transform.position, bossTarget.position);
        float targetDistance = Mathf.Clamp(currentDistance, optimalMinDistance, optimalMaxDistance);
        
        Vector3 idealPos = bossTarget.position - dirToBoss * targetDistance;
        Vector3 circlePos = idealPos + perpendicular * 3f;
        
        Vector2 direction = (circlePos - transform.position).normalized;
        Vector3 beforePosition = transform.position;
        
        transform.position += (Vector3)direction * moveSpeed * circlingStrafeSpeed * Time.deltaTime;
        
        ProcessMovementResults(beforePosition, 0f);
    }
    
    public void RepositionToOptimalRange()
    {
        if (bossTarget == null || agent.IsMovementDisabled) return;
        
        if (Time.time - lastRepositionTime < repositionCooldown && !isRepositioning)
            return;
            
        float currentDistance = Vector3.Distance(transform.position, bossTarget.position);
        
        if (currentDistance >= optimalMinDistance && currentDistance <= optimalMaxDistance && !isRepositioning)
            return;
        
        Vector3 dirToBoss = (bossTarget.position - transform.position).normalized;
        float optimalDistance = (optimalMinDistance + optimalMaxDistance) / 2f;
        
        Quaternion rotation = Quaternion.Euler(0, 0, randomAngle);
        Vector3 rotatedDir = rotation * dirToBoss;
        
        if (!isRepositioning)
        {
            repositionTargetPoint = bossTarget.position - rotatedDir * optimalDistance;
            isRepositioning = true;
            lastRepositionTime = Time.time;
        }
        
        Vector2 direction = (repositionTargetPoint - transform.position).normalized;
        Vector3 beforePosition = transform.position;
        
        transform.position += (Vector3)direction * moveSpeed * 1.2f * Time.deltaTime;
        
        if (Vector3.Distance(transform.position, repositionTargetPoint) < 0.5f)
        {
            isRepositioning = false;
            agent.AddReward(0.05f);
        }
        
        ProcessMovementResults(beforePosition, 0f);
    }
    
    private void ProcessMovementResults(Vector3 beforePosition, float previousDistance)
    {
        float moveDelta = Vector3.Distance(beforePosition, transform.position);
        
        if (moveDelta > minMovementThreshold)
        {
            lastActualMovementTime = Time.time;
            stationaryFrames = 0;
            
            agent.AddReward(0.002f); 
            if (previousDistance > 0)
            {
                float newDistance = Vector3.Distance(transform.position, bossTarget.position);
                
                if ((previousDistance <= optimalMinDistance && newDistance > optimalMinDistance && newDistance <= optimalMaxDistance) ||
                    (previousDistance > optimalMaxDistance && newDistance <= optimalMaxDistance && newDistance > optimalMinDistance))
                {
                    agent.AddReward(0.005f);
                    }
            }
        }
        else
        {
            stationaryFrames++;
            if (stationaryFrames > 2)
            {
                if (Time.time - lastActualMovementTime > 0.8f)
                {
                    isDefinitelyStuck = true;
                    stuckTime += Time.deltaTime;
                }
            }
        }
    }
    
    public void StopMovement()
    {
        if (agent.rb != null)
        {
            agent.rb.linearVelocity = Vector2.zero;
            agent.rb.angularVelocity = 0f;
            
            agent.rb.Sleep();
            agent.rb.WakeUp();
        }
        
        if (animator != null)
        {
            animator.SetFloat("Speed", 0f);
        }
    }
    
    public bool IsStuck => isDefinitelyStuck;
    public float StuckTime => stuckTime;
    public float LastActualMovementTime => lastActualMovementTime;
    public Vector3 LastFramePosition => lastFramePosition;
    public int StationaryFrames => stationaryFrames;

    public void SetStationaryFrames(int value) => stationaryFrames = value;
    public void UpdateLastFramePosition() => lastFramePosition = transform.position;
    
    public void ResetStuckState()
    {
        isDefinitelyStuck = false;
        stuckTime = 0f;
        StopMovement();
    }
}