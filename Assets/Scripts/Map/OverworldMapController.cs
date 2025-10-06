using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class OverworldMapController : MonoBehaviour
{
    private List<TileInfo> pathfindingResult = new();

    private readonly PathingController pathingController = new();

    [SerializeField]
    private Tilemap tilemap;

    //[SerializeField]
    //private TileBase hoverTile;

    [SerializeField]
    private TileBase StartTile;

    [SerializeField]
    private TileBase EndTile;

    [SerializeField]
    private TileBase PathResultTile;

    [SerializeField]
    private TileBase PathTile;

    [SerializeField]
    private GameObject playerController;

    public static Action onTurnEnd;

    public List<IBuilding> worldBuildings;

    private void OnEnable()
    {
        PlayerCharacterController.OnPlayerOverTile += ClearTile;
        PlayerController.onPlayerBeginMove += MovePlayer;
    }

    private void OnDisable()
    {
        PlayerCharacterController.OnPlayerOverTile -= ClearTile;
        PlayerController.onPlayerBeginMove -= MovePlayer;
    }

    void Start()
    {
        // [ToDo] load buldings positions and instantiate them on the map




        var playerControllerScript = playerController.GetComponent<PlayerController>();
        playerControllerScript.SpawnPlayer();
        var playerStartingPosition = playerControllerScript.GetPlayerPosition();
        var vec = new Vector3Int((int)playerStartingPosition.x, (int)playerStartingPosition.y, (int)playerStartingPosition.z);
        playerControllerScript.SetCharacterPosition(vec);
        previousStartPos = playerControllerScript.GetCharacterPosition();
        var t = tilemap.GetTile<MapTile>(vec);
        SetStartPoint(vec, t);
    }

    private void DisplayPath(List<TileInfo> closedList)
    {
        foreach (var item in closedList)
        {
            var pos = item.position;
            tilemap.SetTile(pos, PathResultTile);
        }
    }

    private MapTile start;

    private TileBase previousStart;

    private Vector3Int previousStartPos;

    private MapTile end;

    private TileBase previousEnd;

    private Vector3Int previousEndPos;

    private TileBase previousTile;

    private Vector3Int previousTilePos;

    // Update is called once per frame
    bool isBlocked = false;
    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        Plane ground = new(Vector3.up, Vector3.zero);

        if (ground.Raycast(ray, out float enter) && !isBlocked)
        {
            Vector3 point = ray.GetPoint(enter);

            Vector3Int cellPos = tilemap.WorldToCell(point);

            var t = tilemap.GetTile<MapTile>(cellPos);
            if (t != null)
            {
                if (t.IsTraversable)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        isBlocked = true;
                        ClearTiles();
                        SetEndPoint(cellPos, t);
                        FindPath();
                        isBlocked = false;
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

        // Moves player from one location to another
        //if (Input.GetKeyUp(KeyCode.Space))
        //{
        //    MovePlayer();
        //}

        if (Input.GetKeyUp(KeyCode.K))
        {
            ClearTiles();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            start = tilemap.GetTile<MapTile>(previousStartPos);
            end = tilemap.GetTile<MapTile>(previousEndPos);

            pathingController.SetParameters(tilemap, previousStartPos, previousEndPos);
            pathfindingResult = pathingController.FindPath();

            DisplayPath(pathfindingResult);

            float elapsed = watch.ElapsedMilliseconds * 0.001f;
            Debug.Log($"Elapsed: {elapsed} seconds");
            watch.Stop();
        }
        if (Input.GetKeyUp(KeyCode.L))
        {

            foreach (var item in pathfindingResult)
            {
                tilemap.SetTile(item.position, PathTile);
            }

            pathfindingResult.Clear();
        }
    }

    public void MovePlayer(PlayerController controller)
    {
        List<Vector3> convertedPath = new();
        List<Vector3Int> tilesPositions = new();
        foreach (var item in pathfindingResult)
        {
            var tileWorldCenterPosition = tilemap.GetCellCenterWorld(item.position);
            convertedPath.Add(tileWorldCenterPosition);

            tilesPositions.Add(item.position);
        }

        convertedPath.Reverse();
        tilesPositions.Reverse();
        convertedPath.Add(tilemap.GetCellCenterWorld(previousEndPos));
        tilesPositions.Add(previousEndPos);

        playerController.GetComponent<PlayerController>().SetCharacterPath(convertedPath, tilesPositions);

        ReplaceStartWithEnd();
    }


    public void ClearTile(Vector3Int tilePosition)
    {
        tilemap.SetTile(tilePosition, PathTile);
    }

    private void FindPath()
    {
        var watch = System.Diagnostics.Stopwatch.StartNew();
        start = tilemap.GetTile<MapTile>(previousStartPos);
        end = tilemap.GetTile<MapTile>(previousEndPos);

        pathingController.SetParameters(tilemap, previousStartPos, previousEndPos);
        pathfindingResult = pathingController.FindPath();

        DisplayPath(pathfindingResult);

        float elapsed = watch.ElapsedMilliseconds * 0.001f;
        Debug.Log($"Elapsed: {elapsed} seconds");
        watch.Stop();
    }


    private void ReplaceStartWithEnd()
    {
        if (previousEnd != null)
        {
            var script = tilemap.GetTile<MapTile>(previousEndPos);
            script.IsEnd = false; script.IsStart = false;
            tilemap.SetTile(previousEndPos, previousEnd);
        }
        var t = tilemap.GetTile<MapTile>(previousEndPos);
        t.IsStart = true; t.IsEnd = false;
        previousEnd = null;
        SetStartPoint(previousEndPos, t);
    }

    private void SetEndPoint(Vector3Int cellPos, MapTile t)
    {
        if (previousEnd != null)
        {
            var script = tilemap.GetTile<MapTile>(previousEndPos);
            script.IsEnd = false; script.IsStart = false;
            tilemap.SetTile(previousEndPos, previousEnd);
        }

        t.IsStart = false; t.IsEnd = true;
        tilemap.SetTile(cellPos, EndTile);
        previousEnd = t;
        previousEndPos = cellPos;
    }

    private void SetStartPoint(Vector3Int cellPos, MapTile t)
    {
        if (previousStart != null)
        {
            var script = tilemap.GetTile<MapTile>(previousStartPos);
            script.IsStart = false; script.IsEnd = false;
            tilemap.SetTile(previousStartPos, previousStart);

        }

        t.IsStart = true; t.IsEnd = false;
        tilemap.SetTile(cellPos, StartTile);
        previousStart = t;
        previousStartPos = cellPos;
    }

    private void ClearTiles()
    {
        foreach (var item in pathfindingResult)
        {
            tilemap.SetTile(item.position, PathTile);
        }

        pathfindingResult.Clear();
    }

    private void EndTurn()
    {
        onTurnEnd?.Invoke();
    }

    public MapTile MouseOverTile()
    {

        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        var hit = Physics.Raycast(ray, out RaycastHit info);
        return hit ? info.transform.GetComponent<MapTile>() : null;
    }
}