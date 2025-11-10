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
    private PlayerCharacterController Player;

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