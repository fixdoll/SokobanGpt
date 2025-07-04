using UnityEngine;

public class WallController : MonoBehaviour
{
    public GridSystem gridSystem;
    private Vector2Int gridPosition;

    private void Start()
    {
        gridPosition = gridSystem.GetGridPosition(transform.position);
        gridSystem.GetGridCell(gridPosition).SetOccupant(gameObject);
        gridSystem.GetGridCell(gridPosition).SetWall(true);
    }
}
