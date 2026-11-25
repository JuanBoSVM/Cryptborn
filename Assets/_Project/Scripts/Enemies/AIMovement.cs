using System.Collections;
using UnityEngine;
using UnityEngine.Tilemaps;

public enum AIMovementMode
{
    FollowPlayer,
    Wander
}

public enum AIMovementState
{
    Wandering,
    Following,
    Attacking
}

public class AIMovement : EntityMovement
{
    private const int WanderTargetAttempts = 100;

#if UNITY_EDITOR

    private const int DebugCircleSegments = 48;

#endif

    [SerializeField]
    private AIMovementMode m_Mode = AIMovementMode.FollowPlayer;

    [SerializeField, Min(0.16f)]
    [Tooltip("The interval in seconds at which the path is updated.")]
    private float m_UpdatePathInterval = 0.5f;

    [SerializeField]
    private AIMovementState m_State = AIMovementState.Wandering;

    private TilemapPathfinding m_Pathfinding = new TilemapPathfinding();
    private Transform m_PlayerTransform;
    private EnemyData m_EnemyData;
    private Vector2Int m_LastKnownPlayerCoordinates;
    private Vector2 m_PreviousFollowPosition;
    private bool m_HasPreviousFollowPosition;
    private bool m_HasRetriedPathWhileStuck;
    private bool m_UseCounterClockwisePath;
    private Coroutine m_PathUpdateCoroutine;
    private Coroutine m_WanderWaitCoroutine;
    private TilemapPathfinding.PathNode m_FollowTargetNode;
    private bool m_HasWanderTarget;
    private bool m_WarnedNoWanderTarget;

