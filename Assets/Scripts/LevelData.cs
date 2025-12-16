using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Level", menuName = "Sokoban/Level Data")]
public class LevelData : ScriptableObject
{
    [Header("Grid Configuration")]
    public int width = 10;
    public int height = 10;

    [Header("Level Elements")]
    public Vector2Int playerSpawn = Vector2Int.zero;
    public Vector2Int playerGoal = new Vector2Int(9, 9);
    public List<Vector2Int> wallCoordinates = new List<Vector2Int>();
    public List<Vector2Int> boxCoordinates = new List<Vector2Int>();
    public List<Vector2Int> keyCoordinates = new List<Vector2Int>();

    [Header("Level Metadata")]
    public string levelName = "Untitled Level";
    public string levelDescription = "";
    public int estimatedDifficulty = 1; // 1-10 scale, will be overridden by ML later

    /// <summary>
    /// Validates that the level data is consistent and properly formatted
    /// </summary>
    public bool IsValid()
    {
        // Check grid dimensions
        if (width <= 0 || height <= 0)
        {
            Debug.LogError($"Level {levelName}: Invalid grid dimensions ({width}x{height})");
            return false;
        }

        // Check player spawn is within bounds
        if (!IsPositionInBounds(playerSpawn))
        {
            Debug.LogError($"Level {levelName}: Player spawn {playerSpawn} is out of bounds");
            return false;
        }

        // Check player goal is within bounds
        if (!IsPositionInBounds(playerGoal))
        {
            Debug.LogError($"Level {levelName}: Player goal {playerGoal} is out of bounds");
            return false;
        }

        // Check player spawn and goal are different
        if (playerSpawn == playerGoal)
        {
            Debug.LogError($"Level {levelName}: Player spawn and goal cannot be the same position");
            return false;
        }

        // Check all wall coordinates are within bounds
        foreach (var wall in wallCoordinates)
        {
            if (!IsPositionInBounds(wall))
            {
                Debug.LogError($"Level {levelName}: Wall at {wall} is out of bounds");
                return false;
            }
        }

        // Check all box coordinates are within bounds
        foreach (var box in boxCoordinates)
        {
            if (!IsPositionInBounds(box))
            {
                Debug.LogError($"Level {levelName}: Box at {box} is out of bounds");
                return false;
            }
        }

        // Check all key coordinates are within bounds
        foreach (var key in keyCoordinates)
        {
            if (!IsPositionInBounds(key))
            {
                Debug.LogError($"Level {levelName}: Key at {key} is out of bounds");
                return false;
            }
        }

        // Check that boxes and keys have the same count
        if (boxCoordinates.Count < keyCoordinates.Count)
        {
            Debug.LogError($"Level {levelName}: Number of boxes ({boxCoordinates.Count}) can't be less than number of keys ({keyCoordinates.Count})");
            return false;
        }

        // Check for position conflicts
        if (HasPositionConflicts())
        {
            Debug.LogError($"Level {levelName}: Multiple objects occupy the same position");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Checks if a position is within the grid bounds
    /// </summary>
    public bool IsPositionInBounds(Vector2Int position)
    {
        return position.x >= 0 && position.x < width &&
               position.y >= 0 && position.y < height;
    }

    /// <summary>
    /// Checks if any two objects occupy the same position
    /// </summary>
    private bool HasPositionConflicts()
    {
        HashSet<Vector2Int> occupiedPositions = new HashSet<Vector2Int>();

        // Check player spawn
        if (!occupiedPositions.Add(playerSpawn))
            return true;

        // Check player goal (goal can overlap with keys, but not with other objects)
        // We'll allow goal-key overlap for more flexible level design

        // Check walls
        foreach (var wall in wallCoordinates)
        {
            if (!occupiedPositions.Add(wall))
                return true;
        }

        // Check boxes
        foreach (var box in boxCoordinates)
        {
            if (!occupiedPositions.Add(box))
                return true;
        }

        // Keys can overlap with goal, but not with other objects
        foreach (var key in keyCoordinates)
        {
            if (occupiedPositions.Contains(key) && key != playerGoal)
                return true;
        }

        return false;
    }

    /// <summary>
    /// Gets all positions that are occupied by static objects (walls)
    /// </summary>
    public HashSet<Vector2Int> GetStaticObstacles()
    {
        return new HashSet<Vector2Int>(wallCoordinates);
    }

    /// <summary>
    /// Gets all positions that contain movable objects (boxes)
    /// </summary>
    public HashSet<Vector2Int> GetMovableObjects()
    {
        return new HashSet<Vector2Int>(boxCoordinates);
    }

    /// <summary>
    /// Gets all key positions
    /// </summary>
    public HashSet<Vector2Int> GetKeyPositions()
    {
        return new HashSet<Vector2Int>(keyCoordinates);
    }

    /// <summary>
    /// Creates a deep copy of this level data
    /// </summary>
    public LevelData Clone()
    {
        LevelData clone = CreateInstance<LevelData>();
        clone.width = width;
        clone.height = height;
        clone.playerSpawn = playerSpawn;
        clone.playerGoal = playerGoal;
        clone.wallCoordinates = new List<Vector2Int>(wallCoordinates);
        clone.boxCoordinates = new List<Vector2Int>(boxCoordinates);
        clone.keyCoordinates = new List<Vector2Int>(keyCoordinates);
        clone.levelName = levelName + " (Clone)";
        clone.levelDescription = levelDescription;
        clone.estimatedDifficulty = estimatedDifficulty;
        return clone;
    }

    /// <summary>
    /// Converts level data to a feature vector for ML processing
    /// This will be expanded later for neural network input
    /// </summary>
    public float[] ToFeatureVector()
    {
        // Basic features - this will be expanded significantly
        return new float[]
        {
            width,
            height,
            wallCoordinates.Count,
            boxCoordinates.Count,
            keyCoordinates.Count,
            Vector2Int.Distance(playerSpawn, playerGoal),
            // More sophisticated features will be added here
        };
    }

#if UNITY_EDITOR
    /// <summary>
    /// Editor utility to visualize the level in the inspector
    /// </summary>
    [ContextMenu("Print Level Layout")]
    public void PrintLevelLayout()
    {
        Debug.Log($"Level: {levelName}");
        Debug.Log($"Grid: {width}x{height}");
        Debug.Log($"Player: {playerSpawn} -> {playerGoal}");
        Debug.Log($"Walls: {string.Join(", ", wallCoordinates)}");
        Debug.Log($"Boxes: {string.Join(", ", boxCoordinates)}");
        Debug.Log($"Keys: {string.Join(", ", keyCoordinates)}");
    }
#endif
}