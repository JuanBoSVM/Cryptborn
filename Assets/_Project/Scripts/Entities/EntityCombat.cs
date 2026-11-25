using System.Collections;
using UnityEngine;

public class EntityCombat : MonoBehaviour
{
    [SerializeField]
    protected EntityData m_EntityData;

    private float m_CurrentHealth;
    private bool m_CanAttack = true;
    private Coroutine m_AttackCooldownCoroutine;

    protected virtual void Start()
    {
        if (m_EntityData == null)
        {
            Debug.LogError("EntityData is not assigned in the inspector.", this);
            enabled = false;
            return;
        }

        m_CurrentHealth = Mathf.Max(1f, m_EntityData.m_MaxHealth);
    }

    public virtual void Attack(EntityCombat target)
    {
        if (!enabled || !m_CanAttack || target == null || target == this || target.IsDefeated()) { return; }
        if (m_EntityData == null || m_EntityData.m_AttackRate <= 0f) { return; }

        float attackRange = Mathf.Max(0f, m_EntityData.m_AttackRangeRadius);
        if (Vector2.Distance(transform.position, target.transform.position) > attackRange) { return; }

        target.TakeDamage(m_EntityData.m_AttackDamage);
        m_CanAttack = false;
        m_AttackCooldownCoroutine = StartCoroutine(AttackCooldown(1f / m_EntityData.m_AttackRate));
    }

    public virtual void TakeDamage(float damage)
    {
        if (IsDefeated() || damage <= 0f) { return; }

        m_CurrentHealth = Mathf.Max(0f, m_CurrentHealth - damage);
        if (m_CurrentHealth <= 0f) { gameObject.SetActive(false); }
    }

    public bool IsDefeated()
    {
        return m_CurrentHealth <= 0f;
    }

    protected float GetAttackRange()
    {
        return m_EntityData == null ? 0f : Mathf.Max(0f, m_EntityData.m_AttackRangeRadius);
    }

    private IEnumerator AttackCooldown(float duration)
    {
        yield return new WaitForSeconds(duration);
        m_CanAttack = true;
        m_AttackCooldownCoroutine = null;
    }

    protected virtual void OnDisable()
    {
        if (m_AttackCooldownCoroutine != null)
        {
            StopCoroutine(m_AttackCooldownCoroutine);
            m_AttackCooldownCoroutine = null;
        }

        m_CanAttack = true;
    }
}
