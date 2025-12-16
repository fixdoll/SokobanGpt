using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;
using System.Collections;



#if UNITY_EDITOR
using UnityEditor;
#endif

public class GridLevelEditor : MonoBehaviour
{
    public enum BrushType
    {
        None,
        Wall,
        Box,
        Key,
        PlayerSpawn,
        PlayerGoal,
        Eraser
    }

    public static GridLevelEditor Instance;

    [Header("References")]
    public GridSystem gridSystem;
    public Camera editorCamera;
    public SaveLevelDialog saveLevelDialog; // Reference to save dialog
    public LoadLevelDialog loadLevelDialog; // Reference to load dialog

    [Header("Prefabs")]
    public GameObject wallPrefab;
    public GameObject boxPrefab;
    public GameObject keyPrefab;
    public GameObject playerPrefab;
    public GameObject goalPrefab;

    [Header("Level Data")]
    public int width = 10;
    public int height = 10;
    public string levelName = "New Level";
    public string levelDescription = "";

    [Header("Camera Pan Settings")]
    public float panSensitivity = 1.0f;

    // Current brush (set by UI buttons)
    private BrushType currentBrush = BrushType.None;

    // Right-click drag state
    private bool isRightClickDragging = false;
    private Vector3 lastMousePosition;

    // Runtime representation of the grid contents for the editor
    private Dictionary<Vector2Int, GameObject> wallObjects = new Dictionary<Vector2Int, GameObject>();
    private Dictionary<Vector2Int, GameObject> boxObjects = new Dictionary<Vector2Int, GameObject>();
    private Dictionary<Vector2Int, GameObject> keyObjects = new Dictionary<Vector2Int, GameObject>();
    private Vector2Int? playerSpawnPos;
    private GameObject playerObject;
    private Vector2Int? playerGoalPos;
    private GameObject goalObject;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (gridSystem == null)
        {
            Debug.LogError("GridLevelEditor: GridSystem reference is not set.");
            enabled = false;
            return;
        }

        if (editorCamera == null)
        {
            editorCamera = Camera.main;
        }

