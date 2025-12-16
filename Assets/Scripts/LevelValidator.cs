using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LevelValidator
{
    public static bool IsLevelSolvable(LevelData levelData)
    {
        if (levelData == null || !levelData.IsValid())
            return false;

        SokobanState initialState = new SokobanState(levelData);

        // Let the Sokoban search below decide solvability:
        // we want a configuration where all keys are covered by boxes
        // AND the player can walk to the goal with walls/boxes as obstacles.
        if (!CanAllKeysBeCoveredAndGoalReachable(initialState))
        {
            Debug.Log($"Level {levelData.levelName}: Not all keys can be covered with the player still able to reach the goal");
            return false;
        }

        Debug.Log($"Level {levelData.levelName}: SOLVABLE");
        return true;
    }

    /// <summary>
    /// Search for any reachable configuration where:
    ///  - every key tile has a box on it, and
    ///  - the player can walk to the goal, given current walls and boxes.
    /// </summary>
    private static bool CanAllKeysBeCoveredAndGoalReachable(SokobanState state)
    {
        if (state.boxPositions.Count < state.keyPositions.Count)
        {
            Debug.Log($"Insufficient boxes: {state.boxPositions.Count} boxes for {state.keyPositions.Count} keys");
            return false;
        }

        return ExploreBoxConfigurations(state);
    }

    private static bool ExploreBoxConfigurations(SokobanState initialState)
    {
        HashSet<string> visitedConfigurations = new HashSet<string>();
        Queue<SokobanState> stateQueue = new Queue<SokobanState>();

        stateQueue.Enqueue(initialState);
        visitedConfigurations.Add(StateHash(initialState));

        int exploredStates = 0;
        // Slightly higher cap to reduce false "unsolvable" from early cut-off.
        const int MAX_STATES = 50000;

        while (stateQueue.Count > 0 && exploredStates < MAX_STATES)
        {
            SokobanState currentState = stateQueue.Dequeue();
            exploredStates++;

            if (AreAllKeysCovered(currentState) && IsPlayerGoalReachableWithObstacles(currentState))
            {
                Debug.Log($"Found solution after exploring {exploredStates} states");
                return true;
            }

            var nextStates = GenerateNextStates(currentState);
            foreach (var nextState in nextStates)
            {
                string hash = StateHash(nextState);
                if (!visitedConfigurations.Contains(hash) && !IsDeadlocked(nextState))
                {
                    visitedConfigurations.Add(hash);
                    stateQueue.Enqueue(nextState);
                }
            }
        }

        return false;
    }

    private static List<SokobanState> GenerateNextStates(SokobanState currentState)
    {
        List<SokobanState> nextStates = new List<SokobanState>();

        HashSet<Vector2Int> reachable = ComputePlayerReachable(currentState);

        for (int boxIdx = 0; boxIdx < currentState.boxPositions.Count; boxIdx++)
        {
            Vector2Int boxPos = currentState.boxPositions[boxIdx];
            Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

            foreach (var pushDir in directions)
            {
                Vector2Int newBoxPos = boxPos + pushDir;
                Vector2Int playerPos = boxPos - pushDir;

                if (IsPushValid(currentState, boxIdx, newBoxPos, playerPos, reachable))
                {
                    SokobanState newState = CloneState(currentState);
                    newState.boxPositions[boxIdx] = newBoxPos;
                    newState.playerPosition = boxPos;
                    nextStates.Add(newState);
                }
            }
        }

        return nextStates;
    }

    private static bool IsPushValid(SokobanState state, int boxIdx, Vector2Int newBoxPos, Vector2Int playerNeededPos, HashSet<Vector2Int> reachable)
    {
        if (!state.IsValidPosition(newBoxPos) || state.IsWall(newBoxPos)) return false;

        for (int i = 0; i < state.boxPositions.Count; i++)
            if (i != boxIdx && state.boxPositions[i] == newBoxPos) return false;

        return reachable.Contains(playerNeededPos);
    }

    private static HashSet<Vector2Int> ComputePlayerReachable(SokobanState state)
    {
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> obstacles = new HashSet<Vector2Int>(state.walls);
        foreach (var box in state.boxPositions)
            obstacles.Add(box);

        queue.Enqueue(state.playerPosition);
        visited.Add(state.playerPosition);

        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();
            foreach (var dir in directions)
            {
                Vector2Int next = current + dir;
                if (state.IsValidPosition(next) && !obstacles.Contains(next) && !visited.Contains(next))
                {
                    visited.Add(next);
                    queue.Enqueue(next);
                }
            }
        }

        return visited;
    }

    /// <summary>
    /// BFS reachability that treats both walls and boxes as blocking tiles.
    /// Used to verify that, in a candidate solution state, the player can
    /// actually walk from their position to the goal.
    /// </summary>
    private static bool IsPlayerGoalReachableWithObstacles(SokobanState state)
    {
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        HashSet<Vector2Int> obstacles = new HashSet<Vector2Int>(state.walls);
        foreach (var box in state.boxPositions)
        {
            obstacles.Add(box);
        }

        queue.Enqueue(state.playerPosition);
        visited.Add(state.playerPosition);

        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            if (current == state.goalPosition)
                return true;

            foreach (var dir in directions)
            {
                Vector2Int next = current + dir;
                if (state.IsValidPosition(next) && !obstacles.Contains(next) && !visited.Contains(next))
                {
                    visited.Add(next);
                    queue.Enqueue(next);
                }
            }
        }

        return false;
    }

    private static bool IsDeadlocked(SokobanState state)
    {
        foreach (var box in state.boxPositions)
        {
            if (!state.keyPositions.Contains(box))
            {
                bool wallX = state.IsWall(box + Vector2Int.left) || state.IsWall(box + Vector2Int.right);
                bool wallY = state.IsWall(box + Vector2Int.up) || state.IsWall(box + Vector2Int.down);
                if (wallX && wallY) return true;
            }
        }
        return false;
    }

    private static bool AreAllKeysCovered(SokobanState state)
    {
        HashSet<Vector2Int> boxes = new HashSet<Vector2Int>(state.boxPositions);
        return state.keyPositions.All(pos => boxes.Contains(pos));
    }

    private static string StateHash(SokobanState state)
    {
        var sortedBoxes = state.boxPositions.OrderBy(p => p.x).ThenBy(p => p.y);
        return $"{state.playerPosition.x},{state.playerPosition.y};" +
               string.Join(";", sortedBoxes.Select(p => $"{p.x},{p.y}"));
    }

    private static SokobanState CloneState(SokobanState original)
    {
        return new SokobanState
        {
            playerPosition = original.playerPosition,
            goalPosition = original.goalPosition,
            boxPositions = new List<Vector2Int>(original.boxPositions),
            keyPositions = new List<Vector2Int>(original.keyPositions),
            walls = original.walls,
            width = original.width,
            height = original.height
        };
    }
}

// ================================
// SOKOBAN STATE REPRESENTATION
// ================================
public class SokobanState
{
    public Vector2Int playerPosition;
    public Vector2Int goalPosition;
    public List<Vector2Int> boxPositions;
    public List<Vector2Int> keyPositions;
    public HashSet<Vector2Int> walls;
    public int width, height;

    public SokobanState(LevelData levelData)
    {
        playerPosition = levelData.playerSpawn;
        goalPosition = levelData.playerGoal;
        boxPositions = new List<Vector2Int>(levelData.boxCoordinates);
        keyPositions = new List<Vector2Int>(levelData.keyCoordinates);
        walls = new HashSet<Vector2Int>(levelData.wallCoordinates);
        width = levelData.width;
        height = levelData.height;
    }

    // Empty constructor for cloning
    public SokobanState() { }

    public bool IsValidPosition(Vector2Int pos)
    {
        return pos.x >= 0 && pos.x < width && pos.y >= 0 && pos.y < height;
    }

    public bool IsWall(Vector2Int pos)
    {
        return walls.Contains(pos);
    }
}