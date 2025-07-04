using UnityEngine;

public class KeyTile : MonoBehaviour
{
    public GridSystem gridSystem;
    public bool isActivated;
    GridCell cell;

    private void Start()
    {
        Vector2Int gridPosition = gridSystem.GetGridPosition(transform.position);
        cell = gridSystem.GetGridCell(gridPosition);
    }

    private void Update()
    {

        if (cell.IsOccupied() && cell.occupant.CompareTag("Box"))
        {
            isActivated = true;
        }
        else
        {
            isActivated = false;
        }
    }

    public void SetActivation(bool activation)
    {
        isActivated = activation;
    }
}
