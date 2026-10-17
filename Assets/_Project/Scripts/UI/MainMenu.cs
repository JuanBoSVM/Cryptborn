using UnityEngine;
using UnityEngine.UIElements;

public class MainMenu : Menu
{
    Button m_PlayButton;
    Button m_ExitButton;

    private void GetComponents()
    {
        m_PlayButton = GetButtonByName("Play");
        m_ExitButton = GetButtonByName("Exit");
    }

    private bool ValidateComponents()
    {
        bool allComponentsValid = true;

        if (m_PlayButton == null)
        {
            Debug.LogError("Play button is missing in the MainMenu.");
            allComponentsValid = false;
        }

        if (m_ExitButton == null)
        {
            Debug.LogError("Exit button is missing in the MainMenu.");
            allComponentsValid = false;
        }

        return allComponentsValid;
    }

    private void LinkButtonEvents()
    {
        m_PlayButton.clicked += OnPlayButtonClicked;
        m_ExitButton.clicked += OnExitButtonClicked;
    }

    private void UnLinkButtonEvents()
    {
        m_PlayButton.clicked -= OnPlayButtonClicked;
        m_ExitButton.clicked -= OnExitButtonClicked;
    }

    private void OnPlayButtonClicked()
    {
        GameManager.StartGame();
    }

    private void OnExitButtonClicked()
    {
        GameManager.ExitGame();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (ValidateComponents()) { UnLinkButtonEvents(); }
    }

    protected override void Awake()
    {
        base.Awake();
        GetComponents();
        if (ValidateComponents())
        {
            LinkButtonEvents();
        }
    }

    protected override void Start()
    {
        base.Start();
        ValidateComponents();
    }
}
