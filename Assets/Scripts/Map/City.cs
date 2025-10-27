using System;
using System.Collections.Generic;

public class City : InteractibleBuilding
{
    //public static Action<City> AddResourcesFromCity;

    //private void OnEnable()
    //{
    //    OverworldMapController.onTurnEnd += OnTurnEnd;
    //}

    //private void OnDisable()
    //{
    //    OverworldMapController.onTurnEnd -= OnTurnEnd;
    //}

    //public void OnTurnEnd()
    //{
    //    AddResourcesFromCity?.Invoke(this);
    //}


    public List<CityBuilding> cityBuildings = new();

    private void Start()
    {
        InitCity();
    }

    public void InitCity()
    {
        cityBuildings = new()
        { new()
        {
            name = "CityHall",
            level = 1,
            producedResource = WorldResource.Gold,
            producedAmount = 1000,
            maxLevel = 3,
            producedUnits = null,
        }
        };
    }
}

public class CityBuilding
{
    public string name;

    public uint level;

    public uint maxLevel;

    public WorldResource producedResource;

    public uint producedAmount;

    public string? producedUnits;
}
