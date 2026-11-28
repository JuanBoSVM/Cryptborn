using UnityEngine;
using UnityEngine.InputSystem;

public class MenuManager : Manager<MenuManager>
{
    [SerializeField] private Menu m_InitialMenu;

    private Menu[] m_Menus;
    private Menu m_CurrentMenu;
    private Menu m_PreviousMenu;
    private InputAction m_PauseAction;
    private float m_TimeScaleBeforePause;
    private bool m_IsPaused;

    public bool IsPaused => m_IsPaused;

    protected override void Awake()
    {
        base.Awake();
        if (Instance != this)
        {
            return;
        }

        m_Menus = GetComponentsInChildren<Menu>(true);
        m_PauseAction = InputSystem.actions.FindAction("UI/Pause");
        if (m_PauseAction == null)
        {
            Debug.LogError("UI/Pause input action was not found.", this);
        }

        if (m_InitialMenu == null || !IsManagedMenu(m_InitialMenu))
        {
            Debug.LogError("The initial menu must be assigned and be a child of the MenuManager.", this);
            return;
        }

        foreach (Menu menu in m_Menus)
        {
            if (menu.gameObject.activeSelf)
            {
                menu.gameObject.SetActive(false);
            }
        }

        ShowMenu(m_InitialMenu);
    }

    private void OnEnable()
    {
        if (m_PauseAction == null) { return; }

        m_PauseAction.performed += OnPausePerformed;
        m_PauseAction.Enable();
    }

    private void OnDisable()
    {
        if (m_PauseAction == null) { return; }

        m_PauseAction.performed -= OnPausePerformed;
    }

    private void OnPausePerformed(InputAction.CallbackContext context)
    {
        if (m_IsPaused)
        {
            ResumeGame();
            return;
        }

        if (m_CurrentMenu != null) { return; }

        PauseGame();
    }

    private void PauseGame()
    {
        PauseMenu pauseMenu = null;
        foreach (Menu menu in m_Menus)
        {
            pauseMenu = menu as PauseMenu;
            if (pauseMenu != null) { break; }
        }

        if (pauseMenu == null)
        {
            Debug.LogError("Cannot pause because no PauseMenu is a child of the MenuManager.", this);
            return;
        }

        m_TimeScaleBeforePause = Time.timeScale;
        m_IsPaused = true;
        Time.timeScale = 0f;
        ShowMenu(pauseMenu);
    }

    public void ResumeGame()
    {
        if (!m_IsPaused) { return; }

        m_IsPaused = false;
        Time.timeScale = m_TimeScaleBeforePause;
        HideCurrentMenu();
    }

    public void ShowMenu(Menu menu)
    {
        if (menu == null)
        {
            Debug.LogError("Cannot show a null menu.", this);
            return;
        }

        if (!IsManagedMenu(menu))
        {
            Debug.LogError($"Menu '{menu.name}' is not a child of this MenuManager.", menu);
            return;
        }

        if (m_CurrentMenu == menu && menu.gameObject.activeSelf)
        {
            return;
        }

        if (m_CurrentMenu != null)
        {
            m_PreviousMenu = m_CurrentMenu;
            m_CurrentMenu.gameObject.SetActive(false);
        }

        m_CurrentMenu = menu;
        m_CurrentMenu.gameObject.SetActive(true);
    }

    public void ShowMenu<T>() where T : Menu
    {
        foreach (Menu menu in m_Menus)
        {
            if (menu is T)
            {
                ShowMenu(menu);
                return;
            }
        }

        Debug.LogError($"No child menu of type '{typeof(T).Name}' was found.", this);
    }

    public void ShowPreviousMenu()
    {
        if (m_PreviousMenu == null)
        {
            Debug.LogError("No previous menu is available to show.", this);
            return;
        }

        ShowMenu(m_PreviousMenu);
    }

    public void HideCurrentMenu()
    {
        if (m_CurrentMenu != null)
        {
            m_CurrentMenu.gameObject.SetActive(false);
            m_CurrentMenu = null;
        }

        m_PreviousMenu = null;
    }

    private bool IsManagedMenu(Menu menu)
    {
        if (m_Menus == null)
        {
            return false;
        }

        foreach (Menu managedMenu in m_Menus)
        {
            if (managedMenu == menu)
            {
                return true;
            }
        }

        return false;
    }

}
