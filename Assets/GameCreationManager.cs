using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameCreationManager : MonoBehaviour
{
    public bool botEnabled, classicRules;

    public float turnTimeLimit, gameTimeLimit;

    public int botDifficulty;

    [SerializeField] private Slider botSlider;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ReturnToMenu()
        => SceneManager.LoadScene("MainMenu");

    public void StartGame()
        => SceneManager.LoadScene("Game");

    public void LoadPreviousMap()
    {

    }

    public void LoadNextMap()
    {

    }

    public void UpdateBotDifficulty()
        => botDifficulty = (int)botSlider.value;

    public void UpdateTurnTimeLimit(float n)
        => turnTimeLimit = n;


    public void UpdateGameTimeLimit(float n)
        => gameTimeLimit = n;
}
