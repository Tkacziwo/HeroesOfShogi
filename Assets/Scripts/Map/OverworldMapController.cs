using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.AppUI.UI;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;

public class OverworldMapController : MonoBehaviour
{
    private List<TileInfo> pathfindingResult = new();

    private readonly PathingController pathingController = new();

    [SerializeField] Camera topCamera;

    public Tilemap tilemap;

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

    public static System.Action onTurnEnd;

    private InteractibleBuilding chosenWorldBuilding;

    private bool isPlayerMoving;

    [SerializeField] private PlayerController playerController;

    private Camera currentCamera;

    [SerializeField] private ResourceUIController resourceUIController;

    private Vector3Int playerCharacterStartPoint = Vector3Int.zero;

    private Vector3Int npcCharacterStartPoint = Vector3Int.zero;

    private bool isPlayerInCity = false;

    private int turns = 1;

    public static event System.Action BotsEndTurn;

    [SerializeField] private BuildingController buildingController;

    [SerializeField] private GameObject mapRootRef;

    private bool duringBotMove;

    [SerializeField] private TextMeshProUGUI duringBotMoveText;

    private void OnEnable()
    {
        PlayerCharacterController.OnPlayerOverTile += ClearTile;
        BuildingEvents.onBuildingClicked += FindPathToBuilding;
        PlayerEvents.OnPlayerBeginMove += MovePlayer;
        PlayerEvents.OnPlayerEndMove += HandlePlayerEndMove;
        PlayerController.TurnEnded += HandleUpdateUIResources;
        CityEvents.OnPlayerInCity += HandleOnPlayerInCity;
        PanelController.CityOpened += HandleCityOpened;
        GameOverController.OnBackToMap += HandleBackToMap;
        ContinuePathAction.OnNPCContinuePath += HandleNPCContinuesPath;
        PlayerRegistry.OnPlayersLoaded += AfterPlayersLoaded;
    }

    private void OnDisable()
    {
        PlayerCharacterController.OnPlayerOverTile -= ClearTile;
        BuildingEvents.onBuildingClicked -= FindPathToBuilding;
        PlayerEvents.OnPlayerBeginMove -= MovePlayer;
        PlayerEvents.OnPlayerEndMove -= HandlePlayerEndMove;
        PlayerController.TurnEnded -= HandleUpdateUIResources;
        CityEvents.OnPlayerInCity -= HandleOnPlayerInCity;
        PanelController.CityOpened -= HandleCityOpened;
        GameOverController.OnBackToMap -= HandleBackToMap;
        ContinuePathAction.OnNPCContinuePath -= HandleNPCContinuesPath;
        PlayerRegistry.OnPlayersLoaded -= AfterPlayersLoaded;
    }

    void Start()
    {
        playerController.SpawnPlayers(tilemap, 1);

        var playerCamera = playerController.player.GetCameraController().GetComponentInChildren<Camera>();
        playerCamera.enabled = true;
        topCamera.enabled = false;
        currentCamera = playerCamera;
        buildingController.SetCamera(playerCamera);
    }

    private void HandleBackToMap()
    {
        foreach (var r in mapRootRef.GetComponentsInChildren<Renderer>(true))
        {
            r.enabled = true;
        }
        isPlayerInBattle = false;
        var playerModel = playerController.GetCurrentPlayer();
        var playerCamera = playerModel.GetCameraController().GetComponentInChildren<Camera>();

        playerCamera.transform.SetPositionAndRotation(previousCameraPosition, previousCameraRotation);

        var winner = BattleDeploymentStaticData.winner;
        HandleCharacterDefeated(winner);
    }