        // Ensure gridSystem uses our width/height
        gridSystem.width = width;
        gridSystem.height = height;
        gridSystem.InitializeGrid();
    }

    private void Update()
    {
        HandleRightClickDrag();
        HandleMouseInput();
    }

    private void HandleRightClickDrag()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            isRightClickDragging = false;
            return;
        }

        if (Input.GetMouseButtonDown(1))
        {
            isRightClickDragging = true;
            lastMousePosition = Input.mousePosition;
        }

        if (isRightClickDragging && Input.GetMouseButton(1))
        {
            Vector3 mouseDelta = Input.mousePosition - lastMousePosition;

            Vector3 worldDelta;
            if (editorCamera.orthographic)
            {
                float worldHeight = editorCamera.orthographicSize * 2f;
                float worldWidth = worldHeight * editorCamera.aspect;
                worldDelta = new Vector3(
                    -mouseDelta.x * (worldWidth / Screen.width),
                    -mouseDelta.y * (worldHeight / Screen.height),
                    0
                );
            }
            else
            {
                Plane plane = new Plane(Vector3.back, editorCamera.transform.position);
                Ray ray1 = editorCamera.ScreenPointToRay(lastMousePosition);
                Ray ray2 = editorCamera.ScreenPointToRay(Input.mousePosition);

                if (plane.Raycast(ray1, out float dist1) && plane.Raycast(ray2, out float dist2))
                {
                    Vector3 worldPos1 = ray1.GetPoint(dist1);
                    Vector3 worldPos2 = ray2.GetPoint(dist2);
                    worldDelta = worldPos1 - worldPos2;
                }
                else
                {
                    worldDelta = Vector3.zero;
                }
            }

            editorCamera.transform.position += worldDelta * panSensitivity;
            lastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(1))
        {
            isRightClickDragging = false;
        }
    }

    private void HandleMouseInput()
    {
        if (!Input.GetMouseButton(0) || isRightClickDragging)
            return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        Ray ray = editorCamera.ScreenPointToRay(Input.mousePosition);
        Plane plane = new Plane(Vector3.back, Vector3.zero);

        if (plane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            Vector2Int gridPos = gridSystem.GetGridPosition(hitPoint);

            if (gridPos.x < 0 || gridPos.x >= width || gridPos.y < 0 || gridPos.y >= height)
                return;

            ApplyBrushAt(gridPos);
        }
    }

    private void ApplyBrushAt(Vector2Int gridPos)
    {
        switch (currentBrush)
        {
            case BrushType.Wall:
                PlaceSingleAt(gridPos, wallPrefab, wallObjects);
                break;
            case BrushType.Box:
                PlaceSingleAt(gridPos, boxPrefab, boxObjects);
                break;
            case BrushType.Key:
                PlaceSingleAt(gridPos, keyPrefab, keyObjects);
                break;
            case BrushType.PlayerSpawn:
                PlaceUnique(ref playerSpawnPos, ref playerObject, gridPos, playerPrefab);
                break;
            case BrushType.PlayerGoal:
                PlaceUnique(ref playerGoalPos, ref goalObject, gridPos, goalPrefab);
                break;
            case BrushType.Eraser:
                EraseAt(gridPos);
                break;
        }
    }

    private void PlaceSingleAt(Vector2Int pos, GameObject prefab, Dictionary<Vector2Int, GameObject> dict)
    {
        EraseAt(pos);

        if (prefab == null)
        {
            Debug.LogWarning("GridLevelEditor: prefab not assigned for brush.");
            return;
        }

        if (dict.TryGetValue(pos, out GameObject existing))
        {
            Destroy(existing);
            dict.Remove(pos);
        }

        Vector3 worldPos = gridSystem.GetWorldPosition(pos);
        GameObject instance = Instantiate(prefab, worldPos, Quaternion.identity, transform);
        SetupGridSystemReference(instance);
        dict[pos] = instance;
    }

    private void PlaceUnique(ref Vector2Int? storedPos, ref GameObject storedObj, Vector2Int newPos, GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogWarning("GridLevelEditor: unique prefab not assigned.");
            return;
        }

        if (storedObj != null)
        {
            Destroy(storedObj);
        }

        EraseAt(newPos);

        Vector3 worldPos = gridSystem.GetWorldPosition(newPos);
        storedObj = Instantiate(prefab, worldPos, Quaternion.identity, transform);
        SetupGridSystemReference(storedObj);
        storedPos = newPos;
    }

    private void EraseAt(Vector2Int pos)
    {
        if (wallObjects.TryGetValue(pos, out GameObject wall))
        {
            Destroy(wall);
            wallObjects.Remove(pos);
        }
        if (boxObjects.TryGetValue(pos, out GameObject box))
        {
            Destroy(box);
            boxObjects.Remove(pos);
        }
        if (keyObjects.TryGetValue(pos, out GameObject key))
        {
            Destroy(key);
            keyObjects.Remove(pos);
        }

        if (playerSpawnPos.HasValue && playerSpawnPos.Value == pos)
        {
            if (playerObject != null) Destroy(playerObject);
            playerObject = null;
            playerSpawnPos = null;
        }

        if (playerGoalPos.HasValue && playerGoalPos.Value == pos)
        {
            if (goalObject != null) Destroy(goalObject);
            goalObject = null;
            playerGoalPos = null;
        }
    }

    private void SetupGridSystemReference(GameObject instance)
    {
        if (instance == null || gridSystem == null) return;

        BoxController boxController = instance.GetComponent<BoxController>();
        if (boxController != null)
        {
            boxController.gridSystem = gridSystem;
        }

        KeyTile keyTile = instance.GetComponent<KeyTile>();
        if (keyTile != null)
        {
            keyTile.gridSystem = gridSystem;
        }

        WallController wallController = instance.GetComponent<WallController>();
        if (wallController != null)
        {
            wallController.gridSystem = gridSystem;
        }

        GoalTile goalTile = instance.GetComponent<GoalTile>();
        if (goalTile != null)
        {
            goalTile.gridSystem = gridSystem;
        }

        PlayerController playerController = instance.GetComponent<PlayerController>();
        if (playerController != null)
        {
            playerController.gridSystem = gridSystem;
        }
    }

    public void SetBrush_None() { currentBrush = BrushType.None; }
    public void SetBrush_Wall() { currentBrush = BrushType.Wall; }
    public void SetBrush_Box() { currentBrush = BrushType.Box; }
    public void SetBrush_Key() { currentBrush = BrushType.Key; }
    public void SetBrush_PlayerSpawn() { currentBrush = BrushType.PlayerSpawn; }
    public void SetBrush_PlayerGoal() { currentBrush = BrushType.PlayerGoal; }
    public void SetBrush_Eraser() { currentBrush = BrushType.Eraser; }

    public void ClearGrid()
    {
        foreach (var kvp in wallObjects) Destroy(kvp.Value);
        foreach (var kvp in boxObjects) Destroy(kvp.Value);
        foreach (var kvp in keyObjects) Destroy(kvp.Value);
        wallObjects.Clear();
        boxObjects.Clear();
        keyObjects.Clear();

        if (playerObject != null) Destroy(playerObject);
        if (goalObject != null) Destroy(goalObject);
        playerObject = null;
        goalObject = null;
        playerSpawnPos = null;
        playerGoalPos = null;
    }

    private LevelData BuildLevelDataSnapshot()
    {
        LevelData data = ScriptableObject.CreateInstance<LevelData>();
        data.width = width;
        data.height = height;
        data.levelName = levelName;
        data.levelDescription = levelDescription;

        data.wallCoordinates = new List<Vector2Int>(wallObjects.Keys);
        data.boxCoordinates = new List<Vector2Int>(boxObjects.Keys);
        data.keyCoordinates = new List<Vector2Int>(keyObjects.Keys);

        data.playerSpawn = playerSpawnPos ?? Vector2Int.zero;
        data.playerGoal = playerGoalPos ?? (width > 0 && height > 0 ? new Vector2Int(width - 1, height - 1) : Vector2Int.zero);

        return data;
    }

    public void ValidateCurrentLevel()
    {
        LevelData snapshot = BuildLevelDataSnapshot();
        bool isValidData = snapshot.IsValid();
        bool solvable = isValidData && LevelValidator.IsLevelSolvable(snapshot);

        if (!isValidData)
        {
            Debug.LogWarning($"Editor Level '{snapshot.levelName}' has invalid configuration (see previous log).");
        }
        else
        {
            Debug.Log($"Editor Level '{snapshot.levelName}' solvable? {solvable}");
        }
    }

    public void TestCurrentLevel()
    {
        LevelData snapshot = BuildLevelDataSnapshot();
        bool isValidData = snapshot.IsValid();
        bool solvable = isValidData && LevelValidator.IsLevelSolvable(snapshot);

        if (!isValidData)
        {
            Debug.LogWarning($"Editor Level '{snapshot.levelName}' has invalid configuration (see previous log).");
        }
        else
        {
            StartCoroutine(LoadPlayScene(snapshot));
            
        }
    }

    private IEnumerator LoadPlayScene(LevelData data)
    {
        var load = SceneManager.LoadSceneAsync("PlayScene", LoadSceneMode.Additive);

        yield return load;
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("PlayScene"));
        FlowController.Instance.isEditorTest = true;
        FlowController.Instance.startingLevelData = data;
        LevelLoadController.Instance.LoadLevel(data);
        yield return SceneManager.UnloadSceneAsync("EditorScene");

        yield return null;
    }

    public void OpenSaveDialog()
    {
        if (saveLevelDialog == null)
        {
            Debug.LogError("SaveLevelDialog reference is not set! Please assign it in the Inspector.");
            // Fallback to direct save if dialog is not available
            SaveLevelToScriptableObject();
            return;
        }

        saveLevelDialog.ShowDialog(this);
    }

    public void OpenLoadDialog()
    {
        if (loadLevelDialog == null)
        {
            Debug.LogError("LoadLevelDialog reference is not set! Please assign it in the Inspector.");
            return;
        }

        loadLevelDialog.ShowDialog(this);
    }

    /// <summary>
    /// Saves the level to a ScriptableObject asset. Now called by SaveLevelDialog.
    /// Can also be called directly for quick saves.
    /// </summary>
    public void SaveLevelToScriptableObject()
    {
#if UNITY_EDITOR
        LevelData snapshot = BuildLevelDataSnapshot();

        if (!snapshot.IsValid())
        {
            Debug.LogError($"Cannot save level '{snapshot.levelName}': Level data is invalid!");
            return;
        }

        string resourcesPath = "Assets/Resources";
        string levelsPath = "Assets/Resources/Levels";

        if (!AssetDatabase.IsValidFolder(resourcesPath))
        {
            AssetDatabase.CreateFolder("Assets", "Resources");
        }

        if (!AssetDatabase.IsValidFolder(levelsPath))
        {
            AssetDatabase.CreateFolder(resourcesPath, "Levels");
        }

        string assetName = SanitizeFileName(snapshot.levelName);
        string assetPath = $"{levelsPath}/{assetName}.asset";

        LevelData existingAsset = AssetDatabase.LoadAssetAtPath<LevelData>(assetPath);

        if (existingAsset != null)
        {
            existingAsset.width = snapshot.width;
            existingAsset.height = snapshot.height;
            existingAsset.levelName = snapshot.levelName;
            existingAsset.levelDescription = snapshot.levelDescription;
            existingAsset.playerSpawn = snapshot.playerSpawn;
            existingAsset.playerGoal = snapshot.playerGoal;
            existingAsset.wallCoordinates = new List<Vector2Int>(snapshot.wallCoordinates);
            existingAsset.boxCoordinates = new List<Vector2Int>(snapshot.boxCoordinates);
            existingAsset.keyCoordinates = new List<Vector2Int>(snapshot.keyCoordinates);

            EditorUtility.SetDirty(existingAsset);
            AssetDatabase.SaveAssets();
            Debug.Log($"Updated existing level asset: {assetPath}");
        }
        else
        {
            AssetDatabase.CreateAsset(snapshot, assetPath);
            AssetDatabase.SaveAssets();
            Debug.Log($"Created new level asset: {assetPath}");
        }
#else
        Debug.LogWarning("SaveLevelToScriptableObject is only available in the Unity Editor. Use JSON save for builds.");
#endif
    }

    public void LoadLevelFromScriptableObjectByName(string levelName)
    {
        LevelData levelData = Resources.Load<LevelData>($"Levels/{levelName}");

        if (levelData == null)
        {
            Debug.LogError($"Level '{levelName}' not found in Resources/Levels!");
            return;
        }

        LoadLevelDataIntoEditor(levelData);
    }

    public void LoadLevelFromScriptableObject(LevelData levelData)
    {
        if (levelData == null)
        {
            Debug.LogError("LevelData is null!");
            return;
        }

        LoadLevelDataIntoEditor(levelData);
    }

    public LevelData[] GetAvailableLevels()
    {
        LevelData[] levels = Resources.LoadAll<LevelData>("Levels");
        return levels;
    }

    public void LoadLevelDataIntoEditor(LevelData levelData)
    {
        if (!levelData.IsValid())
        {
            Debug.LogError($"Cannot load level '{levelData.levelName}': Level data is invalid!");
            return;
        }

        ClearGrid();

        width = levelData.width;
        height = levelData.height;
        levelName = levelData.levelName;
        levelDescription = levelData.levelDescription;

        gridSystem.width = width;
        gridSystem.height = height;
        gridSystem.InitializeGrid();

        foreach (var wallPos in levelData.wallCoordinates)
        {
            PlaceSingleAt(wallPos, wallPrefab, wallObjects);
        }

        foreach (var boxPos in levelData.boxCoordinates)
        {
            PlaceSingleAt(boxPos, boxPrefab, boxObjects);
        }

        foreach (var keyPos in levelData.keyCoordinates)
        {
            PlaceSingleAt(keyPos, keyPrefab, keyObjects);
        }

        if (levelData.playerSpawn.x >= 0 && levelData.playerSpawn.x < width &&
            levelData.playerSpawn.y >= 0 && levelData.playerSpawn.y < height)
        {
            PlaceUnique(ref playerSpawnPos, ref playerObject, levelData.playerSpawn, playerPrefab);
        }

        if (levelData.playerGoal.x >= 0 && levelData.playerGoal.x < width &&
            levelData.playerGoal.y >= 0 && levelData.playerGoal.y < height)
        {
            PlaceUnique(ref playerGoalPos, ref goalObject, levelData.playerGoal, goalPrefab);
        }

        Debug.Log($"Loaded level '{levelData.levelName}' into editor!");
    }

    private string SanitizeFileName(string fileName)
    {
        char[] invalidChars = System.IO.Path.GetInvalidFileNameChars();
        string sanitized = fileName;

        foreach (char c in invalidChars)
        {
            sanitized = sanitized.Replace(c, '_');
        }

        sanitized = sanitized.Trim(' ', '.');

        if (string.IsNullOrEmpty(sanitized))
        {
            sanitized = "Untitled Level";
        }

        return sanitized;
    }
}