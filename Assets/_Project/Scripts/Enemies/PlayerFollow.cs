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
    private bool m_IsWandering = true;

    private Vector2Int m_WanderTargetCoordinates = Vector2Int.zero;

    private int WanderDistance => ((m_EntityData as EnemyData)?.m_WanderDistance ?? 0);

    private float DetectionDistance => ((m_EntityData as EnemyData)?.m_DetectionDistance ?? 0f);
    private float AttentionDistance => ((m_EntityData as EnemyData)?.m_AttentionDistance ?? 0f);

    private Vector2Int CurrentPlayerCoordinates => new Vector2Int(Mathf.FloorToInt(m_PlayerTransform.position.x), Mathf.FloorToInt(m_PlayerTransform.position.y));
    private Vector2Int CurrentSelfCoordinates => new Vector2Int(Mathf.FloorToInt(transform.position.x), Mathf.FloorToInt(transform.position.y));

    private Vector2 NodePosition => (m_Pathfinding.GetCurrentNode()?.position ?? (Vector2)transform.position) + Vector2.one * 0.5f;
    private Vector2 NodeOffset => NodePosition - (Vector2)transform.position;
    private Vector2 NodeDirection => NodeOffset.normalized;
    private Vector2 PlayerDirection => ((m_PlayerTransform?.position ?? transform.position) - transform.position).normalized;


    private bool WanderReached => m_WanderTargetCoordinates == CurrentSelfCoordinates;

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

    private Vector2Int GetRandomWanderCoordinates()
    {
        int randomX = Random.Range(-WanderDistance, WanderDistance);
        int randomY = Random.Range(-WanderDistance, WanderDistance);

        m_WanderTargetCoordinates = CurrentSelfCoordinates + new Vector2Int(randomX, randomY);

        return m_WanderTargetCoordinates;
    }

    private bool IsPlayerInRange()
    {
        float distanceToPlayer = ((m_PlayerTransform?.position ?? transform.position) - transform.position).magnitude;

        m_IsWandering = distanceToPlayer > (m_IsWandering ? DetectionDistance : AttentionDistance);

        return !m_IsWandering;
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

        Vector2Int targetCoordinates;

        if (IsPlayerInRange())
        {
            m_IsWandering = false;
            targetCoordinates = CurrentPlayerCoordinates;
        }

        else
        {
            m_IsWandering = true;
            targetCoordinates = m_WanderTargetCoordinates;

            int attempts = 0;

            while (!m_Pathfinding.IsWalkable(targetCoordinates))
            {
                targetCoordinates = GetRandomWanderCoordinates();
                attempts++;

                if (attempts >= 100)
                {
                    Debug.LogError("Failed to find a walkable wander target.");
                    yield break;
                }
            }
        }

        m_Pathfinding.GeneratePath(CurrentSelfCoordinates, targetCoordinates);

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

            if (m_Pathfinding.IsAtEndOfPath && m_IsWandering)
            {
                GetRandomWanderCoordinates();
            }
        }

        m_TargetDirection = NodeDirection;

#if UNITY_EDITOR

        m_Pathfinding.DrawPath();
        Debug.DrawRay(transform.position, PlayerDirection * (m_IsWandering ? DetectionDistance : AttentionDistance), m_IsWandering ? Color.red : Color.green);

#endif

        // Movement is handled in the base class
        base.FixedUpdate();
    }
}
