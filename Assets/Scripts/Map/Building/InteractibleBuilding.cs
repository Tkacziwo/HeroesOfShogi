using System;
using UnityEngine;

public class InteractibleBuilding : MonoBehaviour
{
    public bool isCaptured;

    public int capturerId;

    public string buildingName;

    public static Action<InteractibleBuilding> AddResourcesToCapturer;

    public void CaptureBuilding(int capturerId, Color? capturerColor = null)
    {
        if (capturerId == this.capturerId) return;

        if (capturerColor != null)
        {
            this.GetComponent<MeshRenderer>().material.color = capturerColor.Value;
        }

        isCaptured = true;
        this.capturerId = capturerId;
    }

    public void FindPathToBuilding()
    {
        Debug.Log("Hit building: " + buildingName);
        var collider = this.transform.GetComponent<BoxCollider>();
        BuildingEvents.onBuildingClicked?.Invoke(this);
    }

    private void OnEnable()
    {
        OverworldMapController.onTurnEnd += OnTurnEnd;
        BuildingRegistry.Instance?.Register(this);
    }

    private void OnDisable()
    {
        OverworldMapController.onTurnEnd -= OnTurnEnd;
        BuildingRegistry.Instance?.Unregister(this);
    }

    public void OnTurnEnd()
       => AddResourcesToCapturer?.Invoke(this);

    public bool GetIsCaptured()
       => isCaptured;
}

public static class BuildingEvents
{
    public static Action<InteractibleBuilding> onResourcesAdd;

    public static Action<InteractibleBuilding> onBuildingClicked;
}