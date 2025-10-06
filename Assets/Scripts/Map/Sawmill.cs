using System;
using UnityEngine;

public interface IBuilding
{
    void AddResources();

    BuildingBounds BuildingBounds { get; set; }

    int CapturerId { get; set; }

    bool IsCaptured { get; set; }
}

public class Sawmill : MonoBehaviour, IBuilding
{
    public bool IsCaptured { get; set; }

    public int CapturerId {get; set;}

    public BuildingBounds BuildingBounds { get; set;}

    private void OnEnable()
    {
        OverworldMapController.onTurnEnd += AddResources;
    }

    private void OnDisable()
    {
        OverworldMapController.onTurnEnd -= AddResources;
    }

    /// <summary>
    /// Do not call this method directly. Subscribe to the onTurnEnd event in the OverworldMapController.
    /// </summary>
    public void AddResources()
    {
        BuildingEvents.onResourcesAdd?.Invoke(this);
    }
}

public static class BuildingEvents
{
    public static Action<IBuilding> onResourcesAdd;
}

public class BuildingBounds
{
    public Vector3 topLeft;
    public Vector3 topRight;
    public Vector3 bottomLeft;
    public Vector3 bottomRight;

    /// <summary>
    /// Checks if player position is contained in building bounds
    /// </summary>
    /// <param name="position"></param>
    /// <returns></returns>
    public bool Contains(Vector3 position)
    {
        // [ToDo] improve
        if (position.x >= topLeft.x && position.x <= topRight.x
            && position.z <= topLeft.z
            && position.x >= bottomLeft.z)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
