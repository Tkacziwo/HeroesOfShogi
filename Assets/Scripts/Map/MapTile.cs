using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

#if UNITY_EDITOR
using UnityEditor;
#endif
public class MapTile : Tile
{
    public override void RefreshTile(Vector3Int position, ITilemap tilemap)
    {
        base.RefreshTile(position, tilemap);
    }

    public override void GetTileData(Vector3Int position, ITilemap tilemap, ref TileData tileData)
    {
        tileData.sprite = sprite;
    }

#if UNITY_EDITOR
    [MenuItem("Assets/Create/2D/Custom Tiles/MapTile")]
    public static void CreateMapTile()
    {
        string path = EditorUtility.SaveFilePanelInProject("Save MapTile", "New MapTile", "Asset", "Save MapTile", "Assets");
        if (path == "") return;

        AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<MapTile>(), path);

    }
#endif
    [SerializeField]
    public Sprite sprite;

    [SerializeField]
    public Position TilePosition;

    public Vector3Int tilePosition;

    [SerializeField]
    private PlayerCharacter Player;

    [SerializeField]
    private float size = 0.5f;

    [SerializeField]
    public bool IsTraversable;

    [SerializeField]
    public bool IsStart;

    [SerializeField]
    public bool IsEnd;

    [SerializeField]
    public bool IsPath;

    public float gCost;

    public float hCost;

    public float fCost;

    public MapTile parent;

    public void CalculateGCost(float previousGCost, bool isDiagonal)
    {
        float add = isDiagonal ? 1.4f : 1;
        gCost = previousGCost + add;
    }

    public void CalculateHCost(Position endPos)
    {
        hCost = Mathf.Sqrt(Mathf.Pow(Mathf.Abs(this.TilePosition.x - endPos.x), 2) + Mathf.Pow(Mathf.Abs(this.TilePosition.y - endPos.y), 2));
    }

    public void CalculateFCost()
    {
        fCost = gCost + hCost;
    }


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    //public void SetPosition(Position pos)
    //    => this.TilePosition = pos;

    public void SetNonTraversable()
    {
        IsTraversable = false;
    }

    public bool GetIsTraversable()
        => IsTraversable;

    public Position GetPosition()
        => this.TilePosition.GetPosition();
}


public class PlayerCharacter
{
    [SerializeField]
    private Position PlayerPosition;

    [SerializeField]
    private string Name;

    [SerializeField]
    private double MovementPoints;

    private List<Unit> AssignedUnits { get; set; }

    public void SetPlayerPosition(Position newPos)
    {
        PlayerPosition = new(newPos);
    }
}

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
    public float HealthPoints { get; set; }

    public int AttackPower { get; set; }

    public int ArmorPower { get; set; }

    public List<string> SpecialAbilities { get; set; }

    public int ArmorPenetrationModifier { get; set; }

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