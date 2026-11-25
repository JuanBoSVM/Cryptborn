using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : EntityCombat
{
    private InputAction m_AttackAction;

    private void Awake()
    {
        m_AttackAction = InputSystem.actions.FindAction("Player/Attack");
        if (m_AttackAction == null)
        {
            Debug.LogError("Player/Attack input action was not found.", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (m_AttackAction != null) { m_AttackAction.performed += OnAttackPerformed; }
    }

    protected override void OnDisable()
    {
        if (m_AttackAction != null) { m_AttackAction.performed -= OnAttackPerformed; }
        base.OnDisable();
    }

    private void OnAttackPerformed(InputAction.CallbackContext context)
    {
        Attack(FindNearestTarget());
    }

    private EntityCombat FindNearestTarget()
    {
        Collider2D[] nearbyColliders = Physics2D.OverlapCircleAll(transform.position, GetAttackRange());
        EntityCombat nearestTarget = null;
        float nearestDistance = float.PositiveInfinity;

        foreach (Collider2D nearbyCollider in nearbyColliders)
        {
            EntityCombat target = nearbyCollider.GetComponentInParent<EntityCombat>();
            if (target == null || target == this || target.IsDefeated()) { continue; }

            float distance = Vector2.Distance(transform.position, target.transform.position);
            if (distance < nearestDistance)
            {
                nearestTarget = target;
                nearestDistance = distance;
            }
        }

        return nearestTarget;
    }
}
