using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSystem : MonoBehaviour
{
    public Vector2 roomSize = new Vector2(20, 20);
    public float cellSize = 1f;
    public LayerMask obstacleLayer;

    private bool[,] grid;

    private void Start()
    {
        GenerateGrid();
    }
    public void GenerateGrid()
    {
        int width = Mathf.CeilToInt(roomSize.x / cellSize);
        int height = Mathf.CeilToInt(roomSize.y / cellSize);

        grid = new bool[width, height];

        Vector2 bottomLeft = (Vector2)transform.position - roomSize / 2f;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 worldPoint = bottomLeft + new Vector2(x * cellSize + cellSize / 2, y * cellSize + cellSize / 2);
                grid[x, y] = !Physics2D.OverlapCircle(worldPoint, cellSize / 2f, obstacleLayer);
            }
        }
    }

    public bool IsWalkable(Vector2 worldPosition)
    {
        int x, y;
        if (WorldToCell(worldPosition, out x, out y))
        {
            return grid[x, y];
        }
        return false;
    }

    public Vector2 CellToWorld(int x, int y)
    {
        Vector2 bottomLeft = (Vector2)transform.position - roomSize / 2f;
        return bottomLeft + new Vector2(x * cellSize + cellSize / 2, y * cellSize + cellSize / 2);
    }

    public bool WorldToCell(Vector2 worldPosition, out int x, out int y)
    {
        Vector2 localPosition = worldPosition - ((Vector2)transform.position - roomSize / 2f);
        x = Mathf.FloorToInt(localPosition.x / cellSize);
        y = Mathf.FloorToInt(localPosition.y / cellSize);

        return x >= 0 && x < grid.GetLength(0) && y >= 0 && y < grid.GetLength(1);
    }

    void OnDrawGizmosSelected()
    {
        if (grid == null)
        {
            GenerateGrid();
        }
        Vector2 bottomLeft = (Vector2)transform.position - roomSize / 2f;
        for (int x = 0; x < grid.GetLength(0); x++)
        {
            for (int y = 0; y < grid.GetLength(1); y++)
            {
                Vector2 cellCenter = CellToWorld(x, y);
                if (grid[x, y])
                {
                    Gizmos.color = new Color(0, 1, 0, 0.1f); // Semi-transparent green for walkable area
                }
                else
                {
                    Gizmos.color = new Color(1, 0, 0, 0.1f); // Semi-transparent red for non-walkable area
                }
                Gizmos.DrawCube(cellCenter, Vector2.one * (cellSize - 0.1f));

                // Draw wire outline
                Gizmos.color = grid[x, y] ? Color.green : Color.red;
                Gizmos.DrawWireCube(cellCenter, Vector2.one * (cellSize - 0.1f));
            }
        }
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, roomSize);
    }
}
