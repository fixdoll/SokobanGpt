using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GoalTile : MonoBehaviour
{
    public GridSystem gridSystem;
    public List<KeyTile> keyTiles;
    public Sprite closedSprite;
    public Sprite openSprite;
    private SpriteRenderer spriteRenderer;
    private bool isOpen;
    private bool finished;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        // Find all KeyTiles in the scene and add them to the list
        keyTiles = new List<KeyTile>(FindObjectsByType<KeyTile>(FindObjectsSortMode.None));
        keyTiles = keyTiles.Where(x => x.gameObject.scene == gameObject.scene).ToList();
        UpdateGoalState();
    }

    private void Update()
    {
        UpdateGoalState();

        if (isOpen)
        {
            Vector2Int gridPosition = gridSystem.GetGridPosition(transform.position);
            GridCell cell = gridSystem.GetGridCell(gridPosition);

            if (cell.IsOccupied() && cell.occupant.CompareTag("Player"))
            {
                if (!finished)
                {
                    finished = true;
                    FlowController.Instance.LevelComplete();
                }
            }
        }
    }

    private void UpdateGoalState()
    {
        isOpen = true;
        foreach (KeyTile keyTile in keyTiles)
        {
            if (!keyTile.isActivated)
            {
                isOpen = false;
                break;
            }
        }

        spriteRenderer.sprite = isOpen ? openSprite : closedSprite;
    }
}
