using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "RecruitUnits", story: "[Self] recruits units", category: "Action", id: "c83f82cb4c83288da8fd790f32038f8d")]
public partial class RecruitUnitsAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<City> city;
    [SerializeReference] public BlackboardVariable<PlayerCharacterController> playerInCity;

    protected override Status OnStart()
    {


        return Status.Success;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

