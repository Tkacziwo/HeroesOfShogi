using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.XR;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;
using static UnityEditor.Progress;

public class OverworldMapController : MonoBehaviour
{
    private List<TileInfo> pathfindingResult = new();

    private readonly PathingController pathingController = new();

    [SerializeField] Camera topCamera;

    [SerializeField]
    private Tilemap tilemap;

    [SerializeField]
    private TileBase StartTile;

    [SerializeField]
    private TileBase EndTile;

    [SerializeField]
    private TileBase PathResultTile;

    [SerializeField]
    private TileBase UnreachableTile;

    [SerializeField]
    private TileBase PathTile;

    public static Action onTurnEnd;

    public List<InteractibleBuilding> worldBuildings;

    private InteractibleBuilding chosenWorldBuilding;

    private bool isPlayerMoving;

    [SerializeField] private PlayerController playerController;

    private Camera currentCamera;

    [SerializeField] private ResourceUIController resourceUIController;

    private PlayerCharacterController currentCharacter;

    private void OnEnable()
    {
        PlayerCharacterController.OnPlayerOverTile += ClearTile;
        BuildingEvents.onBuildingClicked += FindPathToBuilding;
        PlayerEvents.OnPlayerBeginMove += MovePlayer;
        PlayerEvents.OnPlayerEndMove += HandlePlayerEndMove;
        PlayerController.TurnEnded += HandleUpdateUIResources;
        PlayerController.PlayerCharacterChanged += HandlePlayerCharacterChanged;
        PlayerController.CameraChanged += HandleCameraChanged;
    }

    private void OnDisable()
    {
        PlayerCharacterController.OnPlayerOverTile -= ClearTile;
        BuildingEvents.onBuildingClicked -= FindPathToBuilding;
        PlayerEvents.OnPlayerBeginMove -= MovePlayer;
        PlayerEvents.OnPlayerEndMove -= HandlePlayerEndMove;
        PlayerController.TurnEnded -= HandleUpdateUIResources;
        PlayerController.PlayerCharacterChanged -= HandlePlayerCharacterChanged;
        PlayerController.CameraChanged -= HandleCameraChanged;
    }

    private void HandlePlayerCharacterChanged(Tuple<Vector3Int, Vector3Int> positions)
    {
        tilemap.SetTile(previousStartPos, PathTile);
        previousStartPos = positions.Item1;
        var vec = positions.Item2;
        var t = tilemap.GetTile<MapTile>(vec);
        SetStartPoint(vec, t);
    }

    private void HandleCameraChanged(Camera changedCamera)
    {
        topCamera.enabled = !topCamera.enabled;
        currentCamera = topCamera.enabled ? topCamera : changedCamera;
    }

    private void HandleUpdateUIResources()
    {
        resourceUIController.UpdateResourcesUI(playerController.player.playerResources);

        resourceUIController.IncrementTurnNumber();
    }

    private void HandlePlayerEndMove(PlayerCharacterController character)
    {
        var currentPosition = character.GetTargetPosition();
        var converted = new Vector3Int((int)currentPosition.x, (int)currentPosition.z, (int)currentPosition.y);
        if (converted == previousEndPos)
        {
            ReplaceStartWithEnd();
            character.SetPlayerPosition(previousStartPos);

            if (chosenWorldBuilding != null)
            {
                chosenWorldBuilding.CaptureBuilding(character.playerId, character.playerColor);
                chosenWorldBuilding = null;
            }

            pathfindingResult.Clear();
            character.ClearPath();
        }
        else
        {
            Vector3Int cellPos = tilemap.WorldToCell(currentPosition);
            var t = tilemap.GetTile<MapTile>(cellPos);
            SetStartPoint(cellPos, t);
        }

        isPlayerMoving = false;
    }

