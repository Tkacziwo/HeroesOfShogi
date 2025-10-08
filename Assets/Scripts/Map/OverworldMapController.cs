using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using static UnityEditor.Progress;

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

    public static Action onTurnEnd;

    public List<InteractibleBuilding> worldBuildings;

    private InteractibleBuilding chosenWorldBuilding;

    [SerializeField] private PlayerController playerController;

    private void OnEnable()
    {
        PlayerCharacterController.OnPlayerOverTile += ClearTile;
        BuildingEvents.onBuildingClicked += FindPathToBuilding;
        PlayerEvents.OnPlayerBeginMove += MovePlayer;
    }

    private void OnDisable()
    {
        PlayerCharacterController.OnPlayerOverTile -= ClearTile;
        BuildingEvents.onBuildingClicked -= FindPathToBuilding;
        PlayerEvents.OnPlayerBeginMove -= MovePlayer;
    }

    public void FindPathToBuilding(InteractibleBuilding building)
    {
        //[ToDo] Handle doubleclicking
        //finding neighbours of tiles;
        List<Vector3Int> traversableTiles = new();

        var colliderBounds = building.GetComponent<BoxCollider>().bounds;

        for (int y = (int)colliderBounds.min.z; y < colliderBounds.max.z; y++)
        {
            for (int x = (int)colliderBounds.min.x; x < colliderBounds.max.x; x++)
            {
                for (int cellY = -1; cellY <= 1; cellY++)
                {
                    for (int cellX = -1; cellX <= 1; cellX++)
                    {
                        int destX = cellX + x;
                        int destY = cellY + y;

                        Vector3Int destPos = new(destX, destY, 0);

                        var tile = tilemap.GetTile<MapTile>(destPos);
                        if (tile != null && !traversableTiles.Contains(destPos) && tile.IsTraversable)
                        {
                            traversableTiles.Add(destPos);
                        }
                    }
                }
            }
        }

        List<TileInfo> bestPath = new();
        Vector3Int bestEndPosition = new(0, 0, 0);

        foreach (var item in traversableTiles)
        {
            pathingController.SetParameters(tilemap, previousStartPos, item);
            var path = pathingController.FindPath();

            if (bestPath.Count == 0 || bestPath.Count > path.Count)
            {
                bestEndPosition = item;
                bestPath = new(path);
            }
        }

        var t = tilemap.GetTile<MapTile>(bestEndPosition);
        SetEndPoint(bestEndPosition, t);
        pathfindingResult = bestPath;
        DisplayPath(pathfindingResult);
        chosenWorldBuilding = building;
    }

    void Start()
    {
        //var width = tilemap.cellBounds.xMax;
        //var height = tilemap.cellBounds.yMax;
        //for (int y = 0; y < height; y++)
        //{
        //    for (int x = 0; x < width; x++)
        //    {
                //Vector3Int v = new(x, y, 0);
                //var tile = tilemap.GetTile<GameObject>(v);
                //tile.GetCompoentn.SetActive(false);
        //    }
        //}
        // [ToDo] load buldings positions and instantiate them on the map

        // [ToDo] load players and place them on the map

        var playerControllerScript = playerController.GetComponent<PlayerController>();
        playerControllerScript.player.SpawnPlayer(100);
        var playerStartingPosition = playerControllerScript.player.GetPlayerPosition();
        var vec = new Vector3Int((int)playerStartingPosition.x, (int)playerStartingPosition.y, (int)playerStartingPosition.z);
        playerControllerScript.player.SetCharacterPosition(vec);
        previousStartPos = playerControllerScript.player.GetCharacterPosition();
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

        if (Input.GetKeyDown(KeyCode.Return))
        {
            EndTurn();
        }
    }

    public void MovePlayer(PlayerModel controller)
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

        playerController.player.GetComponent<PlayerModel>().SetCharacterPath(convertedPath, tilesPositions);

        ReplaceStartWithEnd();

        if (chosenWorldBuilding != null)
        {
            chosenWorldBuilding.CaptureBuilding(controller.playerId, controller.playerColor);
            chosenWorldBuilding = null;
        }
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
        chosenWorldBuilding = null;
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