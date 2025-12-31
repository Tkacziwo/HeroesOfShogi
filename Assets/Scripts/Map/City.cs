using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

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

    }
    private void OnDisable()
    {
        CityViewController.OnBuildingUpgrade -= HandleBuldingUpgrade;
        OverworldMapController.onTurnEnd -= OnTurnEnd;
        OverworldMapController.OnWeekEnd -= HandleWeekEnd;
        AssignPanelsController.OnUnitsRecruit -= HandleUnitsRecruited;
        if (BuildingRegistry.Instance != null)
        {
            BuildingRegistry.Instance?.Unregister(this);
        }
    }

    private void HandleUnitsRecruited(Tuple<string, ProducedUnits> units)
    {
        if (units.Item1 == cityName)
        {
            this.producedUnits = units.Item2;
        }
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

        var barracks = cityBuildings.Single(o => o.name == "Barracks");
        producedUnits.goldGenerals = (int)barracks.producedAmount;
        producedUnits.silverGenerals = (int)barracks.producedAmount;

        var temple = cityBuildings.Single(o => o.name == "Temple");
        producedUnits.bishops = (int)temple.producedAmount;
        var cauldrons = cityBuildings.Single(o => o.name == "Steel Cauldrons");
        producedUnits.rooks = (int)cauldrons.producedAmount;
    }

    private void Start()
    {
        InitCity();
        cityName = Guid.NewGuid().ToString();

        Random rand = new();

        var names = CityNames.cityNames;
        var takenNames = CityNames.takenCityNames;

        while (true)
        {
            var chosenIndex = rand.Next(0, names.Count);
            if (!takenNames.Contains(names[chosenIndex]))
            {
                cityName = names[chosenIndex];
                CityNames.takenCityNames.Add(cityName);
                break;
            }
        }

        producedUnits = new()
        {
            pawns = 3,
            lances = 2,
            horses = 1,
            goldGenerals = 0,
            silverGenerals = 0,
            rooks = 0,
            bishops = 0,
        };
        BuildingRegistry.Instance?.Register(this);
    }

    public ProducedUnits GetAvailableUnits()
    {
        return producedUnits;
    }

    private void HandleWeekEnd()
    {
        producedUnits.pawns = 3;
        producedUnits.lances = 2;
        producedUnits.horses = 1;

        var barracks = cityBuildings.Single(o => o.name == "Barracks");
        producedUnits.goldGenerals = (int)barracks.producedAmount;
        producedUnits.silverGenerals = (int)barracks.producedAmount;

        var temple = cityBuildings.Single(o => o.name == "Temple");
        producedUnits.bishops = (int)temple.producedAmount;
        var cauldrons = cityBuildings.Single(o => o.name == "Steel Cauldrons");
        producedUnits.rooks = (int)cauldrons.producedAmount;
    }

    public bool HasAvailableUnits()
        => producedUnits.HasAvailableUnits();

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
    public int goldGenerals;
    public int silverGenerals;
    public int rooks;
    public int bishops;

    public bool HasAvailableUnits()
        => pawns != 0 || lances != 0 || horses != 0 || goldGenerals != 0 || silverGenerals != 0 || rooks != 0 || bishops != 0;
}