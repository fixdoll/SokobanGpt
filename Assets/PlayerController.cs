using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GridSystem gridSystem;
    private Vector2Int gridPosition;
    public float moveDelay = 0.2f;
    private float moveCooldown;

    private Stack<GameState> gameStateHistory = new Stack<GameState>();
    public List<BoxController> boxes;
    public List<KeyTile> keyTiles;

    private void Start()
    {
        gridPosition = gridSystem.GetGridPosition(transform.position);
        SaveInitialState();  // Save the initial state
    }

    private void Update()
    {
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
                inputDirection = Vector2Int.up;
            else if (Input.GetKeyDown(KeyCode.DownArrow))
                inputDirection = Vector2Int.down;
            else if (Input.GetKeyDown(KeyCode.LeftArrow))
                inputDirection = Vector2Int.left;
            else if (Input.GetKeyDown(KeyCode.RightArrow))
                inputDirection = Vector2Int.right;

            if (inputDirection != Vector2Int.zero)
            {
                SaveState();  // Save the state before moving
                TryMove(inputDirection);
                moveCooldown = moveDelay;
            }
        }
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
        List<Vector2Int> boxPositions = new List<Vector2Int>();
        foreach (BoxController box in boxes)
        {
            boxPositions.Add(box.GetGridPosition());
        }

        List<bool> keyTileStates = new List<bool>();
        foreach (KeyTile keyTile in keyTiles)
        {
            keyTileStates.Add(keyTile.isActivated);
        }

        GameState initialState = new GameState(gridPosition, boxPositions, keyTileStates);
        gameStateHistory.Push(initialState);
    }

    // Save the state before each move is made
    private void SaveState()
    {
        List<Vector2Int> boxPositions = new List<Vector2Int>();
        foreach (BoxController box in boxes)
        {
            boxPositions.Add(box.GetGridPosition());
        }

        List<bool> keyTileStates = new List<bool>();
        foreach (KeyTile keyTile in keyTiles)
        {
            keyTileStates.Add(keyTile.isActivated);
        }

        GameState currentState = new GameState(gridPosition, boxPositions, keyTileStates);
        gameStateHistory.Push(currentState);
    }

    private void UndoMove()
    {
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
            for (int i = 0; i < boxes.Count; i++)
            {
                boxes[i].SetPosition(prevState.boxPositions[i]);
            }

            // Restore key tile states
            for (int i = 0; i < keyTiles.Count; i++)
            {
                keyTiles[i].isActivated = prevState.keyTileStates[i];
            }
        }
    }
}
