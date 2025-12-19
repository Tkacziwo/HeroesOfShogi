using System.Collections.Generic;

public class NPCModel : PlayerModel
{
    public List<TileInfo> RemainingPath { get; set; } = new();

    public InteractibleBuilding ChosenBuilding { get; set; }

    public bool ReachedDestination { get; set; }
}