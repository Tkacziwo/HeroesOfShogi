using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

public class ResourceUIController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI goldText;

    [SerializeField] private TextMeshProUGUI stoneText;

    [SerializeField] private TextMeshProUGUI woodText;

    [SerializeField] private TextMeshProUGUI turnText;

    [SerializeField] private UnityEngine.Canvas canvasRef;

    [SerializeField] private PanelController playerCharacterPanel;

    [SerializeField] private CityViewController cityViewPrefab;

    private CityViewController cityView;

    private void OnEnable()
    {
        PlayerController.PlayerSpawned += UpdatePlayerCharacterPanels;
        PlayerModel.UpdateResourceUI += UpdateResourcesUI;
    }

    private void OnDisable()
    {
        PlayerController.PlayerSpawned -= UpdatePlayerCharacterPanels;
        PlayerModel.UpdateResourceUI -= UpdateResourcesUI;
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

        float size = playerCharacterPanel.GetComponent<RectTransform>().rect.width;
        float posX = 60f;

        float posY = -60f;

        foreach (var character in player.GetPlayerCharacters())
        {
            var obj = Instantiate(playerCharacterPanel, new(posX, posY), Quaternion.identity);
            obj.transform.SetParent(canvasRef.transform, false);

            var panelScript = obj.GetComponent<PanelController>();
            panelScript.SetPlayer(character);
            //obj.transform.position = new Vector3(posX, posY);
            posX += size;
        }

        UpdatePlayerCityPanels(player);
    }

    public void UpdatePlayerCityPanels(PlayerModel player)
    {
        if (!player.isRealPlayer) return;

        float size = playerCharacterPanel.GetComponent<RectTransform>().rect.width;
        float posX = 60f;

        float posY = -180f;

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
}