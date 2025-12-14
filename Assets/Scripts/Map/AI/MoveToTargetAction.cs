using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.Behavior;
using Unity.Properties;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "MoveToTarget", story: "[Self] moves to target tile", category: "Action", id: "23af5e4d20913c9d5de336f4ba2a8815")]
public partial class MoveToTargetAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<Tilemap> tilemap;

    private PathingController Pathing { get; set; } = new();


    protected override Status OnStart()
    {
        var character = Self.Value.GetComponent<NPCModel>().GetCurrentPlayerCharacter();

        var startPos = character.characterPosition;

        var npc = Self.Value.GetComponent<NPCModel>();

        npc.RemainingPath = FindPath(startPos, GetRandomWalkableTile(tilemap, startPos));
        npc.ChosenBuilding = null;
        npc.ReachedDestination = false;
        return Status.Success;
    }

    private Vector3Int GetRandomWalkableTile(Tilemap map, Vector3Int startPos)
    {
        BoundsInt bounds = map.cellBounds;

        for (int i = 0; i < 100; i++)
        {
            int x = UnityEngine.Random.Range(bounds.xMin, bounds.xMax);
            int y = UnityEngine.Random.Range(bounds.yMin, bounds.yMax);

            Vector3Int pos = new(x, y, 0);

            if (map.HasTile(pos))
            {
                var tile = map.GetTile<MapTile>(pos);
                if (tile != null && tile.IsTraversable && !pos.Equals(startPos))
                {
                    return pos;
                }
            }
        }

        return Vector3Int.zero;
    }

    private List<TileInfo> FindPath(Vector3Int start, Vector3Int end)
    {
        Pathing.SetParameters(tilemap, start, end);
        return Pathing.FindPath();
    }
}