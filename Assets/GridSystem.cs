using System.Collections.Generic;
using UnityEngine;

public class GridSystem : MonoBehaviour
{
    public int width, height;
    public float cellSize;
    private GridCell[,] grid;

    private void Awake()
    {
        grid = new GridCell[width, height];
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                grid[x, y] = new GridCell(new Vector2Int(x, y));
            }
        }
    }

    public GridCell GetGridCell(Vector2Int position)
    {
        if (position.x >= 0 && position.x < width && position.y >= 0 && position.y < height)
        {
            return grid[position.x, position.y];
        }
        return null;
    }

    public Vector3 GetWorldPosition(Vector2Int gridPosition)
    {
        return new Vector3(gridPosition.x * cellSize, gridPosition.y * cellSize, 0);
    }

    public Vector2Int GetGridPosition(Vector3 worldPosition)
    {
        return new Vector2Int(Mathf.FloorToInt(worldPosition.x / cellSize), Mathf.FloorToInt(worldPosition.y / cellSize));
    }

    private void OnGUI()
    {
        // Display grid coordinates for each cell
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2 cellWorldPosition = GetWorldPosition(new Vector2Int(x, y));
                Vector3 screenPosition = Camera.main.WorldToScreenPoint(cellWorldPosition);

                // Convert the screen position from bottom-left origin to top-left origin for OnGUI()
                Vector2 adjustedScreenPosition = new Vector2(screenPosition.x, Screen.height - screenPosition.y);

                GUI.Label(new Rect(adjustedScreenPosition.x - 15, adjustedScreenPosition.y - 10, 50, 20), $"({x},{y})");
            }
        }
    }

}

public class GridCell
{
    public Vector2Int position;
    public GameObject occupant;

    public bool hasWall;

    public GridCell(Vector2Int position)
    {
        this.position = position;
    }

    public bool IsOccupied()
    {
        return occupant != null;
    }

    public void SetOccupant(GameObject obj)
    {
        occupant = obj;
    }

    public void SetWall(bool wall)
    {
        hasWall = wall;
    }

    public void ClearOccupant()
    {
        occupant = null;
    }
}
