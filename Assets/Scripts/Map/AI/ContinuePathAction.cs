using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using System.Collections.Generic;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "ContinuePath", story: "[Self] continues on its path", category: "Action", id: "581c28e9572bb685a9ad17754f543e54")]
public partial class ContinuePathAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;

    public static Action<List<TileInfo>> OnNPCContinuePath;

    protected override Status OnStart()
    {
        var npc = Self.Value.GetComponent<NPCModel>();
        var remainingPath = Self.Value.GetComponent<NPCModel>().RemainingPath;

        int remainingMovementPoints = Self.Value.GetComponent<NPCModel>().GetCurrentPlayerCharacter().GetRemainingMovementPoints();

        List<TileInfo> traversedPath;

        if (remainingPath.Count <= remainingMovementPoints)
        {
            traversedPath = new(remainingPath);
            npc.ReachedDestination = true;
            OnNPCContinuePath?.Invoke(traversedPath);
            remainingPath.Clear();
        }
        else
        {
            traversedPath = new(remainingPath.GetRange(0, remainingMovementPoints));
            npc.ReachedDestination = false;
            OnNPCContinuePath?.Invoke(traversedPath);

            remainingPath.RemoveRange(0, remainingMovementPoints);
        }
        return Status.Success;
    }
}