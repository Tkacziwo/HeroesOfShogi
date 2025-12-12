using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameEndController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;

    public void SetScore(string score)
        => scoreText.text = $"Score: {score}";

    public void OnClick()
        => SceneManager.LoadScene("MainMenu");
}