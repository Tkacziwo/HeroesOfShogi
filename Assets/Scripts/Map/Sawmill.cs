using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class InteractibleBuilding : MonoBehaviour
{
    public bool isCaptured;

    public int capturerId;

    public string buildingName;

    public WorldResource buildingResource;

    private void OnEnable()
    {
        OverworldMapController.onTurnEnd += AddResources;
    }

    private void OnDisable()
    {
        OverworldMapController.onTurnEnd -= AddResources;
    }

    public void AddResources()
    {
        throw new NotImplementedException();
    }

    public bool GetIsCaptured()
        => isCaptured;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
            {
                Debug.Log("Hit building: " + hit.transform);
                BuildingEvents.onBuildingClicked?.Invoke(this);

                var collider = hit.transform.GetComponent<BoxCollider>();

                Debug.Log("Collider: " + collider);
                //BuildingEvents.onBuildingClicked?.Invoke(this);

            }

        }
    }
}

public static class BuildingEvents
{
    public static Action<InteractibleBuilding> onResourcesAdd;

    public static Action<InteractibleBuilding> onBuildingClicked;
}