using TMPro;
using UnityEngine;

public class ResourceUIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI goldText;

    [SerializeField] private TextMeshProUGUI stoneText;

    [SerializeField] private TextMeshProUGUI woodText;

    [SerializeField] private TextMeshProUGUI turnText;

    [SerializeField] private UnityEngine.Canvas canvasRef;

    [SerializeField] private PanelController playerCharacterPanel;

    [SerializeField] private CityViewController cityViewPrefab;

    [SerializeField] private GameObject UIRef;

    [SerializeField] private GameObject gameOver;


    private CityViewController cityView;

    private void OnEnable()
    {
        PlayerController.PlayerSpawned += UpdatePlayerCharacterPanels;
        PlayerModel.UpdateResourceUI += UpdateResourcesUI;
        BattleDeploymentController.OnBattleStarted += HandleOnBattleStarted;
        GameOverController.OnBackToMap += HandleOnBattleEnded;
    }

    private void OnDisable()
    {
        PlayerController.PlayerSpawned -= UpdatePlayerCharacterPanels;
        PlayerModel.UpdateResourceUI -= UpdateResourcesUI;
        BattleDeploymentController.OnBattleStarted -= HandleOnBattleStarted;
        GameOverController.OnBackToMap -= HandleOnBattleEnded;
    }

    private void HandleOnBattleStarted(bool state)
    {
        UIRef.SetActive(state);
    }

    private void HandleOnBattleEnded()
    {
        UIRef.SetActive(true);
    }

    private uint turnNumber = 1;

    private void Start()
    {
        turnText.text = $"Turn: {turnNumber}";
    }

    public void UpdateResourcesUI(PlayerResources resources)
    {
        goldText.text = $"Gold: {resources.Gold}";
        stoneText.text = $"Stone: {resources.Stone}";
        woodText.text = $"Wood: {resources.Wood}";
    }

    public void IncrementTurnNumber()
    {
        turnNumber++;
        turnText.text = $"Turn: {turnNumber}";
    }

    public void UpdatePlayerCharacterPanels(PlayerModel player)
    {
        if (!player.isRealPlayer) return;

        float posX = 60f;

        float posY = -60f;

        var obj = Instantiate(playerCharacterPanel, new(posX, posY), Quaternion.identity);
        obj.transform.SetParent(canvasRef.transform, false);

        var panelScript = obj.GetComponent<PanelController>();
        panelScript.SetPlayer(player.GetCurrentPlayerCharacter());

        UpdatePlayerCityPanels(player);
    }

    public void UpdatePlayerCityPanels(PlayerModel player)
    {
        if (!player.isRealPlayer) return;

        float size = playerCharacterPanel.GetComponent<RectTransform>().rect.width;
        float posX = 180f;

        float posY = -60f;

        foreach (var city in player.GetPlayerCities())
        {
            var obj = Instantiate(playerCharacterPanel, new(posX, posY), Quaternion.identity);
            obj.transform.SetParent(canvasRef.transform, false);

            var panelScript = obj.GetComponent<PanelController>();
            panelScript.SetCity(city);
            posX += size;
        }
    }

    public void DisplayCityInfo(City city, PlayerResources playerResources, PlayerCharacterController character = null)
    {
        cityView = Instantiate(cityViewPrefab);
        cityView.transform.SetParent(canvasRef.transform);
        cityView.GetComponent<RectTransform>().anchoredPosition = new Vector2(408, 357);

        var script = cityView.GetComponent<CityViewController>();
        script.Setup(city, playerResources, canvasRef, character);
    }

    public void ShowGameOverScreen(int score)
    {
        gameOver = Instantiate(gameOver);
        gameOver.transform.SetParent(canvasRef.transform);
        this.gameOver.GetComponent<RectTransform>().anchoredPosition = new(0, 0);
        gameOver.GetComponent<GameEndController>().SetScore(score.ToString());
    }
}