    private static Vector2Int GetCoordinates(Vector2 position)
    {
        return new Vector2Int(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y));
    }

    private Vector2 GetNodeOffset()
    {
        TilemapPathfinding.PathNode node = m_Pathfinding.GetCurrentNode();
        return node == null ? Vector2.zero : (Vector2)node.position + Vector2.one * 0.5f - (Vector2)transform.position;
    }

    private bool HasReachedNode()
    {
        Vector2 nodeOffset = GetNodeOffset();
        const float nodeReachDistance = 0.15f;
        return nodeOffset.sqrMagnitude <= nodeReachDistance * nodeReachDistance
            || (MovementDirection.sqrMagnitude > 0.01f && Vector2.Dot(MovementDirection, nodeOffset) < 0f);
    }

    private void SetState(AIMovementState state)
    {
        if (m_State == state) { return; }

        m_State = state;
        m_HasWanderTarget = false;
        m_FollowTargetNode = null;
        m_HasPreviousFollowPosition = false;
        if (state == AIMovementState.Following)
        {
            m_PreviousFollowPosition = transform.position;
            m_HasRetriedPathWhileStuck = false;
            m_UseCounterClockwisePath = false;
        }
        if (state == AIMovementState.Attacking) { MovementDirection = Vector2.zero; }

        if (state != AIMovementState.Wandering && m_WanderWaitCoroutine != null)
        {
            StopCoroutine(m_WanderWaitCoroutine);
            m_WanderWaitCoroutine = null;
        }

        if (state == AIMovementState.Following && m_PlayerTransform != null)
        {
            Vector2Int current = GetCoordinates(transform.position);
            Vector2Int target = GetCoordinates(m_PlayerTransform.position);
            if (target != current) { m_Pathfinding.GeneratePath(current, target, true, GetFollowPathOrder()); }
        }
    }

    public bool IsAttacking()
    {
        return m_State == AIMovementState.Attacking;
    }

    private PathSearchOrder GetFollowPathOrder()
    {
        return m_UseCounterClockwisePath ? PathSearchOrder.CounterClockwise : PathSearchOrder.Clockwise;
    }

    private void UpdateState()
    {
        if (m_Mode == AIMovementMode.Wander || m_PlayerTransform == null)
        {
            SetState(AIMovementState.Wandering);
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, m_PlayerTransform.position);
        float attackRadius = Mathf.Max(0f, m_EnemyData.m_AttackRangeRadius);
        if (m_State == AIMovementState.Attacking && distanceToPlayer <= attackRadius + 0.75f) { return; }

        bool wasFollowing = m_State != AIMovementState.Wandering;
        float range = wasFollowing ? m_EnemyData.m_AttentionDistance : m_EnemyData.m_DetectionDistance;

        if (distanceToPlayer > range)
        {
            SetState(AIMovementState.Wandering);
            return;
        }

        m_LastKnownPlayerCoordinates = GetCoordinates(m_PlayerTransform.position);
        SetState(distanceToPlayer <= attackRadius ? AIMovementState.Attacking : AIMovementState.Following);
    }

    private Vector2 GetFollowingDirection()
    {
        if (GetCoordinates(transform.position) == GetCoordinates(m_PlayerTransform.position))
        {
            m_FollowTargetNode = null;
            return ((Vector2)m_PlayerTransform.position - (Vector2)transform.position).normalized;
        }

        if (m_FollowTargetNode == null)
        {
            m_FollowTargetNode = m_Pathfinding.GetFurthestVisibleNode(GetCoordinates(transform.position));
        }

        if (m_FollowTargetNode == null) { return Vector2.zero; }

        Vector2 target = (Vector2)m_FollowTargetNode.position + Vector2.one * 0.5f;
        Vector2 offset = target - (Vector2)transform.position;
        const float reachDistance = 0.15f;
        bool hasReachedTarget = offset.sqrMagnitude <= reachDistance * reachDistance
            || (MovementDirection.sqrMagnitude > 0.01f && Vector2.Dot(MovementDirection, offset) < 0f);

        if (hasReachedTarget)
        {
            m_Pathfinding.AdvancePastNode(m_FollowTargetNode);
            m_FollowTargetNode = null;
            return Vector2.zero;
        }

        return offset.normalized;
    }

    private IEnumerator WaitBeforeWandering()
    {
        yield return new WaitForSeconds(Mathf.Max(0f, m_EnemyData.m_WanderWaitDuration));
        m_WanderWaitCoroutine = null;
    }

    private void StartWanderPause()
    {
        m_HasWanderTarget = false;
        if (m_WanderWaitCoroutine == null)
        {
            m_WanderWaitCoroutine = StartCoroutine(WaitBeforeWandering());
        }
    }

    private bool TryChooseWanderTarget()
    {
        Vector2Int anchor = m_LastKnownPlayerCoordinates;
        int distance = Mathf.Max(0, m_EnemyData.m_WanderDistance);

        if (distance == 0)
        {
            StartWanderPause();
            return true;
        }

        for (int attempt = 0; attempt < WanderTargetAttempts; attempt++)
        {
            Vector2Int candidate = anchor + new Vector2Int(
                Random.Range(-distance, distance + 1),
                Random.Range(-distance, distance + 1));

            if (candidate == GetCoordinates(transform.position)
                || Vector2Int.Distance(candidate, anchor) > distance
                || !m_Pathfinding.IsWalkable(candidate))
            {
                continue;
            }

            if (m_Pathfinding.GeneratePath(GetCoordinates(transform.position), candidate, false))
            {
                m_HasWanderTarget = true;
                m_WarnedNoWanderTarget = false;
                return true;
            }
        }

        return false;
    }

    private void UpdatePath()
    {
        Vector2Int currentCoordinates = GetCoordinates(transform.position);

        if (m_State == AIMovementState.Following)
        {
            Vector2Int target = GetCoordinates(m_PlayerTransform.position);
            if (target != currentCoordinates)
            {
                m_Pathfinding.GeneratePath(currentCoordinates, target, true, GetFollowPathOrder());
                m_FollowTargetNode = null;
            }
            return;
        }

        if (m_State == AIMovementState.Attacking) { return; }

        if (m_State != AIMovementState.Wandering || m_HasWanderTarget || m_WanderWaitCoroutine != null)
        {
            return;
        }

        if (!TryChooseWanderTarget() && !m_WarnedNoWanderTarget)
        {
            Debug.LogWarning($"{name} could not find a reachable wander target.", this);
            m_WarnedNoWanderTarget = true;
        }
    }

    private IEnumerator UpdatePathCoroutine()
    {
        while (enabled)
        {
            UpdatePath();
            yield return new WaitForSeconds(m_UpdatePathInterval);
        }
    }

    private void StartPathUpdates()
    {
        if (m_PathUpdateCoroutine == null)
        {
            m_PathUpdateCoroutine = StartCoroutine(UpdatePathCoroutine());
        }
    }

    private void AdvancePath()
    {
        if (!HasReachedNode()) { return; }

        TilemapPathfinding.PathNode node = m_Pathfinding.GetCurrentNode();
        if (node == null) { return; }

        Vector2 nodePosition = (Vector2)node.position + Vector2.one * 0.5f;
        if (m_Pathfinding.NodeIndex > 0 && Vector2.Distance(transform.position, nodePosition) <= 0.15f)
        {
            transform.position = new Vector3(nodePosition.x, nodePosition.y, transform.position.z);
        }

        m_Pathfinding.AdvanceToNextNode();

        if (m_State == AIMovementState.Wandering && m_HasWanderTarget && m_Pathfinding.IsAtEndOfPath)
        {
            StartWanderPause();
        }
    }

    private void GetReferences()
    {
        m_Pathfinding.walkableTiles = GameObject.FindGameObjectWithTag("Ground")?.GetComponent<Tilemap>();
        m_PlayerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        m_EnemyData = m_EntityData as EnemyData;
    }

    private bool ValidateReferences()
    {
        if (m_Pathfinding.walkableTiles == null)
        {
            Debug.LogError("Ground Tilemap reference is missing.");
            GameManager.ExitGame();
            return false;
        }

        if (m_EnemyData == null)
        {
            Debug.LogError("EnemyData is not assigned in the inspector.", this);
            return false;
        }

        if (m_Mode == AIMovementMode.FollowPlayer && m_PlayerTransform == null)
        {
            Debug.LogError("Player Transform reference is missing.");
            GameManager.ExitGame();
            return false;
        }

        return true;
    }

    private void Start()
    {
        GetReferences();
        if (!ValidateReferences()) { enabled = false; return; }

        m_LastKnownPlayerCoordinates = GetCoordinates(transform.position);
        StartPathUpdates();
    }

    private void OnEnable()
    {
        if (m_EnemyData != null) { StartPathUpdates(); }
    }

    private void OnDisable()
    {
        if (m_PathUpdateCoroutine != null)
        {
            StopCoroutine(m_PathUpdateCoroutine);
            m_PathUpdateCoroutine = null;
        }

        if (m_WanderWaitCoroutine != null)
        {
            StopCoroutine(m_WanderWaitCoroutine);
            m_WanderWaitCoroutine = null;
        }
    }

    protected override void MoveTowards(Vector2 direction)
    {
        float speed = MaxSpeed * (m_State == AIMovementState.Wandering ? Mathf.Max(0f, m_EnemyData.m_WanderSpeedMultiplier) : 1f);
        MovementDirection = Vector2.MoveTowards(
            MovementDirection,
            direction * speed,
            (direction == Vector2.zero ? Deceleration : Acceleration) * Time.fixedDeltaTime);
    }

    private void UpdateFollowPathIfStuck()
    {
        if (m_State != AIMovementState.Following)
        {
            m_HasPreviousFollowPosition = false;
            return;
        }

        Vector2 currentPosition = transform.position;
        const float minimumMovementSqrMagnitude = 0.0001f;
        bool hasMoved = (currentPosition - m_PreviousFollowPosition).sqrMagnitude > minimumMovementSqrMagnitude;
        if (hasMoved)
        {
            m_HasRetriedPathWhileStuck = false;
        }
        else if (m_HasPreviousFollowPosition && !m_HasRetriedPathWhileStuck)
        {
            m_UseCounterClockwisePath = !m_UseCounterClockwisePath;
            Vector2Int current = GetCoordinates(currentPosition);
            Vector2Int target = GetCoordinates(m_PlayerTransform.position);
            m_Pathfinding.GeneratePath(current, target, true, GetFollowPathOrder());
            m_FollowTargetNode = null;
            m_HasRetriedPathWhileStuck = true;
        }

        m_PreviousFollowPosition = currentPosition;
        m_HasPreviousFollowPosition = true;
    }

    protected override void FixedUpdate()
    {
        UpdateState();
        UpdateFollowPathIfStuck();

        if (m_State == AIMovementState.Following)
        {
            m_TargetDirection = GetFollowingDirection();
        }
        else if (m_State == AIMovementState.Attacking)
        {
            m_FollowTargetNode = null;
            m_TargetDirection = Vector2.zero;
            MovementDirection = Vector2.zero;
        }
        else
        {
            if (m_HasWanderTarget) { AdvancePath(); }
            m_TargetDirection = m_HasWanderTarget ? GetNodeOffset().normalized : Vector2.zero;
        }

#if UNITY_EDITOR
        m_Pathfinding.DrawPath();
        DrawDebugCircle(m_EnemyData.m_DetectionDistance, Color.red);
        DrawDebugCircle(m_EnemyData.m_AttentionDistance, Color.green);
        DrawDebugCircle(m_EnemyData.m_AttackRangeRadius, Color.yellow);
#endif

        if (m_EnemyData != null && m_State != AIMovementState.Attacking)
        {
            Vector2 wallAvoidance = m_Pathfinding.GetWallAvoidance(transform.position, 1.5f);
            if (wallAvoidance.sqrMagnitude > 0.01f)
            {
                float avoidanceStrength = Mathf.Clamp01(wallAvoidance.magnitude * 2f);
                Vector2 avoidanceDirection = wallAvoidance.normalized;
                m_TargetDirection = Vector2.Lerp(m_TargetDirection, avoidanceDirection, avoidanceStrength).normalized;
            }
        }

        base.FixedUpdate();
    }

#if UNITY_EDITOR
    private void DrawDebugCircle(float radius, Color color)
    {
        Vector3 center = transform.position;
        float angleStep = 2f * Mathf.PI / DebugCircleSegments;
        Vector3 previousPoint = center + new Vector3(radius, 0f, 0f);

        for (int i = 1; i <= DebugCircleSegments; i++)
        {
            float angle = angleStep * i;
            Vector3 nextPoint = center + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0f);
            Debug.DrawLine(previousPoint, nextPoint, color, Time.fixedDeltaTime);
            previousPoint = nextPoint;
        }
    }
#endif
}

