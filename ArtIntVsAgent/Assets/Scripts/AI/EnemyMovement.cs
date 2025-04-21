using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    private GameObject player;
    public Enemy enemy;
    public Animator animator;
    public Rigidbody2D rb;
    [HideInInspector]
    public float distance;
    public Vector2 direction;
    Vector2 facingDirection;
    public bool isMovementDisabled = false;

    public LayerMask obstacleLayer;
    public int searchLimit = 100;

    private List<Vector2> path = new List<Vector2>();
    private int currentPathIndex;

    private GridSystem currentRoom;
    public float pathSmoothingTolerance = 0.5f;

    public float meleeDistance = 3f;
    public float rangedDistance = 7f;

    [HideInInspector]
    public bool isQuickReflexOn = false;

    void Start()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }
        player = ZolaImitation.Instance.centerOfThing;

        // Ensure initial values are correct
        FindCurrentRoom();
    }
    void FindCurrentRoom()
    {
        if (RoomManager.Instance != null && RoomManager.Instance.currentRoom != null)
        {
            currentRoom = RoomManager.Instance.currentRoom.GetComponent<GridSystem>();
            if (currentRoom == null)
            {
            }
        }
    }

    private bool shouldFlip = true;

    public void stopFlipping()
    {
        shouldFlip = false;
        animator.GetComponent<SpriteRenderer>().flipX = false;
    }    

    void Update()
    {
        if (enemy.AItype != AIType.ENEMY)
        {
            if (ZolaImitation.Instance.centerOfThing.transform.position.x <= enemy.centerOfEnemy.transform.position.x)
            {
                animator.SetFloat("FacingDirectionX", -1);
                if (shouldFlip)
                    animator.GetComponent<SpriteRenderer>().flipX = false;
            }
            else
            {
                animator.SetFloat("FacingDirectionX", 1);
                if (shouldFlip)
                    animator.GetComponent<SpriteRenderer>().flipX = true;
            }
        }

        Vector2 positionBeforeMovement = transform.position;
        Movement();
        Vector2 positionAfterMovement = transform.position;

        if (positionBeforeMovement != positionAfterMovement)
        {
        }
        else
        {
        }

        if ((Vector2)transform.position != rb.position)
        {
        }

    }

    public void Movement()
    {
        if (player != null)
            distance = Vector2.Distance(enemy.centerOfEnemy.position, ZolaImitation.Instance.centerOfThing.transform.position);

        if (isQuickReflexOn)
            return;

        if (enemy.isDead)
            return;

        if (!isMovementDisabled)
        {
            if (distance <= meleeDistance && enemy.canAttack)
            {
                enemy.MovementSpeed = 0;
                animator.SetFloat("Speed", 0);

                if (enemy != null && enemy.canAttack == true)
                {
                    enemy.Logic();
                }
            }
            else if (distance <= rangedDistance && enemy.canAttack)
            {
                enemy.MovementSpeed = 0;

                animator.SetFloat("Speed", 0);

                if (enemy != null && enemy.canAttack == true)
                {
                    enemy.Logic();
                }
            }
            else 
            {
                enemy.MovementSpeed = enemy.constantMovementSpeed;
                FindPath();
                if (path.Count > 0)
                {
                    FollowPath();
                    UpdateAnimation();
                }
                
            }
        }
    }

    public void StopEnemyMovement()
    {
        isMovementDisabled = true;
        enemy.MovementSpeed = 0;
        rb.linearVelocity = Vector2.zero;
        animator.SetFloat("Speed", 0);
    }

    void FindPath()
    {
        if (currentRoom == null)
        {
            FindCurrentRoom();
        }
        if (currentRoom != null)
        {
            Vector2 startPos = enemy.centerOfEnemy.position;
            Vector2 targetPos = ZolaImitation.Instance.centerOfThing.transform.position;

            if (enemy.AItype != AIType.ENEMY)
            {
                //same but with y offset
                //targetPos = new Vector2(ZolaImitation.Instance.centerOfThing.transform.position.x, Player.Instance.centerOfPlayer.transform.position.y + 1f);
            }


            path = AStar(startPos, targetPos);
            currentPathIndex = 0;
        }
    }



    void FollowPath()
    {
        if (path.Count == 0 || currentRoom == null) return;

        if (currentPathIndex >= path.Count)
        {
            path.Clear();
            return;
        }

        Vector2 currentPosition = enemy.centerOfEnemy.position;
        Vector2 targetPosition = path[currentPathIndex];

        direction = (targetPosition - currentPosition).normalized;
        float distanceToTarget = Vector2.Distance(currentPosition, targetPosition);


        if (distanceToTarget < 0.1f)
        {
            currentPathIndex++;
            if (currentPathIndex < path.Count)
            {
                direction = (path[currentPathIndex] - (Vector2)enemy.centerOfEnemy.position).normalized;
            }
        }

        // Check if we need to recalculate the path
        if (IsObstacleBetween(currentPosition, targetPosition))
        {
            FindPath();
        }
    }

    void UpdateAnimation()
    {
        animator.SetFloat("Horizontal", direction.x);
        animator.SetFloat("Vertical", direction.y);
        animator.SetFloat("Speed", direction.sqrMagnitude);

        if (direction != Vector2.zero)
        {
            if (enemy.AItype == AIType.ENEMY)
            {
                /*
                if (Mathf.Abs(direction.x) > Mathf.Abs(direction.y))
                {
                    facingDirection = new Vector2(Mathf.Sign(direction.x), 0f);
                    animator.SetFloat("FacingDirectionX", direction.x);
                    animator.SetFloat("FacingDirectionY", 0);
                }
                else
                {
                    facingDirection = new Vector2(0f, Mathf.Sign(direction.y));
                    animator.SetFloat("FacingDirectionY", direction.y);
                    animator.SetFloat("FacingDirectionX", 0);
                }*/
                
            }
        }
    }
    

    void FixedUpdate()
    {

        if (!isMovementDisabled && direction.sqrMagnitude > 0 && enemy.MovementSpeed > 0)
        {
            Vector2 movement = direction * enemy.MovementSpeed * Time.fixedDeltaTime;
            Vector2 newPosition = rb.position + movement;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.MovePosition(newPosition);
        }
       
    }


    List<Vector2> AStar(Vector2 start, Vector2 goal)
    {
        if (currentRoom == null)
        {
            return new List<Vector2>();
        }

        var openSet = new List<Vector2>();
        var closedSet = new HashSet<Vector2>();
        var cameFrom = new Dictionary<Vector2, Vector2>();
        var gScore = new Dictionary<Vector2, float>();
        var fScore = new Dictionary<Vector2, float>();

        openSet.Add(start);
        gScore[start] = 0;
        fScore[start] = HeuristicCostEstimate(start, goal);

        int iterations = 0;

        while (openSet.Count > 0 && iterations < searchLimit)
        {
            iterations++;
            var current = GetLowestFScore(openSet, fScore);

            if (Vector2.Distance(current, goal) < currentRoom.cellSize)
            {
                return SmoothPath(ReconstructPath(cameFrom, current));
            }

            openSet.Remove(current);
            closedSet.Add(current);

            foreach (var neighbor in GetNeighbors(current))
            {
                if (closedSet.Contains(neighbor)) continue;

                var tentativeGScore = gScore[current] + Vector2.Distance(current, neighbor);

                if (!openSet.Contains(neighbor))
                {
                    openSet.Add(neighbor);
                }
                else if (tentativeGScore >= gScore[neighbor])
                {
                    continue;
                }

                cameFrom[neighbor] = current;
                gScore[neighbor] = tentativeGScore;
                fScore[neighbor] = gScore[neighbor] + HeuristicCostEstimate(neighbor, goal);
            }
        }

        return new List<Vector2>();
    }
    float HeuristicCostEstimate(Vector2 start, Vector2 goal)
    {
        return Vector2.Distance(start, goal);
    }
    List<Vector2> SmoothPath(List<Vector2> path)
    {
        if (path.Count < 3) return path;

        var smoothedPath = new List<Vector2> { path[0] };
        for (int i = 1; i < path.Count - 1; i++)
        {
            Vector2 prev = smoothedPath[smoothedPath.Count - 1];
            Vector2 current = path[i];
            Vector2 next = path[i + 1];

            if (!IsValidMove(prev, next))
            {
                smoothedPath.Add(current);
            }
        }
        smoothedPath.Add(path[path.Count - 1]);

        return smoothedPath;
    }

    bool IsObstacleBetween(Vector2 start, Vector2 end)
    {
        RaycastHit2D hit = Physics2D.Linecast(start, end, obstacleLayer);
        if (hit.collider != null)
        {
            Debug.DrawLine(start, hit.point, Color.red, 0.1f);
            return true;
        }
        // Change the color to blue and make it transparent
        Color transparentBlue = new Color(0, 0, 1, 0.5f); // RGBA: Blue with 30% opacity
        Debug.DrawLine(start, end, transparentBlue, 0.1f);
        return false;
    }

    Vector2 GetLowestFScore(List<Vector2> openSet, Dictionary<Vector2, float> fScore)
    {
        Vector2 lowest = openSet[0];
        foreach (var item in openSet)
        {
            if (fScore.ContainsKey(item) && fScore[item] < fScore[lowest])
                lowest = item;
        }
        return lowest;
    }

    List<Vector2> GetNeighbors(Vector2 node)
    {
        var neighbors = new List<Vector2>();
        if (currentRoom == null) return neighbors;

        int nodeX, nodeY;
        currentRoom.WorldToCell(node, out nodeX, out nodeY);

        // Check in 8 directions
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0) continue;

                int checkX = nodeX + x;
                int checkY = nodeY + y;

                Vector2 worldPos = currentRoom.CellToWorld(checkX, checkY);

                if (IsValidMove(node, worldPos))
                {
                    neighbors.Add(worldPos);
                }
            }
        }

        return neighbors;
    }
    bool IsValidMove(Vector2 from, Vector2 to)
    {
        if (!currentRoom.IsWalkable(to))
            return false;

        // Check for diagonal movement
        if (from.x != to.x && from.y != to.y)
        {
            // Check if both cardinal directions are walkable
            Vector2 cardinal1 = new Vector2(from.x, to.y);
            Vector2 cardinal2 = new Vector2(to.x, from.y);
            if (!currentRoom.IsWalkable(cardinal1) || !currentRoom.IsWalkable(cardinal2))
                return false;
        }

        return !IsObstacleBetween(from, to);
    }

    List<Vector2> ReconstructPath(Dictionary<Vector2, Vector2> cameFrom, Vector2 current)
    {
        var path = new List<Vector2> { current };
        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Add(current);
        }
        path.Reverse();
        return path;
    }
    void OnDrawGizmos()
    {
        if (path != null && path.Count > 0)
        {
            // Stronger yellow color
            Gizmos.color = Color.yellow * 1.5f; // Brighter yellow

            // Increase line thickness
            for (int i = 0; i < path.Count - 1; i++)
            {
                Vector3 start = path[i];
                Vector3 end = path[i + 1];

                // Draw multiple lines to increase thickness
                for (int j = 0; j < 3; j++)
                {
                    Vector3 offset = Vector3.Cross(end - start, Vector3.forward).normalized * (0.05f * j);
                    Gizmos.DrawLine(start + offset, end + offset);
                    Gizmos.DrawLine(start - offset, end - offset);
                }
            }
        }

        // Rest of your OnDrawGizmos code...
        if (currentRoom != null)
        {
            Gizmos.color = Color.cyan;
            foreach (var neighbor in GetNeighbors(enemy.centerOfEnemy.position))
            {
                Gizmos.DrawWireSphere(neighbor, 0.1f);
            }
        }
    }
}
