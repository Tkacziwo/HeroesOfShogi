using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "PathEstabilished", story: "[Self] has estabilished path", category: "Conditions", id: "d7f290f0abe16f43d2c1a0cdb863e81c")]
public partial class PathEstabilishedCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;

    public override bool IsTrue()
        => Self.Value.GetComponent<NPCModel>().RemainingPath.Count > 0;
}