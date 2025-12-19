using System;
using System.Collections.Generic;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "ExecuteMoveAction", story: "[self] executes move", category: "Action", id: "e829de4ab1be4b7f302e5a042847bce4")]
public partial class ExecuteMoveAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;

    protected override Status OnStart()
    {
        var character = Self.Value.GetComponent<NPCModel>().GetCurrentPlayerCharacter();
        List<TileInfo> path = new(PathingResult.Instance.GetPath());
        Tuple<int, List<TileInfo>> botResults = new(character.characterId, path);


        PathingResult.Instance.ClearPath();
        //AIEvents.OnBotMove?.Invoke(botResults);

        return Status.Success;
    }
}