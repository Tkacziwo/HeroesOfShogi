using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class BuildingRegistry : MonoBehaviour
{
    public List<InteractibleBuilding> buildings = new();
    public static BuildingRegistry Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void Register(InteractibleBuilding b)
    {
        if (!buildings.Contains(b))
            buildings.Add(b);
    }

    public void Unregister(InteractibleBuilding b)
    {
        buildings.Remove(b);
    }

    public List<InteractibleBuilding> GetAllBuildings()
    {
        return buildings;
    }
}