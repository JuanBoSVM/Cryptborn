using System.Collections;
using UnityEngine;

public class EnemyCombat : EntityCombat
{
    private AIMovement m_AIMovement;
    private EntityCombat m_PlayerCombat;

    protected override void Start()
    {
        base.Start();
        if (!enabled) { return; }

        m_AIMovement = GetComponent<AIMovement>();
        if (m_AIMovement == null)
        {
            Debug.LogError("AIMovement component is missing.", this);
            enabled = false;
            return;
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        m_PlayerCombat = player != null ? player.GetComponent<EntityCombat>() : null;
        if (m_PlayerCombat == null)
        {
            Debug.LogError("Player EntityCombat component was not found.", this);
            enabled = false;
            return;
        }

        StartCoroutine(AttackWhileEngaging());
    }

    private IEnumerator AttackWhileEngaging()
    {
        while (enabled)
        {
            if (m_AIMovement.IsAttacking())
            {
                Attack(m_PlayerCombat);
                float rate = m_EntityData.m_AttackRate;
                yield return new WaitForSeconds(rate > 0f ? 1f / rate : 1f);
            }
            else
            {
                yield return null;
            }
        }
    }
}