    private void HandleCharacterDefeated(PlayerCharacterController winner)
    {
        var players = PlayerRegistry.Instance.GetAllPlayers();
        var realPlayer = players.Single(o => o.isRealPlayer);
        var buildings = BuildingRegistry.Instance.GetAllBuildings();

        if (winner.playerId == realPlayer.playerId)
        {
            //real player won
            realPlayer.UpdateCharacter(winner);
            var loser = BattleDeploymentStaticData.enemyCharacter;

            var loserPlayer = players.Single(o => o.playerId == loser.playerId);

            loserPlayer.KillCharacter();

            foreach (var building in buildings)
            {
                if (building is City city)
                {
                    if (city.capturerId == loserPlayer.playerId)
                    {
                        SpawnPlayerAtCity(city, loserPlayer);
                        return;
                    }
                }
            }

            resourceUIController.ShowGameOverScreen(10000);
        }
        else
        {
            //npc won
            var npcPlayer = players.Single(o => o.playerId == winner.playerId);
            npcPlayer.UpdateCharacter(winner);

            realPlayer.KillCharacter();

            foreach (var building in buildings)
            {
                if (building is City city)
                {
                    if (city.capturerId == realPlayer.playerId)
                    {
                        SpawnPlayerAtCity(city, realPlayer);
                        return;
                    }
                }
            }

            resourceUIController.ShowGameOverScreen(10000);
        }
        GameOverController.OnBackToMap -= HandleBackToMap;
    }

