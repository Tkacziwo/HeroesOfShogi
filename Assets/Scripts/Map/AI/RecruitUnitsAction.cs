using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32.SafeHandles;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "RecruitUnits", story: "[Self] recruits units", category: "Action", id: "c83f82cb4c83288da8fd790f32038f8d")]
public partial class RecruitUnitsAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<City> city;
    [SerializeReference] public BlackboardVariable<PlayerCharacterController> playerInCity;

    private readonly List<Unit> unitTemplates = StaticData.unitTemplates;

    protected override Status OnStart()
    {
        var units = playerInCity.Value.AssignedUnits;

        var unitsDict = GetUnitsDictionary(units);

        foreach (var type in Enum.GetValues(typeof(UnitEnum)).Cast<UnitEnum>())
        {
            if (!unitsDict.ContainsKey(type))
                unitsDict[type] = 0;
        }

        var availableUnits = GetAvailableUnitsDictionary(city.Value.producedUnits);

        var size = CountUnits(units);

        Dictionary<UnitEnum, int> unitMaxLimits = new()
        {
            {UnitEnum.Pawn, 9 },
            {UnitEnum.Lance, 2 },
            {UnitEnum.Horse, 2 },
            {UnitEnum.GoldGeneral, 2 },
            {UnitEnum.SilverGeneral, 2 },
            {UnitEnum.Bishop, 1 },
            {UnitEnum.Rook, 1 },
        };

        foreach (var unitType in unitMaxLimits)
        {
            var newAmount = RecruitUnits(availableUnits[unitType.Key], unitsDict[unitType.Key], unitMaxLimits[unitType.Key], playerInCity.Value.armySizeLimit - size);

            int oldAmount = unitsDict[unitType.Key];

            unitsDict[unitType.Key] = newAmount;
            availableUnits[unitType.Key] -= (newAmount - oldAmount);

            size += (newAmount - oldAmount) * unitTemplates.Single(o => o.UnitName == unitType.Key).SizeInArmy;
        }

        playerInCity.Value.AssignedUnits.Clear();

        foreach (var unitType in unitsDict)
        {
            playerInCity.Value.AssignedUnits.Add(unitTemplates.Single(o => o.UnitName == UnitEnum.King));
            var template = unitTemplates.Single(o => o.UnitName == unitType.Key);

            for (int i = 0; i < unitType.Value; i++)
            {
                playerInCity.Value.AssignedUnits.Add(template);
            }

        }



        return Status.Success;
    }

    private int CountUnits(List<Unit> units)
    {
        int sum = 0;

        foreach (var unit in units)
        {
            sum += unit.SizeInArmy;
        }

        return sum;
    }



    private int RecruitUnits(int availableUnits, int assignedUnits, int unitLimit, int armyLimit)
    {
        if (assignedUnits < unitLimit)
        {
            var sizeToRecruit = unitLimit - assignedUnits;

            var amountToRecruit = Mathf.Min(availableUnits, sizeToRecruit, armyLimit);

            assignedUnits += amountToRecruit;

        }
        return assignedUnits;
    }

    private Dictionary<UnitEnum, int> GetUnitsDictionary(List<Unit> assignedUnits)
    {
        Dictionary<UnitEnum, int> dict = new();
        foreach (var unit in assignedUnits)
        {
            if (dict.ContainsKey(unit.UnitName))
            {
                dict[unit.UnitName] += 1;
            }
            else
            {
                dict.Add(unit.UnitName, 1);
            }
        }
        return dict;
    }

    private Dictionary<UnitEnum, int> GetAvailableUnitsDictionary(ProducedUnits units)
    {
        Dictionary<UnitEnum, int> dict = new()
        {
            { UnitEnum.Pawn, units.pawns },
            { UnitEnum.Lance, units.lances },
            { UnitEnum.Horse, units.horses },
            { UnitEnum.GoldGeneral, units.goldGenerals },
            { UnitEnum.SilverGeneral, units.silverGenerals },
            { UnitEnum.Rook, units.rooks },
            { UnitEnum.Bishop, units.bishops }
        };

        return dict;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}

