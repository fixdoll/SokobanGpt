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
        // Center the grid so the middle cell is at world (0,0)
        float offsetX = -(width - 1) * cellSize / 2.0f;
        float offsetY = -(height - 1) * cellSize / 2.0f;
        return new Vector3(gridPosition.x * cellSize + offsetX, gridPosition.y * cellSize + offsetY, 0);
    }

    public Vector2Int GetGridPosition(Vector3 worldPosition)
    {
        // Apply the same offset when converting from world to grid
        float offsetX = -(width - 1) * cellSize / 2.0f;
        float offsetY = -(height - 1) * cellSize / 2.0f;
        float adjustedX = worldPosition.x - offsetX;
        float adjustedY = worldPosition.y - offsetY;
        
        // Use RoundToInt instead of FloorToInt to find the nearest cell center
        // This ensures clicks near cell boundaries map to the correct cell
        int gridX = Mathf.RoundToInt(adjustedX / cellSize);
        int gridY = Mathf.RoundToInt(adjustedY / cellSize);
        
        return new Vector2Int(gridX, gridY);
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

                GUIStyle semiTransparentBox = new GUIStyle(GUI.skin.box);
                semiTransparentBox.normal.background = MakeTintedTexture(new Color(0, 0, 0, 0.22f));

                Texture2D MakeTintedTexture(Color color)
                {
                    Texture2D texture = new Texture2D(1, 1);
                    texture.SetPixel(0, 0, color);
                    texture.Apply();
                    return texture;
                }

                GUI.Label(new Rect(adjustedScreenPosition.x - 25, adjustedScreenPosition.y + 5, 50, 20), $"({x},{y})");
                GUI.Box(new Rect(adjustedScreenPosition.x - 25, adjustedScreenPosition.y - 25, 50, 50), "", semiTransparentBox);
            }
        }
    }

    public void InitializeGrid()
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
