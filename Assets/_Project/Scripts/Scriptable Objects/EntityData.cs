using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "EntityData", menuName = "Scriptable Objects/EntityData")]
public class EntityData : ScriptableObject
{
    [Header("Visuals")]
    [Space(10)]

    [SerializeField]
    [Tooltip("The sprite used by this entity at runtime.")]
    public Sprite m_Sprite;

    [Header("Audio")]
    [Space(10)]

    [SerializeField]
    private List<AudioClip> m_Walking_SFX;

    [SerializeField]
    private List<AudioClip> m_Attacking_SFX;

    [SerializeField]
    private List<AudioClip> m_ReceivingDamage_SFX;

    [SerializeField]
    private List<AudioClip> m_Dying_SFX;

    [SerializeField]
    private List<AudioClip> m_Healing_SFX;

    [SerializeField]
    private List<AudioClip> m_Spawning_SFX;

    [Header("Movement Settings")]
    [Space(10)]

    [Min(0f)]
    [Tooltip("The max speed at which the entity can move (Units per second)")]
    public float maxSpeed = 10f;

    [Min(0f)]
    [Tooltip("The acceleration of the entity (Units per second squared)")]
    public float acceleration = 20f;

    [Min(0f)]
    [Tooltip("The deceleration of the entity (Units per second squared)")]
    public float deceleration = 40f;

    [Header("Combat Settings")]
    [Space(10)]

    [SerializeField, Min(0f)]
    [Tooltip("The radius within which this entity can attack.")]
    public float m_AttackRangeRadius = 1f;

    [SerializeField, Min(0f)]
    [Tooltip("The damage dealt by this entity per attack.")]
    public float m_AttackDamage = 1f;

    [SerializeField, Min(0f)]
    [Tooltip("The number of attacks this entity can perform per second.")]
    public float m_AttackRate = 1f;

    [SerializeField, Min(1f)]
    [Tooltip("The maximum damage this entity can take before being defeated.")]
    public float m_MaxHealth = 10f;

    public AudioClip GetRandomWalkingSFX()
    {
        return GetRandomSFX(m_Walking_SFX);
    }

    public AudioClip GetRandomAttackingSFX()
    {
        return GetRandomSFX(m_Attacking_SFX);
    }

    public AudioClip GetRandomReceivingDamageSFX()
    {
        return GetRandomSFX(m_ReceivingDamage_SFX);
    }

    public AudioClip GetRandomDyingSFX()
    {
        return GetRandomSFX(m_Dying_SFX);
    }

    public AudioClip GetRandomHealingSFX()
    {
        return GetRandomSFX(m_Healing_SFX);
    }

    public AudioClip GetRandomSpawningSFX()
    {
        return GetRandomSFX(m_Spawning_SFX);
    }

    private AudioClip GetRandomSFX(List<AudioClip> sfxList)
    {
        if (sfxList == null || sfxList.Count == 0)
        {
            Debug.LogWarning($"No sound effects found for {name}.");
            return null;
        }

        return sfxList[Random.Range(0, sfxList.Count)];
    }
}
