using UnityEngine;
using UnityEngine.UIElements;

public class PauseMenu : Menu
{
    private Button m_ContinueButton;
    private Button m_SettingsButton;
    private Button m_QuitButton;

    private void GetComponents()
    {
        m_ContinueButton = GetButtonByName("Continue");
        m_SettingsButton = GetButtonByName("Settings");
        m_QuitButton = GetButtonByName("Quit");
    }

    private bool ValidateComponents()
    {
        bool allComponentsValid = true;

        if (m_ContinueButton == null)
        {
            Debug.LogError("Continue button is missing in the PauseMenu.", this);
            allComponentsValid = false;
        }

        if (m_SettingsButton == null)
        {
            Debug.LogError("Settings button is missing in the PauseMenu.", this);
            allComponentsValid = false;
        }

        if (m_QuitButton == null)
        {
            Debug.LogError("Quit button is missing in the PauseMenu.", this);
            allComponentsValid = false;
        }

        return allComponentsValid;
    }

    private void LinkButtonEvents()
    {
        m_ContinueButton.clicked += OnContinueButtonClicked;
        m_SettingsButton.clicked += OnSettingsButtonClicked;
        m_QuitButton.clicked += OnQuitButtonClicked;
    }

    private void UnlinkButtonEvents()
    {
        m_ContinueButton.clicked -= OnContinueButtonClicked;
        m_SettingsButton.clicked -= OnSettingsButtonClicked;
        m_QuitButton.clicked -= OnQuitButtonClicked;
    }

    private void OnContinueButtonClicked()
    {
        MenuManager.Instance.ResumeGame();
    }

    private void OnSettingsButtonClicked()
    {
        MenuManager.Instance.ShowMenu<SettingsMenu>();
    }

    private void OnQuitButtonClicked()
    {
        GameManager.ExitGame();
    }

    protected override void Awake()
    {
        base.Awake();
        GetComponents();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        GetComponents();
        if (ValidateComponents()) { LinkButtonEvents(); }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (ValidateComponents()) { UnlinkButtonEvents(); }
    }
}
