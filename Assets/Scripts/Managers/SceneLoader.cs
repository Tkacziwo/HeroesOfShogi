using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Loads scenes. Handles game quitting.
/// </summary>
public class SceneLoader : MonoBehaviour
{
    [SerializeField] private string sceneName;

    public void ChangeScene()
    {
        SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}