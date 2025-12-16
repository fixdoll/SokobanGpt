using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelLoadController : MonoBehaviour
{
    public static LevelLoadController Instance;

    [Header("Prefab References")]
    public GameObject playerPrefab;
    public GameObject boxPrefab;
    public GameObject keyTilePrefab;
    public GameObject goalTilePrefab;
    public GameObject wallPrefab;

    [Header("Grid System")]
    public GridSystem gridSystem;

    [Header("Controllers")]
    public CameraFollow cameraFollow;

    private List<GameObject> levelObjects = new List<GameObject>();

    private void Awake()
    {
        Instance = this;
    }

    public bool LoadLevel(LevelData levelData)
    {
        if (levelData == null)
        {
            Debug.LogError("LevelData is null!");
            return false;
        }

        if (!levelData.IsValid())
        {
            Debug.LogError($"LevelData {levelData.levelName} is not valid!");
            return false;
        }

        // Clear existing level
        ClearLevel();

        // Configure grid system
        gridSystem.width = levelData.width;
        gridSystem.height = levelData.height;

        // Reinitialize grid
        gridSystem.InitializeGrid();

        // Create walls
        foreach (var wallPos in levelData.wallCoordinates)
        {
            CreateWall(wallPos);
        }

        // Create keys
        foreach (var keyPos in levelData.keyCoordinates)
        {
            CreateKey(keyPos);
        }

        // Create boxes
        foreach (var boxPos in levelData.boxCoordinates)
        {
            CreateBox(boxPos);
        }

        // Create goal
        CreateGoal(levelData.playerGoal);

        // Create/position player
        GameObject player = CreatePlayer(levelData.playerSpawn);

        // Setup camera
        if (cameraFollow != null)
        {
            cameraFollow.target = player.transform;
        }

        Debug.Log($"Level '{levelData.levelName}' loaded successfully!");
        return true;
    }

    /// <summary>
    /// Clears all level objects from the scene
    /// </summary>
    public void ClearLevel()
    {
        foreach (GameObject obj in levelObjects)
        {
            if (obj != null)
            {
                DestroyImmediate(obj);
            }
        }
        levelObjects.Clear();
    }

    private GameObject CreatePlayer(Vector2Int gridPos)
    {
        Vector3 worldPos = gridSystem.GetWorldPosition(gridPos);
        GameObject player = Instantiate(playerPrefab, worldPos, Quaternion.identity);

        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc != null)
        {
            pc.gridSystem = gridSystem;
            pc.boxes = FindObjectsOfType<BoxController>().ToList();
            pc.keyTiles = FindObjectsOfType<KeyTile>().ToList();
        }

        levelObjects.Add(player);
        return player;
    }

    private GameObject CreateBox(Vector2Int gridPos)
    {
        Vector3 worldPos = gridSystem.GetWorldPosition(gridPos);
        GameObject box = Instantiate(boxPrefab, worldPos, Quaternion.identity);

        BoxController bc = box.GetComponent<BoxController>();
        if (bc != null)
        {
            bc.gridSystem = gridSystem;
        }

        levelObjects.Add(box);
        return box;
    }

    private GameObject CreateKey(Vector2Int gridPos)
    {
        Vector3 worldPos = gridSystem.GetWorldPosition(gridPos);
        GameObject key = Instantiate(keyTilePrefab, worldPos, Quaternion.identity);

        KeyTile kt = key.GetComponent<KeyTile>();
        if (kt != null)
        {
            kt.gridSystem = gridSystem;
        }

        levelObjects.Add(key);
        return key;
    }

    private GameObject CreateGoal(Vector2Int gridPos)
    {
        Vector3 worldPos = gridSystem.GetWorldPosition(gridPos);
        GameObject goal = Instantiate(goalTilePrefab, worldPos, Quaternion.identity);

        GoalTile gt = goal.GetComponent<GoalTile>();
        if (gt != null)
        {
            gt.gridSystem = gridSystem;
        }

        levelObjects.Add(goal);
        return goal;
    }

    private GameObject CreateWall(Vector2Int gridPos)
    {
        Vector3 worldPos = gridSystem.GetWorldPosition(gridPos);
        GameObject wall = Instantiate(wallPrefab, worldPos, Quaternion.identity);

        WallController wc = wall.GetComponent<WallController>();
        if (wc != null)
        {
            wc.gridSystem = gridSystem;
        }

        levelObjects.Add(wall);
        return wall;
    }
}