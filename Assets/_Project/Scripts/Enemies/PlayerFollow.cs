using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerFollow : EntityMovement
{
    private Tilemap m_GroundTilemap;

    private Transform m_PlayerTransform;

    private Vector2Int m_LastPlayerTile;
    private Vector2Int m_LastSelfTile;

    private Vector2Int GetTilemapCoodinateFromPosition(Vector3 position)
    {
        Vector3Int cellPosition = m_GroundTilemap.WorldToCell(position);

        return new Vector2Int(cellPosition.x, cellPosition.y);
    }

    private bool PositionHasTile(Vector2Int tilePosition)
    {
        return m_GroundTilemap.HasTile(new Vector3Int(tilePosition.x, tilePosition.y, (int)m_GroundTilemap.transform.position.z));
    }

    private bool UpdateTileLocations()
    {
        Vector2Int currentPlayerTile = GetTilemapCoodinateFromPosition(m_PlayerTransform.position);
        Vector2Int currentSelfTile = GetTilemapCoodinateFromPosition(transform.position);

        bool playerTileChanged = false;
        bool selfTileChanged = false;

        if (PositionHasTile(currentPlayerTile) && currentPlayerTile != m_LastPlayerTile)
        {
            playerTileChanged = true;
            m_LastPlayerTile = currentPlayerTile;
        }

        if (PositionHasTile(currentSelfTile) && currentSelfTile != m_LastSelfTile)
        {
            selfTileChanged = true;
            m_LastSelfTile = currentSelfTile;
        }

        return playerTileChanged || selfTileChanged;
    }

    private void GetReferences()
    {
        m_GroundTilemap = GameObject.FindGameObjectWithTag("Ground")?.GetComponent<Tilemap>();

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

    private void Start()
    {
        GetReferences();
        ValidateReferences();
    }

    protected override void FixedUpdate()
    {
        if (UpdateTileLocations())
        {
            m_TargetDirection = (m_PlayerTransform.position - transform.position).normalized;
        }

        base.FixedUpdate();
    }
}
