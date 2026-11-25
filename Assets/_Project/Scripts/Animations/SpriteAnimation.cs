using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteAnimation : MonoBehaviour
{
    [SerializeField]
    private EntityData m_EntityData;

    private Rigidbody2D m_Rigidbody;
    private Animator m_Animator;
    private SpriteRenderer m_SpriteRenderer;

    private Vector2 MovementDirection => m_Rigidbody?.linearVelocity ?? Vector2.zero;
    private bool IsMoving => MovementDirection != Vector2.zero;

    private void PlayFootstepSound()
    {
        AudioClip clip = m_EntityData.GetRandomWalkingSFX();

        if (clip != null)
        {
            SoundManager.QueueSound(clip, transform.position);
        }
    }

    private void FlipSprite()
    {
        if (MovementDirection == Vector2.zero) { return; }

        if (MovementDirection.x > 0f) { m_SpriteRenderer.flipX = false; }
        else if (MovementDirection.x < 0f) { m_SpriteRenderer.flipX = true; }
    }

    private void Awake()
    {
        m_Animator = GetComponent<Animator>();
        m_SpriteRenderer = GetComponent<SpriteRenderer>();

        if (m_Animator == null)
        {
            Debug.LogError($"No Animator found on {gameObject.name}.");
        }

        if (m_SpriteRenderer == null)
        {
            Debug.LogError($"No SpriteRenderer found on {gameObject.name}.");
        }
    }


    private void Start()
    {
        m_Rigidbody = GetComponentInParent<Rigidbody2D>();

        if (m_Rigidbody == null)
        {
            Debug.LogError($"No Rigidbody2D found in parent of {gameObject.name}.");
        }

        if (m_EntityData != null && m_EntityData.m_Sprite != null)
        {
            m_SpriteRenderer.sprite = m_EntityData.m_Sprite;
        }
    }

    private void FixedUpdate()
    {
        m_Animator.SetBool("IsMoving", IsMoving);
        FlipSprite();
    }
}
