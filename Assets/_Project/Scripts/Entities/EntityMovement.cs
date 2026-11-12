using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EntityMovement : MonoBehaviour
{
    [SerializeField]
    protected EntityData m_EntityData;

    protected Rigidbody2D m_Rigidbody;

    protected Vector2 m_TargetDirection;

    protected float MaxSpeed { get { return m_EntityData.maxSpeed; } }

    protected float Acceleration { get { return m_EntityData.acceleration; } }

    protected float Deceleration { get { return m_EntityData.deceleration; } }

    protected Vector2 MovementDirection
    {
        get { return m_Rigidbody.linearVelocity; }
        set { m_Rigidbody.linearVelocity = value; }
    }

    protected virtual void MoveTowards(Vector2 direction)
    {
        MovementDirection = Vector2.MoveTowards
            (
            MovementDirection,
            direction * MaxSpeed,
            (direction == Vector2.zero ? Deceleration : Acceleration) * Time.fixedDeltaTime
            );
    }

    protected virtual void GetComponents()
    {
        m_Rigidbody = GetComponent<Rigidbody2D>();
    }

    protected virtual void ValidateComponents()
    {
        if (m_EntityData == null)
        {
            Debug.LogError("EntityData is not assigned in the inspector.");
        }

        if (m_Rigidbody == null)
        {
            Debug.LogError("Rigidbody2D component is missing.");
        }
    }

    protected virtual void Awake()
    {
        GetComponents();
        ValidateComponents();
    }

    protected virtual void FixedUpdate()
    {
        MoveTowards(m_TargetDirection);
    }
}
