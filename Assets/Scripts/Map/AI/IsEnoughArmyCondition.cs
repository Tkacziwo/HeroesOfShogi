using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "IsEnoughArmyArmy", story: "[self] has enough army size", category: "Conditions", id: "3b3ac7b2e70143223e46e9605f0d779e")]
public partial class IsEnoughArmyCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;

    public override bool IsTrue()
    {
        int armySize = 0;

        var units = Self.Value.GetComponent<NPCModel>().GetCurrentPlayerCharacter().AssignedUnits;

        foreach (var unit in units)
        {
            armySize += unit.SizeInArmy;
        }

        //[ToDo] move threshold to static or bot aggression
        if(armySize >= 15)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
