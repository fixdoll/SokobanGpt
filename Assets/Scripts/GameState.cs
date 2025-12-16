using System.Collections.Generic;
using UnityEngine;

public class GameState
{
    public Vector2Int playerPosition;
    public List<Vector2Int> boxPositions;
    public List<bool> keyTileStates;

    public GameState(Vector2Int playerPosition, List<Vector2Int> boxPositions, List<bool> keyTileStates)
    {
        this.playerPosition = playerPosition;
        this.boxPositions = new List<Vector2Int>(boxPositions);
        this.keyTileStates = new List<bool>(keyTileStates);
    }
}
