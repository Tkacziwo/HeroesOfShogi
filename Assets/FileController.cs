using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.Linq;

public class FileController : MonoBehaviour
{

    void Start()
    {
        LoadFromJson();

        LoadIcons();
    }

    private void LoadFromJson()
    {
        StaticData.cityBuildingUpgrades.Clear();
        StaticData.cityBuildingUpgrades?.Clear();

        var levelsRes = JsonConvert.DeserializeObject<List<BuildingUpgradeInfo>>
            (Resources.Load<TextAsset>("CityResources/CityBuildingLevels").text);
        StaticData.cityBuildingUpgrades = new(levelsRes);

        StaticData.cityBuildingUpgrades.AddRange(JsonConvert.DeserializeObject<List<BuildingUpgradeInfo>>
            (Resources.Load<TextAsset>("CityResources/CityHallLevels").text));

        var buildingRes = JsonConvert.DeserializeObject<List<CityBuilding>>
            (Resources.Load<TextAsset>("CityResources/CityBuildings").text);

        StaticData.cityBuildings = new(buildingRes);
    }

    private void LoadIcons()
    {
        StaticData.unitIcons?.Clear();

        StaticData.unitIcons =  Resources.LoadAll<Sprite>("Sprites/UnitIcons").ToList();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
