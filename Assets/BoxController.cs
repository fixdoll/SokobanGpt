using UnityEngine;

public class BoxController : MonoBehaviour
{
    public GridSystem gridSystem;
    private Vector2Int gridPosition;

    private void Start()
    {
        gridPosition = gridSystem.GetGridPosition(transform.position);
        gridSystem.GetGridCell(gridPosition).SetOccupant(gameObject);
    }

    public bool TryMove(Vector2Int direction)
    {
        Vector2Int targetPosition = gridPosition + direction;
        GridCell targetCell = gridSystem.GetGridCell(targetPosition);

        if (targetCell != null && !targetCell.IsOccupied())
        {
            gridSystem.GetGridCell(gridPosition).ClearOccupant();
            gridPosition = targetPosition;
            transform.position = gridSystem.GetWorldPosition(gridPosition);
            gridSystem.GetGridCell(gridPosition).SetOccupant(gameObject);
            return true;
        }
        return false;
    }

    public Vector2Int GetGridPosition()
    {
        return gridPosition;
    }

    public void SetPosition(Vector2Int newPosition)
    {
        gridSystem.GetGridCell(gridPosition).ClearOccupant();
        gridPosition = newPosition;
        transform.position = gridSystem.GetWorldPosition(gridPosition);
        gridSystem.GetGridCell(gridPosition).SetOccupant(gameObject);
    }
}
