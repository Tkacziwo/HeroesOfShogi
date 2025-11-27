using System;
using System.Collections.Generic;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "CaptureBuilding", story: "[Self] captures building", category: "Action", id: "75b73f5e4bfc3fcb2c075ecf4402cb47")]
public partial class CaptureBuildingAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<Tilemap> tilemap;
    [SerializeReference] public BlackboardVariable<int> result;
    [SerializeReference] public BlackboardVariable<bool> wentToCity;


    private readonly PathingController pathing = new();

    public static event Action<BotCaptureInfo> OnBotCapture;

    private Vector3Int bestEndPos;

    protected override Status OnStart()
    {
        var character = Self.Value.GetComponent<PlayerModel>().GetCurrentPlayerCharacter();
        result.Value = 1;
        List<Tuple<int, List<TileInfo>>> botResults = new();

        var chosenBuilding = FindNearestBuilding(character.characterPosition, character.playerId);

        if (chosenBuilding != null)
        {
            if (character.unreachedBuilding != null)
            {
                chosenBuilding = character.unreachedBuilding;
                character.unreachedBuilding = null;
                character.unreachedBotDestination = new Vector3Int(0, 0, 0);
            }


            var path = FindPathToBuilding(chosenBuilding, character);

            if (path.Count > character.GetRemainingMovementPoints())
            {
                character.unreachedBuilding = chosenBuilding;
                character.unreachedBotDestination = new Vector3Int(0, 0, 0);
            }
            else
            {
                character.unreachedBuilding = null;
            }

            character.unreachedBuilding = null;

            BotCaptureInfo botCaptureInfo = new()
            {
                character = character,
                chosenBuilding = chosenBuilding,
                pathToBuilding = path,
                endPos = bestEndPos
            };

            OnBotCapture?.Invoke(botCaptureInfo);
            return Status.Success;
        }
        else
        {
            result.Value = 0;
        }

        return Status.Success;
    }

    private InteractibleBuilding FindNearestBuilding(Vector3Int startPos, int playerId)
    {
        var buildings = BuildingRegistry.Instance.GetAllBuildings();
        float bestDist = float.MaxValue;
        InteractibleBuilding bestBuilding = null;
        foreach (var building in buildings)
        {
            if (building.capturerId == playerId) continue;

            var pos = tilemap.Value.WorldToCell(building.transform.position);

            float dist = Vector3Int.Distance(startPos, pos);
            if (dist < bestDist)
            {
                bestDist = dist;
                bestBuilding = building;
            }
        }
        return bestBuilding;
    }

    public List<TileInfo> FindPathToBuilding(InteractibleBuilding building, PlayerCharacterController currentCharacter)
    {
        List<Vector3Int> traversableTiles = new();

        var colliderBounds = building.GetComponent<BoxCollider>().bounds;

        for (int y = (int)colliderBounds.min.z; y < colliderBounds.max.z; y++)
        {
            for (int x = (int)colliderBounds.min.x; x < colliderBounds.max.x; x++)
            {
                for (int cellY = -1; cellY <= 1; cellY++)
                {
                    for (int cellX = -1; cellX <= 1; cellX++)
                    {
                        int destX = cellX + x;
                        int destY = cellY + y;

                        Vector3Int destPos = new(destX, destY, 0);

                        var tile = tilemap.Value.GetTile<MapTile>(destPos);

                        if (tile != null)
                        {
                            if (!traversableTiles.Contains(destPos) && tile.IsTraversable)
                            {
                                traversableTiles.Add(destPos);
                            }
                        }
                    }
                }
            }
        }


        List<TileInfo> bestPath = new();
        Vector3Int bestEndPosition = new(0, 0, 0);

        foreach (var item in traversableTiles)
        {
            pathing.SetParameters(tilemap, currentCharacter.characterPosition, item);
            var path = pathing.FindPath();

            if (bestPath.Count == 0 || bestPath.Count > path.Count)
            {
                bestEndPosition = item;
                bestPath = new(path);
            }
        }

        bestEndPos = bestEndPosition;
        return bestPath;
    }


    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}



public class BotCaptureInfo
{
    public PlayerCharacterController character;

    public InteractibleBuilding chosenBuilding;

    public List<TileInfo> pathToBuilding;

    public Vector3Int endPos;
}