    private void SpawnPlayerAtCity(City building, PlayerModel playerModel)
    {
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

                        if (tile != null && tile.IsTraversable)
                        {
                            var worldPosition = tilemap.GetCellCenterWorld(destPos);
                            playerModel.SpawnPlayer(destPos, worldPosition, StaticData.unitTemplates.Single(o => o.UnitName == UnitEnum.King));
                            return;
                        }
                    }
                }
            }
        }
    }

    private void HandleCityOpened(City city)
    {
        var player = playerController.GetCurrentPlayer();
        resourceUIController.DisplayCityInfo(city, player.playerResources, player.GetCurrentPlayerCharacter());
    }

    private void HandleOnPlayerInCity(bool res)
        => isPlayerInCity = res;


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



        var playerOverTile = playerController.GetPlayerOverTile(converted, character.playerId);

        if (playerOverTile != null && playerOverTile.playerId != character.playerId)
        {
            foreach (var r in mapRootRef.GetComponentsInChildren<Renderer>(true))
            {
                r.enabled = false;
            }

            StartBattle(playerOverTile, character);
        }
        else if (converted == previousEndPos && chosenWorldBuilding != null)
        {
            chosenWorldBuilding.CaptureBuilding(character.playerId, character.playerColor);

            if (chosenWorldBuilding is City city)
            {
                if (character.playerId == playerController.player.playerId)
                {
                    isPlayerInCity = true;

                    CityEvents.OnPlayerInCity?.Invoke(true);

                    var player = playerController.GetCurrentPlayer();
                    resourceUIController.DisplayCityInfo(city, player.playerResources, player.GetCurrentPlayerCharacter());
                    resourceUIController.UpdatePlayerCityPanels(player);
                }
                else
                {
                    var agent = playerController.GetBot().GetComponent<BehaviorGraphAgent>();

                    agent.BlackboardReference.SetVariableValue("tilemap", tilemap);
                    agent.BlackboardReference.SetVariableValue("IsMyTurn", true);
                    agent.BlackboardReference.SetVariableValue("reachedCity", true);
                    agent.BlackboardReference.SetVariableValue("ChosenCity", city);
                    agent.BlackboardReference.SetVariableValue("cityHasUnits", city.HasAvailableUnits());
                    agent.Start();
                }
            }
            chosenWorldBuilding = null;
        }

        pathfindingResult.Clear();
        character.ClearPath();
        SetStartPoint(cellPos);

        bool isRealPlayer = PlayerRegistry.Instance.GetAllPlayers().Single(o => o.playerId == character.playerId).isRealPlayer;

        if (isRealPlayer)
        {
            playerCharacterStartPoint = character.characterPosition;
        }
        else
        {
            duringBotMove = false;
            duringBotMoveText.gameObject.SetActive(false);
            BotsEndTurn?.Invoke();
            SetStartPoint(playerCharacterStartPoint);
            npcCharacterStartPoint = character.characterPosition;
        }

        isPlayerMoving = false;
    }

    public void FindPathToBuilding(InteractibleBuilding building)
    {
        //finding neighbours of tiles;
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
                                    city.CaptureBuilding(currentCharacter.playerId, currentCharacter.playerColor);
                                    var player = playerController.GetCurrentPlayer();
                                    resourceUIController.DisplayCityInfo(city, player.playerResources, player.GetCurrentPlayerCharacter());
                                }
                                else if (building is WorldBuilding worldBuilding)
                                {
                                    worldBuilding.CaptureBuilding(currentCharacter.playerId, currentCharacter.playerColor);
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

        SetEndPoint(bestEndPosition);
        pathfindingResult = bestPath;
        DisplayPath(pathfindingResult);
        TileInfo endTile = new()
        {
            position = bestEndPosition,
        };
        pathfindingResult.Add(endTile);

        chosenWorldBuilding = building;
    }

    private void AfterPlayersLoaded()
    {
        var players = PlayerRegistry.Instance.GetAllPlayers();

        playerCharacterStartPoint = players.Single(o => o.isRealPlayer).GetCurrentPlayerCharacter().characterPosition;

        SetStartPoint(playerCharacterStartPoint);

        npcCharacterStartPoint = playerController.GetBot().GetComponent<NPCModel>().GetCurrentPlayerCharacter().characterPosition;
        tilemap.SetTile(playerCharacterStartPoint, StartTile);
        tilemap.SetTile(npcCharacterStartPoint, StartTile);
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

    private Vector3Int previousStartPos;

    private Vector3Int previousEndPos;

    private bool isPlayerInBattle = false;

    private Vector3 previousCameraPosition;

    private Quaternion previousCameraRotation;

    void Update()
    {
        if (isPlayerInCity) return;

        if (isPlayerInBattle) return;

        if (duringBotMove) return;

        Ray ray = currentCamera.ScreenPointToRay(Input.mousePosition);

        Plane ground = new(Vector3.up, Vector3.zero);

        if (!EventSystem.current.IsPointerOverGameObject() && ground.Raycast(ray, out float enter) && !isPlayerMoving)
        {
            Vector3 point = ray.GetPoint(enter);

            Vector3Int cellPos = tilemap.WorldToCell(point);

            var t = tilemap.GetTile<MapTile>(cellPos);

            if (t != null && Input.GetMouseButtonDown(0))
            {
                if (!previousEndPos.Equals(cellPos))
                {
                    ClearTiles();
                }

                if (t.IsTraversable || (!playerCharacterStartPoint.Equals(cellPos) && playerController.IsPlayerOverTile(cellPos)))
                {
                    SetEndPoint(cellPos);
                    FindPath();
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (!duringBotMove)
            {
                EndTurn();
            }
        }
    }

    private void StartBattle(PlayerModel playerOverTile, PlayerCharacterController character)
    {

        if (playerOverTile.isRealPlayer)
        {
            BattleDeploymentStaticData.playerCharacter = playerOverTile.GetCurrentPlayerCharacter();
            BattleDeploymentStaticData.enemyCharacter = character;

        }
        else
        {
            BattleDeploymentStaticData.playerCharacter = character;
            BattleDeploymentStaticData.enemyCharacter = playerOverTile.GetCurrentPlayerCharacter();
        }

        var playerModel = PlayerRegistry.Instance.GetAllPlayers().Single(o => o.isRealPlayer);
        var playerCamera = playerModel.GetCameraController().GetComponentInChildren<Camera>();

        playerCamera.transform.GetPositionAndRotation(out Vector3 position, out Quaternion rotation);

        previousCameraPosition = DeepCopyVector3(position);
        previousCameraRotation = DeepCopyQuaternion(rotation);


        Vector3 battleCameraPosition = new(20, 30.3f, 15);
        Quaternion battleCameraRotation = Quaternion.Euler(60, -90, 0);

        playerCamera.transform.SetPositionAndRotation(battleCameraPosition, battleCameraRotation);

        isPlayerInBattle = !isPlayerInBattle;
        SceneManager.LoadScene("BattleDeployment", LoadSceneMode.Additive);
    }

    public void HandleNPCContinuesPath(List<TileInfo> path)
    {
        int currentPlayerId = playerController.currentPlayerId;
        tilemap.SetTile(playerCharacterStartPoint, StartTile);

        List<NPCModel> botModels = new();
        playerController.bots.ForEach(o => botModels.Add(o.GetComponent<NPCModel>()));
        var botModel = botModels.Single(o => o.playerId == currentPlayerId);

        chosenWorldBuilding = botModel.ChosenBuilding != null && botModel.ReachedDestination ? botModel.ChosenBuilding : null;

        pathfindingResult = new(path);
        if (pathfindingResult.Count == 0) return;

        var cellPos = pathfindingResult[pathfindingResult.Count - 1].position;

        //var tile = tilemap.GetTile<MapTile>(cellPos);

        tilemap.SetTile(botModel.GetCurrentPlayerCharacter().characterPosition, PathTile);
        SetEndPoint(cellPos);
        DisplayPath(pathfindingResult);
        //tilemap.SetTile(npcCharacterStartPoint, StartTile);
        MovePlayer(botModel.GetCurrentPlayerCharacter());
    }

    public void MovePlayer(PlayerCharacterController currentCharacter)
    {
        if (tilemap.WorldToCell(currentCharacter.characterPosition) == previousEndPos) return;

        var remainingMovementPoints = currentCharacter.GetRemainingMovementPoints();

        List<Vector3> convertedPath = new();

        if (remainingMovementPoints <= 0 || pathfindingResult.Count == 0) return;

        int usedMovementPoints;
        if (remainingMovementPoints >= pathfindingResult.Count)
        {
            // Move the whole path
            foreach (var item in pathfindingResult)
            {
                var tileWorldCenterPosition = tilemap.GetCellCenterWorld(item.position);
                convertedPath.Add(tileWorldCenterPosition);

            }
            usedMovementPoints = pathfindingResult.Count;
        }
        else
        {
            // Less movement points than path
            for (int i = 0; i < remainingMovementPoints; i++)
            {
                var tileWorldCenterPosition = tilemap.GetCellCenterWorld(pathfindingResult[i].position);
                convertedPath.Add(tileWorldCenterPosition);
            }
            usedMovementPoints = remainingMovementPoints;

            if (chosenWorldBuilding != null) chosenWorldBuilding = null;
        }

        currentCharacter.SetPath(convertedPath);

        currentCharacter.ReduceAvailableMovementPoints(usedMovementPoints);
        isPlayerMoving = true;
        tilemap.SetTile(previousStartPos, PathTile);

        pathfindingResult.Clear();
    }

    private void FindPath()
    {
        var watch = System.Diagnostics.Stopwatch.StartNew();

        pathingController.SetParameters(tilemap, previousStartPos, previousEndPos);
        pathfindingResult = pathingController.FindPath();

        DisplayPath(pathfindingResult);

        TileInfo endTile = new()
        {
            position = previousEndPos,
        };
        pathfindingResult.Add(endTile);

        float elapsed = watch.ElapsedMilliseconds * 0.001f;
        Debug.Log($"Elapsed: {elapsed} seconds");
        watch.Stop();
        chosenWorldBuilding = null;
        tilemap.SetTile(playerCharacterStartPoint, StartTile);
        tilemap.SetTile(npcCharacterStartPoint, StartTile);
    }

    private void SetEndPoint(Vector3Int cellPos)
    {
        if (!previousEndPos.Equals(Vector3Int.zero))
        {
            tilemap.SetTile(previousEndPos, PathTile);
        }
        tilemap.SetTile(cellPos, PathResultTile);
        previousEndPos = cellPos;
    }

    private void RemoveEndPoint()
    {
        if (previousEndPos != Vector3Int.zero)
        {
            tilemap.SetTile(previousEndPos, PathTile);
        }
    }

    private void SetStartPoint(Vector3Int cellPos)
    {
        tilemap.SetTile(cellPos, StartTile);
        previousStartPos = cellPos;
    }

    private void ClearTiles()
    {
        foreach (var item in pathfindingResult)
        {
            tilemap.SetTile(item.position, PathTile);
        }

        pathfindingResult.Clear();

        tilemap.SetTile(playerCharacterStartPoint, StartTile);
        tilemap.SetTile(npcCharacterStartPoint, StartTile);
    }

    public static System.Action OnWeekEnd;

    public void EndTurn()
    {
        if (!duringBotMove)
        {
            duringBotMove = true;
            duringBotMoveText.gameObject.SetActive(true);
            playerController.OnTilemapShared(tilemap);
            onTurnEnd?.Invoke();
            RemoveEndPoint();
            turns++;
            ClearTiles();
            if (turns % 7 == 0)
            {
                OnWeekEnd?.Invoke();
            }
        }
    }

    private Vector3 DeepCopyVector3(Vector3 vec)
        => new(vec.x, vec.y, vec.z);

    private Quaternion DeepCopyQuaternion(Quaternion q)
        => new(q.x, q.y, q.z, q.w);

    public void ClearTile(Vector3 tilePosition)
        => tilemap.SetTile(tilemap.WorldToCell(tilePosition), PathTile);
}