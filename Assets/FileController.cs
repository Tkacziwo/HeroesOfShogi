using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class FileController : MonoBehaviour
{

    void Start()
    {
        LoadFromJson();
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

    // Update is called once per frame
    void Update()
    {

    }
}
