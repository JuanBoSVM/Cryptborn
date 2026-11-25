using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public enum PathSearchOrder
{
    Clockwise,
    CounterClockwise
}

public class TilemapPathfinding
{
    private static readonly List<Vector2Int> m_Directions = new List<Vector2Int>
    {
        Vector2Int.up,
        Vector2Int.right,
        Vector2Int.down,
        Vector2Int.left
    };

    public Tilemap walkableTiles;
    public Tilemap unwalkableTiles;

    public bool HasPath => m_Path.Count > 0;

    private List<PathNode> m_OpenNodes = new List<PathNode>();
    private List<PathNode> m_ClosedNodes = new List<PathNode>();
    private List<PathNode> m_Path = new List<PathNode>();

    private Vector2Int m_PreviousStartPosition = Vector2Int.zero;
    private Vector2Int m_PreviousEndPosition = Vector2Int.zero;
    private PathSearchOrder m_PreviousSearchOrder = PathSearchOrder.Clockwise;

    private int m_CurrentNodeIndex = 0;
    private PathNode m_CurrentNode;

    public class PathNode
    {
        public Vector2Int position;
        public int startCost;
        public int endCost;
        public PathNode previous;

        public int TotalCost => startCost + endCost;
    }

    public int NodeIndex => m_CurrentNodeIndex;

    public bool IsAtEndOfPath => m_CurrentNodeIndex >= m_Path.Count;

    public PathNode GetCurrentNode()
    {
        if (m_Path.Count == 0 || m_CurrentNodeIndex >= m_Path.Count)
        {
            return null;
        }

        return m_Path[m_CurrentNodeIndex];
    }

    public PathNode AdvanceToNextNode()
    {
        ++m_CurrentNodeIndex;
        return GetCurrentNode();
    }

    public PathNode GetFurthestVisibleNode(Vector2Int from)
    {
        if (IsAtEndOfPath) { return null; }

        int furthestVisibleIndex = m_CurrentNodeIndex;
        for (int i = m_CurrentNodeIndex + 1; i < m_Path.Count; i++)
        {
            if (HasLineOfSight(from, m_Path[i].position))
            {
                furthestVisibleIndex = i;
            }
        }

        return m_Path[furthestVisibleIndex];
    }

    public PathNode AdvancePastNode(PathNode node)
    {
        int nodeIndex = m_Path.IndexOf(node);
        if (nodeIndex >= m_CurrentNodeIndex)
        {
            m_CurrentNodeIndex = nodeIndex + 1;
        }

        return GetCurrentNode();
    }

    public PathNode GoBackToPreviousNode()
    {
        --m_CurrentNodeIndex;
        return GetCurrentNode();
    }

    private bool CanGeneratePath(Vector2Int start, Vector2Int end, bool logWarnings)
    {
        if (walkableTiles == null)
        {
            Debug.LogError("Walkable Tilemap reference is missing.");
            return false;
        }

        if (!IsWalkable(start) || !IsWalkable(end))
        {
            if (logWarnings)
            {
                Debug.LogWarning("Cannot generate a path because the start or end tile is not walkable.");
            }
            return false;
        }

        return true;
    }

    public bool GeneratePath(
        Vector2Int start,
        Vector2Int end,
        bool logWarnings = true,
        PathSearchOrder searchOrder = PathSearchOrder.Clockwise)
    {
        if (!CanGeneratePath(start, end, logWarnings))
        {
            m_Path.Clear();
            m_CurrentNodeIndex = 0;
            return false;
        }

        if (start == m_PreviousStartPosition
            && end == m_PreviousEndPosition
            && searchOrder == m_PreviousSearchOrder
            && m_Path.Count > 0)
        {
            return true;
        }

        m_PreviousStartPosition = start;
        m_PreviousEndPosition = end;
        m_PreviousSearchOrder = searchOrder;
        m_Path.Clear();
        m_CurrentNodeIndex = 0;

        m_OpenNodes.Clear();
        m_ClosedNodes.Clear();

        PathNode startNode = new PathNode
        {
            position = start,
            startCost = 0,
            endCost = GetHeuristic(start, end),
            previous = null
        };

        m_OpenNodes.Add(startNode);

        // Loop until we either find the end node or exhaust all possible nodes
        while (m_OpenNodes.Count > 0 && m_OpenNodes.Count < walkableTiles.size.x * walkableTiles.size.y)
        {
            // Look into the open nodes and find the lowest cost node to explore next
            m_CurrentNode = GetLowestCostNode();
            m_OpenNodes.Remove(m_CurrentNode);
            m_ClosedNodes.Add(m_CurrentNode);

            // Check if we've reached the end node
            if (m_CurrentNode.position == end)
            {
                SaveCurrentPath();
                return true;
            }

            AddNeighborsToOpenList(m_CurrentNode, end, searchOrder);
        }

        if (logWarnings) { Debug.LogWarning("No path could be found between the supplied tiles."); }
        return false;
    }

    public bool IsWalkable(Vector2Int position)
    {
        Vector3Int position3D = new Vector3Int(position.x, position.y, (int)walkableTiles.transform.position.z);

        return walkableTiles.HasTile(position3D)
            && (unwalkableTiles == null || !unwalkableTiles.HasTile(position3D));
    }

