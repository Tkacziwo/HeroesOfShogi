using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "ReturnControlToPlayerController", story: "[self] return control to controller", category: "Action", id: "67f919479f0e8d3ed8f8647e8cc08f22")]
public partial class ReturnControlToPlayerControllerAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;

    public static Action<int> OnReturnControlToController;

    protected override Status OnStart()
    {
        OnReturnControlToController?.Invoke(Self.Value.GetComponent<NPCModel>().playerId);
        return Status.Success;
    }
}

