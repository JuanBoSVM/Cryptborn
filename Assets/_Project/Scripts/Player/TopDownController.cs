using UnityEngine;
using UnityEngine.InputSystem;

public class TopDownController : EntityMovement
{
    private InputAction m_MoveAction;

    private void GetActions()
    {
        m_MoveAction = InputSystem.actions.FindAction("Player/Move");
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        m_TargetDirection = context.ReadValue<Vector2>();
    }

    private void SubscribeToInputEvents()
    {
        m_MoveAction.performed += OnMovePerformed;
    }

    private void UnsubscribeFromInputEvents()
    {
        m_MoveAction.performed -= OnMovePerformed;
    }

    protected override void Awake()
    {
        base.Awake();
        GetActions();
    }

    private void OnEnable()
    {
        SubscribeToInputEvents();
    }

    private void OnDisable()
    {
        UnsubscribeFromInputEvents();
    }
}
