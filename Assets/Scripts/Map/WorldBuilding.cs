using System;

public class WorldBuilding : InteractibleBuilding
{
    //public static Action<WorldBuilding> AddResourcesToCapturer;

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
    //    AddResourcesToCapturer?.Invoke(this);
    //}

    public WorldResource buildingResource;

    public int ResourceAmount;
}