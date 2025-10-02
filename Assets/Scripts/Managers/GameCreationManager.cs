using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Handles game creation panel.
/// </summary>
public class GameCreationManager : MonoBehaviour
{
    public bool botEnabled, classicRules;

    public float turnTimeLimit, gameTimeLimit;

    public int botDifficulty;

    [SerializeField] private Slider botSlider;

    [SerializeField] private Toggle botCheckbox;

    [SerializeField] private Image mapImage;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StaticData.botEnabled = true;
    }

    public void ReturnToMenu()
        => SceneManager.LoadScene("MainMenu");

    public void StartGame()
    {
        StaticData.map = mapImage.GetComponent<Image>().sprite.name;
        StaticData.tutorial = false;
        SceneManager.LoadScene("Game");
    }

    public void LoadPreviousMap()
    {
        if (mapImage.GetComponent<Image>().sprite.name == "GrasslandsImage")
        {
            var sprite = Resources.Load<Sprite>("Maps/DesertImage");
            mapImage.GetComponent<Image>().sprite = sprite;
        }
        else
        {
            var sprite = Resources.Load<Sprite>("Maps/GrasslandsImage");
            mapImage.GetComponent<Image>().sprite = sprite;
        }
    }

    public void LoadNextMap()
    {
        if (mapImage.GetComponent<Image>().sprite.name == "GrasslandsImage")
        {
            var sprite = Resources.Load<Sprite>("Maps/DesertImage");
            mapImage.GetComponent<Image>().sprite = sprite;
        }
        else
        {
            var sprite = Resources.Load<Sprite>("Maps/GrasslandsImage");
            mapImage.GetComponent<Image>().sprite = sprite;
        }
    }

    public void UpdateBotDifficulty()
    {
        botDifficulty = (int)botSlider.value;
        StaticData.botDifficulty = (int)botSlider.value;
    }

    public void EnableBot()
    {
        StaticData.botEnabled = botCheckbox.isOn;
    }

    public void UpdateTurnTimeLimit(float n)
        => turnTimeLimit = n;


    public void UpdateGameTimeLimit(float n)
        => gameTimeLimit = n;
}
