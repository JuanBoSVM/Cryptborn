using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

[RequireComponent(typeof(UIDocument))]
public abstract class Menu : MonoBehaviour
{
    protected UIDocument m_UIDocument;
    protected VisualElement m_RootVisualElement;
    protected VisualElement m_ButtonContainer;
    protected VisualElement m_Selectors;
    protected List<Button> m_Buttons;
    protected Image m_LeftSelector;
    protected Image m_RightSelector;

    private void OnButtonFocus(FocusEvent evt)
    {
        Button focusedButton = evt.target as Button;

        if (m_Selectors.style.opacity.value == 0f) { ShowSelectors(); }

        if (focusedButton != null) { MoveSelectorsToButton(focusedButton); }
    }

    private void OnMouseEnterButton(MouseEnterEvent evt)
    {
        Button hoveredButton = evt.target as Button;

        if (m_Selectors.style.opacity.value == 0f) { ShowSelectors(); }

        if (hoveredButton != null) { MoveSelectorsToButton(hoveredButton); }
    }

    protected Button GetButtonByName(string buttonName)
    {
        if (m_Buttons == null)
        {
            Debug.LogWarning("Button list is not initialized.");
            return null;
        }

        Button button = m_Buttons.Find(b => b.name == buttonName);

        if (button == null)
        {
            Debug.LogWarning($"Button with name '{buttonName}' not found in the Root VisualElement.");
        }

        return button;
    }

    protected void ShowSelectors()
    {
        if (m_Selectors == null)
        {
            Debug.LogError("Selectors container is missing in the Root VisualElement.");
            return;
        }

        m_Selectors.style.opacity = 1f;
    }

    protected void HideSelectors()
    {
        if (m_Selectors == null)
        {
            Debug.LogError("Selectors container is missing in the Root VisualElement.");
            return;
        }

        m_Selectors.style.opacity = 0f;
    }

    private void MatchSelectorsHeightToButtonContainer()
    {
        m_Selectors.style.height = m_ButtonContainer.resolvedStyle.height;
        m_Selectors.style.width = m_ButtonContainer.resolvedStyle.width;
        m_Selectors.style.top = m_ButtonContainer.resolvedStyle.top;
        m_Selectors.style.left = m_ButtonContainer.resolvedStyle.left;
    }

    protected void MoveSelectorsToButton(Button button)
    {
        if (button == null)
        {
            Debug.LogError("Button is null.");
            return;
        }

        if (button.worldBound.width <= 0 || button.worldBound.height <= 0)
        {
            Debug.LogWarning($"Button '{button.name}' has an invalid world bound. Cannot move selectors.");
            return;
        }

        Vector2 buttonPosition = m_Selectors.WorldToLocal(button.worldBound.position);
        float verticalOffset = (button.resolvedStyle.height - m_LeftSelector.resolvedStyle.height) / 2f;

        // Move the selectors to the y position of the button
        m_LeftSelector.style.top = buttonPosition.y + verticalOffset;
        m_RightSelector.style.top = buttonPosition.y + verticalOffset;

        // Move each selector to the left and right of the button
        m_LeftSelector.style.left = buttonPosition.x - m_LeftSelector.resolvedStyle.width;
        m_RightSelector.style.left = buttonPosition.x + button.resolvedStyle.width;
    }

    private void GetBaseComponents()
    {
        m_UIDocument = GetComponent<UIDocument>();
        m_RootVisualElement = m_UIDocument.rootVisualElement;
        m_ButtonContainer = m_RootVisualElement.Q<VisualElement>("Buttons");
        m_Selectors = m_RootVisualElement.Q<VisualElement>("Selectors");
        m_Buttons = m_RootVisualElement.Query<Button>().ToList();
        m_LeftSelector = m_RootVisualElement.Q<Image>("Left");
        m_RightSelector = m_RootVisualElement.Q<Image>("Right");
    }

    private bool ValidateBaseComponents()
    {
        bool allComponentsValid = true;

        if (m_RootVisualElement == null)
        {
            Debug.LogError("Root VisualElement is missing in the UIDocument.");
            allComponentsValid = false;
        }

        if (m_ButtonContainer == null)
        {
            Debug.LogWarning("Button container is missing in the Root VisualElement.");
            allComponentsValid = false;
        }

        if (m_Selectors == null)
        {
            Debug.LogWarning("Selectors container is missing in the Root VisualElement.");
            allComponentsValid = false;
        }

        if (m_Buttons == null || m_Buttons.Count == 0)
        {
            Debug.LogWarning("No buttons found in the Root VisualElement.");
            allComponentsValid = false;
        }

        if (m_LeftSelector == null)
        {
            Debug.LogWarning("Left selector is missing in the Root VisualElement.");
            allComponentsValid = false;
        }

        if (m_RightSelector == null)
        {
            Debug.LogWarning("Right selector is missing in the Root VisualElement.");
            allComponentsValid = false;
        }

        return allComponentsValid;
    }

    private void LinkBaseButtonEvents()
    {
        if (m_Buttons == null) { return; }

        foreach (Button button in m_Buttons)
        {
            if (button == null) { continue; }
            button.RegisterCallback<FocusEvent>(OnButtonFocus);
            button.RegisterCallback<MouseEnterEvent>(OnMouseEnterButton);
        }
    }

    private void UnLinkBaseButtonEvents()
    {
        if (m_Buttons == null) { return; }

        foreach (Button button in m_Buttons)
        {
            if (button == null) continue;
            button.UnregisterCallback<FocusEvent>(OnButtonFocus);
            button.UnregisterCallback<MouseEnterEvent>(OnMouseEnterButton);
        }
    }

    protected virtual void OnDisable()
    {
        if (ValidateBaseComponents()) { UnLinkBaseButtonEvents(); }
    }

    protected virtual void Awake()
    {
        GetBaseComponents();
        if (ValidateBaseComponents()) { LinkBaseButtonEvents(); }

    }

    protected virtual void Start()
    {
        if (ValidateBaseComponents())
        {
            MatchSelectorsHeightToButtonContainer();
        }
    }
}
