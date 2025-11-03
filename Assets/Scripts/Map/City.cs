using System;
using System.Collections.Generic;

public class City : InteractibleBuilding
{
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