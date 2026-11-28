using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Manager<GameManager>
{
    private void AdvanceToNextScene()
    {
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        SceneManager.LoadScene(nextSceneIndex);
        SoundManager.StartMusic(nextSceneIndex);
    }

    public static void HandleDungeonExit()
    {

        Instance.AdvanceToNextScene();
    }

    public static void StartGame()
    {
        Instance.AdvanceToNextScene();
    }

    public static void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;

#else
        Application.Quit();

#endif
    }

    private void Start()
    {
        SoundManager.StartMusic(SceneManager.GetActiveScene().buildIndex);
    }
}
