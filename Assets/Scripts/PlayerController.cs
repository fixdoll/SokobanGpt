using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public GridSystem gridSystem;
    private Vector2Int gridPosition;
    public float moveDelay = 0.2f;
    private float moveCooldown;

    private Stack<GameState> gameStateHistory = new Stack<GameState>();
    public List<BoxController> boxes;
    public List<KeyTile> keyTiles;

    [SerializeField] private Sprite spriteDown;
    [SerializeField] private Sprite spriteUp;
    [SerializeField] private Sprite spriteLeft;
    [SerializeField] private Sprite spriteRight;
    private SpriteRenderer spriteRenderer;


    private bool isInEditorScene = false;

    private void Start()
    {
        spriteRenderer = gameObject.GetComponent<SpriteRenderer>();

        // Check if we're in the editor scene (where PlayerController is just a visual placeholder)
        isInEditorScene = SceneManager.GetActiveScene().name == "EditorScene";

        if (isInEditorScene)
        {
            // In editor scene, just set position and disable game logic
            if (gridSystem != null)
            {
                gridPosition = gridSystem.GetGridPosition(transform.position);
            }
            enabled = false; // Disable Update() in editor scene
            return;
        }

        gridPosition = gridSystem.GetGridPosition(transform.position);
        SaveInitialState();  // Save the initial state
    }

    private void Update()
    {
        // Skip game logic in editor scene
        if (isInEditorScene) return;

        moveCooldown -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Z))
        {
            UndoMove();
            return;
        }

        if (moveCooldown <= 0)
        {
            Vector2Int inputDirection = Vector2Int.zero;

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                SetSprite(spriteUp);
                inputDirection = Vector2Int.up;
            }
            else if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                SetSprite(spriteDown);
                inputDirection = Vector2Int.down;
            }
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                SetSprite(spriteLeft);
                inputDirection = Vector2Int.left;
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                SetSprite(spriteRight);
                inputDirection = Vector2Int.right;
            }

            if (inputDirection != Vector2Int.zero)
            {
                SaveState();  // Save the state before moving
                TryMove(inputDirection);
                moveCooldown = moveDelay;
            }
        }
    }

    private void SetSprite(Sprite directionalSprite)
    {
        spriteRenderer.sprite = directionalSprite;
    }

    private void TryMove(Vector2Int direction)
    {
        Vector2Int targetPosition = gridPosition + direction;
        GridCell targetCell = gridSystem.GetGridCell(targetPosition);

        if(targetCell != null && !targetCell.hasWall)
        {
            if (!targetCell.IsOccupied())
            {
                gridSystem.GetGridCell(gridPosition).ClearOccupant();
                gridPosition = targetPosition;
                transform.position = gridSystem.GetWorldPosition(gridPosition);
                gridSystem.GetGridCell(gridPosition).SetOccupant(gameObject);
            }
            else if (targetCell.occupant != null && targetCell.occupant.CompareTag("Box"))
            {
                BoxController box = targetCell.occupant.GetComponent<BoxController>();
                if (box != null && box.TryMove(direction))
                {
                    gridSystem.GetGridCell(gridPosition).ClearOccupant();
                    gridPosition = targetPosition;
                    transform.position = gridSystem.GetWorldPosition(gridPosition);
                    gridSystem.GetGridCell(gridPosition).SetOccupant(gameObject);
                }
            }
        }
    }

    // Save the very first state of the game
    private void SaveInitialState()
    {
        // Safety check: skip if in editor scene or lists are null/empty
        if (isInEditorScene || boxes == null || keyTiles == null)
            return;

        List<Vector2Int> boxPositions = new List<Vector2Int>();
        foreach (BoxController box in boxes)
        {
            if (box != null)
            {
                boxPositions.Add(box.GetGridPosition());
            }
        }

        List<bool> keyTileStates = new List<bool>();
        foreach (KeyTile keyTile in keyTiles)
        {
            if (keyTile != null)
            {
                keyTileStates.Add(keyTile.isActivated);
            }
        }

        GameState initialState = new GameState(gridPosition, boxPositions, keyTileStates);
        gameStateHistory.Push(initialState);
    }

    // Save the state before each move is made
    private void SaveState()
    {
        // Safety check: skip if in editor scene or lists are null/empty
        if (isInEditorScene || boxes == null || keyTiles == null)
            return;

        List<Vector2Int> boxPositions = new List<Vector2Int>();
        foreach (BoxController box in boxes)
        {
            if (box != null)
            {
                boxPositions.Add(box.GetGridPosition());
            }
        }

        List<bool> keyTileStates = new List<bool>();
        foreach (KeyTile keyTile in keyTiles)
        {
            if (keyTile != null)
            {
                keyTileStates.Add(keyTile.isActivated);
            }
        }

        GameState currentState = new GameState(gridPosition, boxPositions, keyTileStates);
        gameStateHistory.Push(currentState);
    }

    private void UndoMove()
    {
        // Skip undo logic in editor scene
        if (isInEditorScene || boxes == null || keyTiles == null)
            return;

        if (gameStateHistory.Count > 1)
        {
            // Get the previous state without popping it immediately
            GameState prevState = gameStateHistory.Pop();

            // Restore player position
            gridSystem.GetGridCell(gridPosition).ClearOccupant();
            gridPosition = prevState.playerPosition;
            transform.position = gridSystem.GetWorldPosition(gridPosition);
            gridSystem.GetGridCell(gridPosition).SetOccupant(gameObject);

            // Restore box positions
            for (int i = 0; i < boxes.Count && i < prevState.boxPositions.Count; i++)
            {
                if (boxes[i] != null)
                {
                    boxes[i].SetPosition(prevState.boxPositions[i]);
                }
            }

            // Restore key tile states
            for (int i = 0; i < keyTiles.Count && i < prevState.keyTileStates.Count; i++)
            {
                if (keyTiles[i] != null)
                {
                    keyTiles[i].isActivated = prevState.keyTileStates[i];
                }
            }
        }
    }
}
