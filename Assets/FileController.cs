using System;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.Linq;
using System.Runtime.CompilerServices;

[Serializable]
public class UnitTemplate
{
    public UnitEnum UnitName { get; set; }
    public int HealthPoints { get; set; }
    public int AttackPower { get; set; }
    public int SizeInArmy { get; set; }
}

public class FileController : MonoBehaviour
{

    void Start()
    {
        LoadFromJson();

        LoadIcons();

        LoadUnitsFromJson();
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

    private void LoadUnitsFromJson()
    {
        StaticData.unitTemplates?.Clear();

        var text = Resources.Load<TextAsset>("Prefabs/Units/UnitInfo").text;

        var templates = JsonConvert.DeserializeObject<List<UnitTemplate>>(text);

        List<Unit> convertedTemplates = new();

        foreach (var item in templates)
        {
            convertedTemplates.Add(new()
            {
                UnitName = item.UnitName,
                HealthPoints = item.HealthPoints,
                AttackPower = item.AttackPower,
                SizeInArmy = item.SizeInArmy
            });
        }

        StaticData.unitTemplates = new(convertedTemplates);
    }

    private void LoadIcons()
    {
        StaticData.unitIcons?.Clear();

        StaticData.unitIcons = Resources.LoadAll<Sprite>("Sprites/UnitIcons").ToList();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
