using UnityEngine;

[CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyData")]
public class EnemyData : EntityData
{
    [Header("Enemy Settings")]
    [Space(10)]

    [SerializeField]
    [Tooltip("The distance at which the enemy will start following the player.")]
    public float m_DetectionDistance = 7.5f;

    [SerializeField]
    [Tooltip("The distance at which the enemy will stop following the player.")]
    public float m_AttentionDistance = 15f;

    [SerializeField]
    [Tooltip("The distance at which the enemy will wander around when the player is not in range.")]
    [Min(0)]
    public int m_WanderDistance = 3;
}
