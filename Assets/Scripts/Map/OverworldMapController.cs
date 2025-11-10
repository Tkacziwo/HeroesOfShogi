using System;
using System.Collections.Generic;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

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

    private Dictionary<int, Vector3Int> characterStartPoints = new();

    private bool isPlayerInCity = false;

    private int turns = 1;

    private void OnEnable()
    {
        PlayerCharacterController.OnPlayerOverTile += ClearTile;
        BuildingEvents.onBuildingClicked += FindPathToBuilding;
        PlayerEvents.OnPlayerBeginMove += MovePlayer;
        PlayerEvents.OnPlayerEndMove += HandlePlayerEndMove;
        PlayerController.TurnEnded += HandleUpdateUIResources;
        PlayerController.PlayerCharacterChanged += HandlePlayerCharacterChanged;
        PlayerController.CameraChanged += HandleCameraChanged;
        CityViewController.OnCityViewClose += HandleCityViewClosed;
        PanelController.CityOpened += HandleCityOpened;
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
        CityViewController.OnCityViewClose -= HandleCityViewClosed;
        PanelController.CityOpened -= HandleCityOpened;

    }

    private void HandleCityOpened(City city)
    {
        var player = playerController.GetCurrentPlayer();
        resourceUIController.DisplayCityInfo(city, player.playerResources, player.GetCurrentPlayerCharacter());
    }

    private void HandleCityViewClosed()
        => isPlayerInCity = false;

    private void HandlePlayerCharacterChanged(Tuple<Vector3Int, Vector3Int> positions)
    {
        previousStartPos = positions.Item1;

        ClearTiles();
        RemoveEndPoint();
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
        Vector3Int cellPos = tilemap.WorldToCell(currentPosition);
        var t = tilemap.GetTile<MapTile>(cellPos);
        character.SetPlayerPosition(cellPos);

        if (converted == previousEndPos && chosenWorldBuilding != null)
        {
            chosenWorldBuilding.CaptureBuilding(character.playerId, character.playerColor);

            if (chosenWorldBuilding is City city)
            {
                isPlayerInCity = true;
                var player = playerController.GetCurrentPlayer();
                resourceUIController.DisplayCityInfo(city, player.playerResources, player.GetCurrentPlayerCharacter());
                resourceUIController.UpdatePlayerCityPanels(player);
            }
            chosenWorldBuilding = null;
        }

        pathfindingResult.Clear();
        character.ClearPath();
        SetStartPoint(cellPos, t);
        characterStartPoints[character.characterId] = cellPos;
        isPlayerMoving = false;
    }

    public void FindPathToBuilding(InteractibleBuilding building)
    {
        //finding neighbours of tiles;
        if (chosenWorldBuilding == building) return;
        if (pathfindingResult.Count > 0) ClearTiles();

        List<Vector3Int> traversableTiles = new();

        var colliderBounds = building.GetComponent<BoxCollider>().bounds;
        var currentCharacter = playerController.GetCurrentPlayerCharacter();

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

                        if (tile != null)
                        {
                            if (currentCharacter.characterPosition.Equals(destPos))
                            {
                                if (building is City city)
                                {
                                    var player = playerController.GetCurrentPlayer();
                                    resourceUIController.DisplayCityInfo(city, player.playerResources, player.GetCurrentPlayerCharacter());
                                }
                                return;
                            }
                            else if (!traversableTiles.Contains(destPos) && tile.IsTraversable)
                            {
                                traversableTiles.Add(destPos);
                            }
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
        var vec = tilemap.WorldToCell(playerStartingPosition);
        playerController.player.SetCharacterPosition(vec, 1);

        var player2StartingPosition = playerController.player.GetPlayerPositionById(2);
        var vec2 = tilemap.WorldToCell(player2StartingPosition);
        playerController.player.SetCharacterPosition(vec2, 2);

        previousStartPos = playerController.player.GetCharacterPosition(1);
        var t = tilemap.GetTile<MapTile>(vec);
        SetStartPoint(vec, t);

        characterStartPoints = new()
        {
            { 1, vec },
            { 2, vec2 },
        };

        foreach (var item in characterStartPoints)
        {
            tilemap.SetTile(item.Value, StartTile);
        }
    }

    private void DisplayPath(List<TileInfo> closedList)
    {
        if (closedList.Count <= 0) return;

        var remainingMovementPoints = playerController.GetCurrentPlayerCharacter().GetRemainingMovementPoints();

        if (remainingMovementPoints <= 0) return;

        if (remainingMovementPoints >= closedList.Count)
        {
            // Display full path
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

    private bool isPlayerInBattle = false;

    // Update is called once per frame
    void Update()
    {
        if (isPlayerInCity) return;

        if (isPlayerInBattle) return;

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

        if (Input.GetKeyDown(KeyCode.U))
        {
            //Test battle
            BattleDeploymentStaticData.playerCharacter = playerController.GetCurrentPlayerCharacter();
            isPlayerInBattle = !isPlayerInBattle;
            SceneManager.LoadScene("BattleDeployment", LoadSceneMode.Additive);
        }
    }

    public void MovePlayer(PlayerModel controller)
    {
        var character = controller.GetPlayerPosition();
        if (tilemap.WorldToCell(character) == previousEndPos) return;

        var currentCharacter = controller.GetCurrentPlayerCharacter();

        var remainingMovementPoints = currentCharacter.GetRemainingMovementPoints();

        List<Vector3> convertedPath = new();
        List<Vector3Int> tilesPositions = new();

        if (remainingMovementPoints <= 0) return;

        if (pathfindingResult == null || pathfindingResult.Count == 0) return;

        int usedMovementPoints = 0;

        if (remainingMovementPoints >= pathfindingResult.Count + 1)
        {
            // Move the whole path
            foreach (var item in pathfindingResult)
            {
                var tileWorldCenterPosition = tilemap.GetCellCenterWorld(item.position);
                convertedPath.Add(tileWorldCenterPosition);

                tilesPositions.Add(item.position);
            }

            convertedPath.Add(tilemap.GetCellCenterWorld(previousEndPos));
            tilesPositions.Add(previousEndPos);

            usedMovementPoints = pathfindingResult.Count + 1;
        }
        else
        {
            // Less movement points than path
            for (int i = 0; i < remainingMovementPoints - 1; i++)
            {
                var tileWorldCenterPosition = tilemap.GetCellCenterWorld(pathfindingResult[i].position);
                convertedPath.Add(tileWorldCenterPosition);
                tilesPositions.Add(pathfindingResult[i].position);
            }

            convertedPath.Add(tilemap.GetCellCenterWorld(pathfindingResult[remainingMovementPoints - 1].position));
            tilesPositions.Add(pathfindingResult[remainingMovementPoints - 1].position);

            usedMovementPoints = remainingMovementPoints;

            if (chosenWorldBuilding != null)
            {
                chosenWorldBuilding = null;
            }
        }

        playerController.player.SetCharacterPath(convertedPath, tilesPositions);

        currentCharacter.ReduceAvailableMovementPoints(usedMovementPoints);
        isPlayerMoving = true;
        tilemap.SetTile(previousStartPos, PathTile);

        pathfindingResult.Clear();
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

    private void RemoveEndPoint()
    {
        if (previousEnd != null)
        {
            var script = tilemap.GetTile<MapTile>(previousEndPos);
            script.IsEnd = false; script.IsStart = false;
            tilemap.SetTile(previousEndPos, previousEnd);

            //[ToDo] fix 
            //var currentCharacterPosition = playerController.player.GetPlayerPosition();
            //previousEndPos = tilemap.WorldToCell(currentCharacterPosition);
        }
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

        foreach (var item in characterStartPoints)
        {
            tilemap.SetTile(item.Value, StartTile);
        }
    }

    public static Action OnWeekEnd;

    public void EndTurn()
    {
        onTurnEnd?.Invoke();
        RemoveEndPoint();
        turns++;

        ClearTiles();

        if ((turns - 1) % 7 == 0)
        {
            Debug.Log("week end");

            OnWeekEnd?.Invoke();
        }
    }
}