    public Vector2 GetWallAvoidance(Vector2 position, float radius)
    {
        if (walkableTiles == null || radius <= 0f) { return Vector2.zero; }

        Vector2Int currentCell = new Vector2Int(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y));
        Vector2[] directions =
        {
            Vector2.up,
            new Vector2(1f, 1f).normalized,
            Vector2.right,
            new Vector2(1f, -1f).normalized,
            Vector2.down,
            new Vector2(-1f, -1f).normalized,
            Vector2.left,
            new Vector2(-1f, 1f).normalized
        };
        const float sampleSpacing = 0.2f;
        Vector2 avoidance = Vector2.zero;

        foreach (Vector2 direction in directions)
        {
            for (float distance = sampleSpacing; distance <= radius; distance += sampleSpacing)
            {
                Vector2 samplePosition = position + direction * distance;
                Vector2Int sampleCell = new Vector2Int(Mathf.FloorToInt(samplePosition.x), Mathf.FloorToInt(samplePosition.y));
                if (sampleCell == currentCell || IsWalkable(sampleCell)) { continue; }

                float proximity = 1f - distance / radius;
                avoidance -= direction * proximity * proximity;
                break;
            }
        }

        return avoidance;
    }

    private bool HasLineOfSight(Vector2Int start, Vector2Int end)
    {
        int x = start.x;
        int y = start.y;
        int deltaX = Mathf.Abs(end.x - start.x);
        int stepX = start.x < end.x ? 1 : -1;
        int deltaY = -Mathf.Abs(end.y - start.y);
        int stepY = start.y < end.y ? 1 : -1;
        int error = deltaX + deltaY;

        while (true)
        {
            if (!IsWalkable(new Vector2Int(x, y))) { return false; }
            if (x == end.x && y == end.y) { return true; }

            int doubledError = 2 * error;
            bool moveX = x != end.x && doubledError >= deltaY;
            bool moveY = y != end.y && doubledError <= deltaX;

            if (moveX && moveY
                && (!IsWalkable(new Vector2Int(x + stepX, y)) || !IsWalkable(new Vector2Int(x, y + stepY))))
            {
                return false;
            }

            if (moveX)
            {
                error += deltaY;
                x += stepX;
            }

            if (moveY)
            {
                error += deltaX;
                y += stepY;
            }
        }
    }

    private bool AlreadyVisited(Vector2Int position)
    {
        return m_ClosedNodes.Exists(node => node.position == position);
    }

    private static int GetHeuristic(Vector2Int from, Vector2Int to)
    {
        return Mathf.Abs(from.x - to.x) + Mathf.Abs(from.y - to.y);
    }

    private PathNode GetLowestCostNode()
    {
        PathNode lowestCostNode = m_OpenNodes[0];

        foreach (PathNode node in m_OpenNodes)
        {
            // Skip nodes that have already been visited
            if (AlreadyVisited(node.position)) { continue; }

            if (node.TotalCost < lowestCostNode.TotalCost)
            {
                lowestCostNode = node;
            }
        }

        return lowestCostNode;
    }

    private void SaveCurrentPath()
    {
        m_Path.Clear();

        m_CurrentNodeIndex = 0;

        while (m_CurrentNode != null)
        {
            m_Path.Insert(0, m_CurrentNode);
            m_CurrentNode = m_CurrentNode.previous;
        }
    }

    private void AddNeighborsToOpenList(PathNode currentNode, Vector2Int end, PathSearchOrder searchOrder)
    {
        // Add the neighboring nodes to the open list if they are walkable and not already visited.
        for (int i = 0; i < m_Directions.Count; i++)
        {
            int directionIndex = searchOrder == PathSearchOrder.Clockwise
                ? i
                : (m_Directions.Count - i) % m_Directions.Count;
            Vector2Int neighborPosition = currentNode.position + m_Directions[directionIndex];

            // Skip the neighbor if it's already visited or not walkable
            if (AlreadyVisited(neighborPosition) || !IsWalkable(neighborPosition)) { continue; }

            // Create a new node for the neighbor
            PathNode neighborNode = new PathNode
            {
                position = neighborPosition,
                startCost = currentNode.startCost + 1,
                endCost = GetHeuristic(neighborPosition, end),
                previous = currentNode
            };

            // Add the neighbor node to the open list
            m_OpenNodes.Add(neighborNode);
        }
    }

#if UNITY_EDITOR
    public void DrawPath()
    {
        Color startColor = Color.blue;
        Color endColor = Color.red;

        // Draw a line connecting the nodes in the current path
        for (int i = 0; i < m_Path.Count - 1; i++)
        {
            Vector3 start = new Vector3(m_Path[i].position.x + 0.5f, m_Path[i].position.y + 0.5f, 0);
            Vector3 end = new Vector3(m_Path[i + 1].position.x + 0.5f, m_Path[i + 1].position.y + 0.5f, 0);
            Debug.DrawLine(start, end, Color.Lerp(startColor, endColor, (float)i / (m_Path.Count - 1)));
        }
    }
#endif
}
