using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEditor.PlayerSettings;

public class OverworldMapController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField]
    private int MapHeight;

    [SerializeField]
    private int MapWidth;

    private List<PlayerCharacter> Players = new();

    [SerializeField]
    private MapTile MapTilePrefab;

    public int RandomThrows;

    public List<TileInfo> closedList = new();

    public PathingController pathingController = new();

    [SerializeField]
    public Tilemap newGrid;

    public TileBase tile;

    [SerializeField]
    private TileBase StartTile;

    [SerializeField]
    private TileBase EndTile;

    [SerializeField]
    private TileBase PathResultTile;

    [SerializeField]
    private TileBase PathTile;

    public Camera camera;


    void Start()
    {
        PlayerCharacter character = new();
        character.SetPlayerPosition(new(0, 0));
        Players.Add(character);
    }

    private void DisplayPath(List<TileInfo> closedList)
    {
        foreach (var item in closedList)
        {
            var pos = item.position;
            newGrid.SetTile(pos, PathResultTile);
        }
    }

    public Vector3Int oldMousePos = new();

    private MapTile start;

    private TileBase previousStart;

    private Vector3Int previousStartPos;

    private MapTile end;

    private TileBase previousEnd;

    private Vector3Int previousEndPos;

    private TileBase previousTile;

    private Vector3Int previousTilePos;

    // Update is called once per frame
    void Update()
    {
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);

        Plane ground = new Plane(Vector3.up, Vector3.zero);

        if (ground.Raycast(ray, out float enter))
        {
            Vector3 point = ray.GetPoint(enter);

            Vector3Int cellPos = newGrid.WorldToCell(point);

            var t = newGrid.GetTile<MapTile>(cellPos);
            if (t != null)
            {
                if (t.IsTraversable && !t.IsPath)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        if (previousStart != null)
                        {
                            var script = newGrid.GetTile<MapTile>(previousStartPos);
                            script.IsStart = false; script.IsEnd = false;
                            newGrid.SetTile(previousStartPos, previousStart);

                        }

                        t.IsStart = true; t.IsEnd = false;
                        newGrid.SetTile(cellPos, StartTile);
                        previousStart = t;
                        previousStartPos = cellPos;
                    }
                    else if (Input.GetMouseButtonDown(1))
                    {
                        if (previousEnd != null)
                        {
                            var script = newGrid.GetTile<MapTile>(previousEndPos);
                            script.IsEnd = false; script.IsStart = false;
                            newGrid.SetTile(previousEndPos, previousEnd);
                        }

                        t.IsStart = false; t.IsEnd = true;
                        newGrid.SetTile(cellPos, EndTile);
                        previousEnd = t;
                        previousEndPos = cellPos;
                    }

                    if (previousTilePos != cellPos)
                    {
                        //newGrid.SetTile(previousTilePos, previousTile);
                        //previousTile = newGrid.GetTile(cellPos);
                        //newGrid.SetTile(cellPos, tile);
                        previousTilePos = cellPos;
                    }

                }
            }
        }
    
        if (Input.GetKeyUp(KeyCode.K))
        {
            ClearTiles();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            start = newGrid.GetTile<MapTile>(previousStartPos);
            end = newGrid.GetTile<MapTile>(previousEndPos);

            pathingController.SetParameters(newGrid, previousStartPos, previousEndPos);
            closedList = pathingController.FindPath();

            DisplayPath(closedList);

            float elapsed = watch.ElapsedMilliseconds * 0.001f;
            Debug.Log($"Elapsed: {elapsed} seconds");
            watch.Stop();
        }
        if (Input.GetKeyUp(KeyCode.L))
        {

            foreach (var item in closedList)
            {
                newGrid.SetTile(item.position, PathTile);
            }
            
            closedList.Clear();
        }
    }

    private void ClearTiles()
    {
        foreach (var item in closedList)
        {
            newGrid.SetTile(item.position, PathTile);
        }

        closedList.Clear();
    }

    public MapTile MouseOverTile()
    {

        var ray = camera.ScreenPointToRay(Input.mousePosition);
        var hit = Physics.Raycast(ray, out RaycastHit info);
        return hit ? info.transform.GetComponent<MapTile>() : null;
    }
}