    public void FindPathToBuilding(InteractibleBuilding building)
    {
        //finding neighbours of tiles;
        if (chosenWorldBuilding == building) return;
        if (pathfindingResult.Count > 0) pathfindingResult.Clear();

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
        playerController.SpawnRealPlayer(100);

        var playerCamera = playerController.player.GetCameraController().GetComponentInChildren<Camera>();
        playerCamera.enabled = false;
        currentCamera = topCamera;

        var playerStartingPosition = playerController.player.GetPlayerPositionById(1);
        var vec = new Vector3Int((int)playerStartingPosition.x, (int)playerStartingPosition.y, (int)playerStartingPosition.z);
        playerController.player.SetCharacterPosition(vec, 1);

        var player2StartingPosition = playerController.player.GetPlayerPositionById(2);
        var vec2 = new Vector3Int((int)player2StartingPosition.x, (int)player2StartingPosition.y, (int)player2StartingPosition.z);
        playerController.player.SetCharacterPosition(vec2, 2);

        previousStartPos = playerController.player.GetCharacterPosition(1);
        var t = tilemap.GetTile<MapTile>(vec);
        SetStartPoint(vec, t);

        currentCharacter = playerController.GetCurrentPlayerCharacter();
    }

    private void DisplayPath(List<TileInfo> closedList)
    {
        if (closedList.Count <= 0) return;

        var remainingMovementPoints = currentCharacter.GetRemainingMovementPoints();

        if (remainingMovementPoints < 0) return;

        if (remainingMovementPoints >= closedList.Count)
        {
            foreach (var item in closedList)
            {
                var pos = item.position;
                tilemap.SetTile(pos, PathResultTile);
            }
        }
        else
        {
            for (int i = 0; i < remainingMovementPoints; i++)
            {
                var pos = closedList[i].position;
                tilemap.SetTile(pos, PathResultTile);
            }

            for (int i = remainingMovementPoints; i < closedList.Count; i++)
            {
                var pos = closedList[i].position;
                tilemap.SetTile(pos, UnreachableTile);
            }
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
    void Update()
    {
        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);

        Plane ground = new(Vector3.up, Vector3.zero);

        if (!EventSystem.current.IsPointerOverGameObject() && ground.Raycast(ray, out float enter) && !isPlayerMoving)
        {
            Vector3 point = ray.GetPoint(enter);

            Vector3Int cellPos = tilemap.WorldToCell(point);

            var t = tilemap.GetTile<MapTile>(cellPos);
            if (t != null && Input.GetMouseButtonDown(0) && t.IsTraversable)
            {
                ClearTiles();
                SetEndPoint(cellPos, t);
                FindPath();
            }
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            EndTurn();
        }
    }

    public void MovePlayer(PlayerModel controller)
    {
        //if(pathfindingResult.Count > 0)
        //{
        //    pathfindingResult.Clear();
        //}

        var currentCharacter = controller.GetCurrentPlayerCharacter();

        var remainingMovementPoints = currentCharacter.GetRemainingMovementPoints();

        List<Vector3> convertedPath = new();
        List<Vector3Int> tilesPositions = new();

        if (remainingMovementPoints >= pathfindingResult.Count + 1)
        {
            foreach (var item in pathfindingResult)
            {
                var tileWorldCenterPosition = tilemap.GetCellCenterWorld(item.position);
                convertedPath.Add(tileWorldCenterPosition);

                tilesPositions.Add(item.position);
            }

            convertedPath.Add(tilemap.GetCellCenterWorld(previousEndPos));
            tilesPositions.Add(previousEndPos);
        }
        else
        {
            // less movement points than path

            for (int i = 0; i < remainingMovementPoints - 1; i++)
            {
                var tileWorldCenterPosition = tilemap.GetCellCenterWorld(pathfindingResult[i].position);
                convertedPath.Add(tileWorldCenterPosition);
                tilesPositions.Add(pathfindingResult[i].position);
            }

            convertedPath.Add(tilemap.GetCellCenterWorld(pathfindingResult[remainingMovementPoints - 1].position));
            tilesPositions.Add(pathfindingResult[remainingMovementPoints - 1].position);
        }

        playerController.player.SetCharacterPath(convertedPath, tilesPositions);
        isPlayerMoving = true;
        tilemap.SetTile(previousStartPos, PathTile);

        //pathfindingResult.Clear();
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

            if (!script.IsEnd)
            {
                script.IsStart = false; script.IsEnd = false;
                tilemap.SetTile(previousStartPos, previousStart);
            }

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

    public void EndTurn()
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