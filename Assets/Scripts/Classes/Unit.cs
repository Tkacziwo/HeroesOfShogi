using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.PlayerSettings;

/// <summary>
/// Unit class symbolizes one unit during battle. 
/// Extension of LogicPiece with additional HealthPoints, AttackPower, SpecialAbilitiesList, ArmorPenetrationModifier.
/// Prepared to be cloned and copied for use withing MinMaxAlgorithm.
/// </summary>
public class Unit : LogicPiece
{
    public UnitEnum UnitName { get; set; }

    public int HealthPoints { get; set; }

    private int maxHealthPoints;

    public int AttackPower { get; set; }

    public List<string> SpecialAbilities { get; set; }

    public int SizeInArmy { get; set; }

    public Sprite UnitSprite { get; set; }

    public static Action OnDeath;

    public bool MovedInTurn { get; set; } = false;

    public void InitUnit(Unit template)
    {
        this.UnitName = template.UnitName;
        this.HealthPoints = template.HealthPoints;
        this.maxHealthPoints = template.HealthPoints;
        this.AttackPower = template.AttackPower;
    }

    public Unit()
    {
    }

    public Unit(Unit other)
    {
        UnitName = other.UnitName;
        HealthPoints = other.HealthPoints;
        AttackPower = other.AttackPower;
        SpecialAbilities = other.SpecialAbilities;
        SizeInArmy = other.SizeInArmy;
        UnitSprite = other.UnitSprite;
        MovedInTurn = other.MovedInTurn;
        value = other.pieceName switch
        {
            "Pawn" => 100,
            "GoldGeneral" => 500,
            "SilverGeneral" => 400,
            "Bishop" => 800,
            "Rook" => 800,
            "Lance" => 400,
            "Horse" => 300,
            "King" => 10000,
            _ => 0,
        };

        isKing = other.isKing;
        originalPieceName = pieceName = other.pieceName;
        Moveset = other.Moveset;
        originalMoveset = other.originalMoveset;
        pos = other.pos;
        isSpecial = other.isSpecial;
        isPromoted = other.isPromoted;
        isDrop = other.isDrop;
        isBlack = other.isBlack;
    }
    public void RestoreMaxHP()
        => HealthPoints = maxHealthPoints;

    public bool ReduceHP(int hp)
    {
        HealthPoints -= hp;

        if(HealthPoints <= 0)
        {
            return true;
        }

        return false;
    }
}