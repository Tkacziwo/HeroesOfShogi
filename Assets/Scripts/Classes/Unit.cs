using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Unit class symbolizes one unit during battle. 
/// Extension of LogicPiece with additional HealthPoints, AttackPower, SpecialAbilitiesList, ArmorPenetrationModifier.
/// Prepared to be cloned and copied for use withing MinMaxAlgorithm.
/// 
/// <param name="ArmorPenetrationModifier">Bypasses armor if the value is higher than attacked unit's ArmorPower resulting in dealing full damage. 
/// If attacked unit's ArmorPower is greater than attacking unit PenetrationModifier + AttackPower the attacked unit sustains fraction of the damage</param>"
/// <param name="ArmorPower">Reduces incoming damage. If AttackPower + PenetrationModifier is greater than attacked unit ArmorPower, the unit sustains full damage. Else it sustains fraction of the damage</param>
/// </summary>
public class Unit : LogicPiece
{
    public UnitEnum UnitName { get; set; }

    public float HealthPoints { get; set; }

    public int AttackPower { get; set; }

    public int ArmorPower { get; set; }

    public List<string> SpecialAbilities { get; set; }

    public int ArmorPenetrationModifier { get; set; }

    public int SizeInArmy { get; set; }

    public Sprite UnitSprite { get; set; }

    //[ToDo] Finish the methods
    public void Clone()
    {

    }

    public void ReduceHP()
    {

    }

    public void IncreaseHP()
    {

    }

    public void SetHP()
    {

    }

    public void ReduceArmor()
    {

    }

    public void IncreaseArmor()
    {

    }

    public void SetArmor()
    {

    }

    public void ReduceAttackPower(int val)
    {

    }

    public void IncreaseAttackPower(int val)
    {

    }

    public void SetAttackPower(int val)
    {

    }

    public void ReducePenetrationModifier()
    {

    }

    public void IncreasePenetrationModifier()
    {

    }

    public void SetPenetrationModifier()
    {

    }
}