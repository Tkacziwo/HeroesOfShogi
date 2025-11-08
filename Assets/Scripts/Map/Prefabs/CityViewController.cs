using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class CityViewController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI cityName;

    [SerializeField] private TextMeshProUGUI buildingLevel;

    [SerializeField] private UnityEngine.UI.Button upgradeButton;

    [SerializeField] private TextMeshProUGUI neededWood;
    [SerializeField] private TextMeshProUGUI neededStone;
    [SerializeField] private TextMeshProUGUI neededGold;
    [SerializeField] private TextMeshProUGUI neededLifeResin;

    [SerializeField] private GameObject recruitPanelPrefab;

    [SerializeField] private UnityEngine.Canvas canvasRef;

    private GameObject recruitPanel;

    private City? currentCity;

    private Dictionary<int, string> CityBuildings { get; } = new()
    {
        {1, "CityHall" },
        {2, "Steel Cauldrons" },
        {3, "Temple" },
        {4, "Barracks" },
        {5, "Tower Of Mages" }
    };

    private CityBuilding? chosenBuilding;

    private PlayerResources? playerResources;

    private RequiredResources requiredResources;

    private PlayerCharacterController currentCharacter;

    private int buildingIndex;

    private BuildingUpgradeInfo upgradeInfo;


    public static Action<int, RequiredResources> OnTakePlayerResources;

    public static Action<string, string, BuildingUpgradeInfo> OnBuildingUpgrade;

    public static Action OnCityViewClose;

    public void Setup(City city, PlayerResources playerResources, Canvas canvasRef, PlayerCharacterController character = null)
    {
        if (character != null)
        {
            currentCharacter = character;
        }
        currentCity = city;
        this.playerResources = playerResources;
        cityName.text = $"City: {currentCity.name}";
        upgradeButton.interactable = false;

        this.canvasRef = canvasRef;
    }

    public void DisplayBuildingInfo(int buildingIndex)
    {
        if (currentCity == null) return;

        chosenBuilding = currentCity.GetCityBuildingByName(CityBuildings[buildingIndex]);
        this.buildingIndex = buildingIndex;
        if (chosenBuilding.level == chosenBuilding.maxLevel)
        {
            upgradeButton.interactable = false;
            neededWood.text = $"Wood: \u221E";
            neededStone.text = $"Stone: \u221E";
            neededGold.text = $"Gold: \u221E";
            //neededLifeResin.text = $"Life Resin: \u221E";
        }
        else
        {
            buildingLevel.text = $"Building Level: {chosenBuilding.level} / {chosenBuilding.maxLevel}";

            var upgrades = StaticData.cityBuildingUpgrades;


            upgradeInfo = upgrades.SingleOrDefault(o => o.Name == chosenBuilding.name && o.level == chosenBuilding.level + 1);

            if (upgradeInfo != null)
            {
                requiredResources = upgradeInfo.requiredResources;
                neededWood.text = $"Wood: {upgradeInfo.requiredResources.wood}";
                neededStone.text = $"Stone: {upgradeInfo.requiredResources.stone}";
                neededGold.text = $"Gold: {upgradeInfo.requiredResources.gold}";
                neededLifeResin.text = $"Life Resing: {upgradeInfo.requiredResources.goldResin}";


                if (PlayerHasEnoughResources(upgradeInfo.requiredResources))
                {
                    upgradeButton.interactable = true;
                }
                else
                {
                    upgradeButton.interactable = false;
                }
            }
        }
    }

    private bool PlayerHasEnoughResources(RequiredResources requiredResources)
    {
        if (playerResources.Wood < requiredResources.wood) return false;
        if (playerResources.Stone < requiredResources.stone) return false;
        if (playerResources.Gold < requiredResources.gold) return false;
        //if (playerResources.LifeResin < requiredResources.goldResin) return false;
        return true;
    }

    public void UpgradeBuilding()
    {
        if (chosenBuilding == null) return;

        chosenBuilding.level++;

        buildingLevel.text = $"Building Level: {chosenBuilding.level} / {chosenBuilding.maxLevel}";

        OnTakePlayerResources?.Invoke(currentCity.capturerId, requiredResources);

        OnBuildingUpgrade?.Invoke(currentCity.cityName, chosenBuilding.name, upgradeInfo);

        DisplayBuildingInfo(buildingIndex);
    }

    public void CloseCityView()
    {
        OnCityViewClose?.Invoke();
        this.gameObject.SetActive(false);
        Destroy(this.gameObject);
    }

    public void OpenRecruitPanel()
    {
        if (currentCharacter == null) return;

        recruitPanel = Instantiate(recruitPanelPrefab);
        recruitPanel.transform.SetParent(canvasRef.transform);
        recruitPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);

        var panelScript = recruitPanel.GetComponent<AssignPanelsController>();

        panelScript.Setup(currentCharacter);
    }
}