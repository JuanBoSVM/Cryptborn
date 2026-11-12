using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;

public class PlayerFollow : EntityMovement
{
    private Tilemap m_GroundTilemap;
    private Transform m_PlayerTransform;

    private TilemapPathfinding m_Pathfinding = new TilemapPathfinding();

    [SerializeField]
    [Min(0.16f)]
    [Tooltip("The interval in seconds at which the path to the player is updated.")]
    private float m_UpdatePathInterval = 0.5f;
    private bool ShouldUpdatePath = true;


    private Vector2Int CurrentPlayerCoordinates => new Vector2Int(Mathf.FloorToInt(m_PlayerTransform.position.x), Mathf.FloorToInt(m_PlayerTransform.position.y));
    private Vector2Int CurrentSelfCoordinates => new Vector2Int(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y));

    private Vector2 NodePosition => (m_Pathfinding.GetCurrentNode()?.position ?? (Vector2)transform.position) + Vector2.one * 0.5f;
    private Vector2 NodeOffset => NodePosition - (Vector2)transform.position;
    private Vector2 NodeDirection => NodeOffset.normalized;

    private bool NodeReached
    {
        get
        {
            const float nodeReachDistance = 0.15f;
            bool isNearNode = NodeOffset.sqrMagnitude <= nodeReachDistance * nodeReachDistance;
            bool hasPassedNode = MovementDirection.sqrMagnitude > 0.01f
                && Vector2.Dot(MovementDirection, NodeOffset) < 0f;

            return isNearNode || hasPassedNode;
        }
    }

    private bool IsNearNode
    {
        get
        {
            const float nodeReachDistance = 0.15f;
            return NodeOffset.sqrMagnitude <= nodeReachDistance * nodeReachDistance;
        }
    }

    protected override void MoveTowards(Vector2 direction)
    {
        if (NodeReached) { MovementDirection = direction.normalized * MovementDirection.magnitude; }
        else { base.MoveTowards(direction); }
    }

    private void GetReferences()
    {
        m_GroundTilemap = GameObject.FindGameObjectWithTag("Ground")?.GetComponent<Tilemap>();

        m_Pathfinding.walkableTiles = m_GroundTilemap;

        m_PlayerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void ValidateReferences()
    {
        if (m_GroundTilemap == null)
        {
            Debug.LogError("Ground Tilemap reference is missing.");
            GameManager.ExitGame();
        }

        if (m_PlayerTransform == null)
        {
            Debug.LogError("Player Transform reference is missing.");
            GameManager.ExitGame();
        }
    }

    private IEnumerator UpdatePathCoroutine()
    {
        if (!ShouldUpdatePath) { yield break; }
        ShouldUpdatePath = false;

        m_Pathfinding.GeneratePath(CurrentSelfCoordinates, CurrentPlayerCoordinates);

        yield return new WaitForSeconds(m_UpdatePathInterval);
        ShouldUpdatePath = true;
    }

    private void SnapToNodeCenter()
    {
        if (m_Pathfinding.GetCurrentNode() != null)
        {
            transform.position = new Vector3(NodePosition.x, NodePosition.y, transform.position.z);
        }
    }

    private void Start()
    {
        GetReferences();
        ValidateReferences();

        StartCoroutine(UpdatePathCoroutine());
        SnapToNodeCenter();
    }

    protected override void FixedUpdate()
    {
        if (ShouldUpdatePath) { StartCoroutine(UpdatePathCoroutine()); }

        if (NodeReached)
        {
            if (m_Pathfinding.NodeIndex > 0 && IsNearNode)
            {
                SnapToNodeCenter();
            }

            m_Pathfinding.AdvanceToNextNode();
        }

        m_TargetDirection = NodeDirection;

#if UNITY_EDITOR

        m_Pathfinding.DrawPath();

#endif

        // Movement is handled in the base class
        base.FixedUpdate();
    }
}
