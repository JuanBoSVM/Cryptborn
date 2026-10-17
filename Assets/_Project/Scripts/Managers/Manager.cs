using UnityEngine;

public abstract class Manager<T> : MonoBehaviour where T : Manager<T>
{
    private static T m_Instance = null;

    public static T Instance
    {
        get
        {
            if (m_Instance == null)
            {
                m_Instance = FindAnyObjectByType<T>();
            }

            return m_Instance;
        }
    }

    protected virtual void Awake()
    {
        if (m_Instance == null) { m_Instance = this as T; }

        else if (m_Instance != this) { Destroy(gameObject); }

        DontDestroyOnLoad(gameObject);
    }
}
