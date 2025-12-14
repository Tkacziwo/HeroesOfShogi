using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "AttackPlayerAction", story: "[self] attacks player", category: "Action", id: "a63234a28cef3c567027c09575b042c0")]
public partial class AttackPlayerAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;

    [SerializeReference] public BlackboardVariable<Tilemap> tilemap;

    [SerializeReference] public BlackboardVariable<bool> result;

    private PathingController Pathing { get; set; } = new();

    protected override Status OnStart()
    {
        var character = Self.Value.GetComponent<NPCModel>().GetCurrentPlayerCharacter();
        var npc = Self.Value.GetComponent<NPCModel>();

        var players = PlayerRegistry.Instance.GetAllPlayers();

        if (players.Count == 0 || character == null) { Debug.LogError("something wrong with bot attack"); return Status.Failure; }

        float dist = float.MaxValue;
        PlayerCharacterController chosenCharacter = null;
        foreach (var player in players)
        {
            if (player.playerId == character.playerId && player.GetCurrentPlayerCharacter().characterId == character.characterId) continue;
            else
            {
                var playerCharacter = player.GetCurrentPlayerCharacter();
                var newDist = Vector3Int.Distance(playerCharacter.characterPosition, character.characterPosition);
                if (newDist < dist)
                {
                    dist = newDist;
                    chosenCharacter = playerCharacter;
                }
            }
        }

        if (chosenCharacter == null)
        {
            result.Value = false;
        }
        else
        {
            Pathing.SetParameters(tilemap.Value, character.characterPosition, chosenCharacter.characterPosition);
            var path = Pathing.FindPath();
            path.Add(new TileInfo()
            {
                position = chosenCharacter.characterPosition,
            });

            npc.RemainingPath = path;
            npc.ChosenBuilding = null;

            if (path != null && path.Count != 0)
            {
                result.Value = true;
            }
            else
            {
                result.Value = false;
            }
        }

        return Status.Success;
    }
}