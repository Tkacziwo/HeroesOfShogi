using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Holds data acquired from game creation scene.
/// </summary>
public static class StaticData
{
    public static int botDifficulty;

    public static bool botEnabled;

    public static string map;

    public static bool tutorial;

    public static float playerMovementSpeed;

    public static List<CityBuilding> cityBuildings = new();

    public static List<BuildingUpgradeInfo> cityBuildingUpgrades = new();

    public static List<Sprite> unitIcons = new();

    public static readonly int battleMapHeight = 15;

    public static readonly int battleMapWidth = 9;

    public static List<Unit> unitTemplates = new();
}