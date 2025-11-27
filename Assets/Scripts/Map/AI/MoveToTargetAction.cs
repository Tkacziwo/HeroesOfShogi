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
    private PathingController pathing = new();

    public static Action<List<Tuple<int, List<TileInfo>>>> OnBotMove;



    protected override Status OnStart()
    {
        var characters = Self.Value.GetComponent<PlayerModel>().GetPlayerCharacters();
        List<Tuple<int, List<TileInfo>>> botResults = new();

        foreach (var character in characters)
        {
            var startPos = character.characterPosition;

            Vector3Int endPos;

            if (!character.unreachedBotDestination.Equals(new Vector3Int(0, 0, 0)))
            {
                endPos = character.unreachedBotDestination;
                character.unreachedBuilding = null;
            }
            else
            {
                endPos = GetRandomWalkableTile(tilemap, startPos);
                character.unreachedBuilding = null;
            }

            character.unreachedBotDestination = new Vector3Int(0, 0, 0);

            var path = FindPath(startPos, endPos);

            if (path.Count > character.GetRemainingMovementPoints())
            {
                character.unreachedBotDestination = endPos;
                character.unreachedBuilding = null;
            }


            botResults.Add(new(character.characterId, path));
        }

        OnBotMove?.Invoke(botResults);
        return Status.Success;
    }



    private Vector3Int GetRandomWalkableTile(Tilemap map, Vector3Int startPos)
    {
        BoundsInt bounds = map.cellBounds;

        for (int i = 0; i < 100; i++)
        {
            int x = UnityEngine.Random.Range(bounds.xMin, bounds.xMax);
            int y = UnityEngine.Random.Range(bounds.yMin, bounds.yMax);

            Vector3Int pos = new Vector3Int(x, y, 0);

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
        pathing.SetParameters(tilemap, start, end);
        return pathing.FindPath();
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}