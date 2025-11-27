using System;
using System.Collections.Generic;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Tilemaps;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "MoveToCity", story: "[Self] moves to city", category: "Action", id: "a9cc5b9cd1d449f60b95bccdd526716c")]
public partial class MoveToCityAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<Tilemap> tilemap;

    [SerializeReference] public BlackboardVariable<int> result;

    [SerializeReference] public BlackboardVariable<bool> cityHasUnits;
    [SerializeReference] public BlackboardVariable<City> Chosencity;
    [SerializeReference] public BlackboardVariable<PlayerCharacterController> characterInCity;



    PathingController pathing = new();

    private Vector3Int bestEndPos;

    public static event Action<BotCaptureInfo> OnBotGoToCity;

    protected override Status OnStart()
    {
        var characters = Self.Value.GetComponent<PlayerModel>().GetPlayerCharacters();
        result.Value = 1;
        cityHasUnits.Value = false;
        foreach (var character in characters)
        {
            var city = FindCity(character.characterPosition, character.playerId);

            if (city == null)
            {
                result.Value = 0;
                return Status.Failure;
            }
            else
            {
                var path = FindPathToBuilding(city, character);

                BotCaptureInfo info = new()
                {
                    character = character,
                    chosenBuilding = city,
                    endPos = bestEndPos,
                    pathToBuilding = path
                };

                OnBotGoToCity?.Invoke(info);

                if(city.HasAvailableUnits())
                {
                    cityHasUnits.Value = true;
                    Chosencity.Value = city;
                }
            }
        }

        return Status.Success;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {

    }

    private City FindCity(Vector3Int startPos, int playerId)
    {
        var buildings = BuildingRegistry.Instance.GetAllBuildings();

        City chosenCity = null;
        float bestDist = float.MaxValue;
        foreach (var building in buildings)
        {
            if (building is City city && city.capturerId == playerId)
            {
                var pos = tilemap.Value.WorldToCell(city.transform.position);
                float dist = Vector3Int.Distance(startPos, pos);
                if (bestDist > dist)
                {
                    bestDist = dist;
                    chosenCity = city;
                }
            }
        }

        return chosenCity;
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
}