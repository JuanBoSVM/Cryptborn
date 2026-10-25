using UnityEngine;

[CreateAssetMenu(fileName = "EntityData", menuName = "Scriptable Objects/EntityData")]
public class EntityData : ScriptableObject
{
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
}
