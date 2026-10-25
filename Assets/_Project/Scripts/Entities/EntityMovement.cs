using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EntityMovement : MonoBehaviour
{
    [SerializeField]
    protected EntityData m_EntityData;

    protected Rigidbody2D m_Rigidbody;

    protected Vector2 m_TargetDirection;

    private float maxSpeed { get { return m_EntityData.maxSpeed; } }

    private float acceleration { get { return m_EntityData.acceleration; } }

    private float deceleration { get { return m_EntityData.deceleration; } }

    private Vector2 movementDirection
    {
        get { return m_Rigidbody.linearVelocity; }
        set { m_Rigidbody.linearVelocity = value; }
    }

    protected virtual void Move(Vector2 direction)
    {
        movementDirection = Vector2.MoveTowards
            (
            movementDirection,
            direction * maxSpeed,
            (direction == Vector2.zero ? deceleration : acceleration) * Time.fixedDeltaTime
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
        Move(m_TargetDirection);
    }
}
