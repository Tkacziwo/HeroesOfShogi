using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Handles game creation panel.
/// </summary>
public class GameCreationManager : MonoBehaviour
{
    public bool botEnabled;

    void Start()
    {
        StaticData.botEnabled = true;
    }

    public void ReturnToMenu()
        => SceneManager.LoadScene("MainMenu");

    public void StartGame()
    {
        StaticData.tutorial = false;
        CityNames.takenCityNames.Clear();
        SceneManager.LoadScene("TestMap");
    }
}