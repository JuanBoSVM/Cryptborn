using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

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

    public PathNode GoBackToPreviousNode()
    {
        --m_CurrentNodeIndex;
        return GetCurrentNode();
    }

    private bool CanGeneratePath(Vector2Int start, Vector2Int end)
    {
        if (walkableTiles == null)
        {
            Debug.LogError("Walkable Tilemap reference is missing.");
            return false;
        }

        if (!IsWalkable(start) || !IsWalkable(end))
        {
            Debug.LogWarning("Cannot generate a path because the start or end tile is not walkable.");
            return false;
        }

        return true;
    }

    public void GeneratePath(Vector2Int start, Vector2Int end)
    {
        if (!CanGeneratePath(start, end)) { return; }
        if (start == m_PreviousStartPosition && end == m_PreviousEndPosition) { return; }

        m_PreviousStartPosition = start;
        m_PreviousEndPosition = end;

        m_Path.Clear();

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
                return;
            }

            AddNeighborsToOpenList(m_CurrentNode, end);
        }

        Debug.LogWarning("No path could be found between the supplied tiles.");
    }

    private bool IsWalkable(Vector2Int position)
    {
        Vector3Int position3D = new Vector3Int(position.x, position.y, (int)walkableTiles.transform.position.z);

        return walkableTiles.HasTile(position3D)
            && (unwalkableTiles == null || !unwalkableTiles.HasTile(position3D));
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

    private void AddNeighborsToOpenList(PathNode currentNode, Vector2Int end)
    {
        // Add the neighboring nodes to the open list if they are walkable and not already visited
        foreach (Vector2Int direction in m_Directions)
        {
            Vector2Int neighborPosition = currentNode.position + direction;

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
        // Draw a line connecting the nodes in the current path
        for (int i = 0; i < m_Path.Count - 1; i++)
        {
            Vector3 start = new Vector3(m_Path[i].position.x + 0.5f, m_Path[i].position.y + 0.5f, 0);
            Vector3 end = new Vector3(m_Path[i + 1].position.x + 0.5f, m_Path[i + 1].position.y + 0.5f, 0);
            Debug.DrawLine(start, end, Color.green);
        }
    }
#endif
}
