using System.Collections.Generic;
using System.Linq;

public class City : InteractibleBuilding
{
    public List<CityBuilding> cityBuildings = new();

    public string cityName;

    public ProducedUnits producedUnits;

    private void OnEnable()
    {
        CityViewController.OnBuildingUpgrade += HandleBuldingUpgrade;
        OverworldMapController.onTurnEnd += OnTurnEnd;
        OverworldMapController.OnWeekEnd += HandleWeekEnd;
        AssignPanelsController.OnUnitsRecruit += HandleUnitsRecruited;
        BuildingRegistry.Instance?.Register(this);
    }
    private void OnDisable()
    {
        CityViewController.OnBuildingUpgrade -= HandleBuldingUpgrade;
        OverworldMapController.onTurnEnd -= OnTurnEnd;
        OverworldMapController.OnWeekEnd -= HandleWeekEnd;
        AssignPanelsController.OnUnitsRecruit -= HandleUnitsRecruited;
        BuildingRegistry.Instance?.Unregister(this);
    }

    private void HandleUnitsRecruited(ProducedUnits units)
    {
        this.producedUnits = units;
    }

    private void HandleBuldingUpgrade(string cityName, string buildingName, BuildingUpgradeInfo upgradedBuilding)
    {
        if (this.cityName != cityName) return;

        var building = cityBuildings.Single(o => o.name == buildingName);
        var index = cityBuildings.FindIndex(o => o.name == buildingName);
        cityBuildings[index] = new()
        {
            level = upgradedBuilding.level,
            maxLevel = building.maxLevel,
            name = building.name,
            producedAmount = upgradedBuilding.amount,
            producedResource = building.producedResource,
            producedUnits = building.producedUnits
        };
    }

    private void Start()
    {
        InitCity();

        producedUnits = new()
        {
            pawns = 5,
            lances = 3,
            horses = 2
        };
    }

    private void HandleWeekEnd()
    {
        //Replenish units
        producedUnits.pawns = 5;
        producedUnits.lances = 3;
        producedUnits.horses = 2;
    }

    public bool HasAvailableUnits()
    {
        if (producedUnits.lances != 0 || producedUnits.pawns != 0 || producedUnits.horses != 0)
        {
            return true;
        }

        return false;
    }

    private void InitCity()
        => cityBuildings = new(StaticData.cityBuildings);

    public CityBuilding? GetCityBuildingByName(string buildingName)
        => cityBuildings.SingleOrDefault(o => o.name == buildingName);
}

public class ProducedUnits
{
    public int pawns;
    public int lances;
    public int horses;
}