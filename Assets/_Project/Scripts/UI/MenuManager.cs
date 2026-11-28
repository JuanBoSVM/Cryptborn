using UnityEngine;

public class MenuManager : Manager<MenuManager>
{
    [SerializeField] private Menu m_InitialMenu;

    private Menu[] m_Menus;
    private Menu m_CurrentMenu;

    protected override void Awake()
    {
        base.Awake();
        if (Instance != this)
        {
            return;
        }

        m_Menus = GetComponentsInChildren<Menu>(true);

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

    public void HideCurrentMenu()
    {
        if (m_CurrentMenu == null)
        {
            return;
        }

        m_CurrentMenu.gameObject.SetActive(false);
        m_CurrentMenu = null;
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
