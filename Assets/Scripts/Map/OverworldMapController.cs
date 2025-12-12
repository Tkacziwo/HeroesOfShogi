using System;
using System.Collections.Generic;
using System.Linq;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

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

    public static Action onTurnEnd;

    public List<InteractibleBuilding> worldBuildings;

    private InteractibleBuilding chosenWorldBuilding;

    private bool isPlayerMoving;

    [SerializeField] private PlayerController playerController;

    private Camera currentCamera;

    [SerializeField] private ResourceUIController resourceUIController;


    private Dictionary<int, Vector3Int> characterStartPoints = new();

    private Dictionary<int, Dictionary<int, Vector3Int>> botStartPoints = new();

    private bool isPlayerInCity = false;

    private int turns = 1;

    public static event Action BotsEndTurn;

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
        MoveToTargetAction.OnBotMove += HandleBotMove;
        CaptureBuildingAction.OnBotCapture += HandleBotCapture;
        MoveToCityAction.OnBotGoToCity += HandleBotCapture;
        AIEvents.OnBotMove += HandleBotMoveSingle;
        GameOverController.OnBackToMap += HandleBackToMap;
        ContinuePathAction.OnNPCContinuePath += HandleNPCContinuesPath;
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
        MoveToTargetAction.OnBotMove -= HandleBotMove;
        CaptureBuildingAction.OnBotCapture -= HandleBotCapture;
        MoveToCityAction.OnBotGoToCity -= HandleBotCapture;
        AIEvents.OnBotMove -= HandleBotMoveSingle;
        GameOverController.OnBackToMap -= HandleBackToMap;
        ContinuePathAction.OnNPCContinuePath -= HandleNPCContinuesPath;
    }

    private void HandleBackToMap()
    {
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
                        SpawnPlayerAtCity(city, realPlayer);
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
            botStartPoints = playerController.GetBotsCharacterPositions();
            foreach (var bot in botStartPoints)
            {
                foreach (var position in bot.Value)
                {
                    tilemap.SetTile(position.Value, StartTile);
                }
            }
            foreach (var building in buildings)
            {
                if (building is City city)
                {
                    if (city.capturerId == realPlayer.playerId)
                    {
                        SpawnPlayerAtCity(city, npcPlayer);
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

    private void HandleBotCapture(BotCaptureInfo info)
    {
        chosenWorldBuilding = info.chosenBuilding;
        pathfindingResult = info.pathToBuilding;

        if (pathfindingResult.Count != 0)
        {
            var t = tilemap.GetTile<MapTile>(info.endPos);
            SetEndPoint(info.endPos, t);
            DisplayPath(pathfindingResult);
            previousStartPos = botStartPoints[info.character.playerId][info.character.characterId];

            tilemap.SetTile(previousStartPos, PathTile);
            MovePlayer(info.character);
        }
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
                    var player = playerController.GetCurrentPlayer();
                    resourceUIController.DisplayCityInfo(city, player.playerResources, player.GetCurrentPlayerCharacter());
                    resourceUIController.UpdatePlayerCityPanels(player);
                }
            }
            chosenWorldBuilding = null;
        }

        pathfindingResult.Clear();
        character.ClearPath();
        SetStartPoint(cellPos, t);

        if (character.playerId == playerController.player.playerId)
        {
            characterStartPoints[character.characterId] = cellPos;
        }
        else
        {

            BotsEndTurn?.Invoke();
            playerController.currentPlayerId = 100;
            previousStartPos = playerController.player.GetCharacterPosition(1);
            botStartPoints[character.playerId][character.characterId] = cellPos;
        }

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

        var t = tilemap.GetTile<MapTile>(bestEndPosition);
        SetEndPoint(bestEndPosition, t);
        pathfindingResult = bestPath;
        DisplayPath(pathfindingResult);
        TileInfo endTile = new()
        {
            position = bestEndPosition,
        };
        pathfindingResult.Add(endTile);

        chosenWorldBuilding = building;
    }

    void Start()
    {
        playerController.SpawnPlayers(tilemap, 1);


        var playerCamera = playerController.player.GetCameraController().GetComponentInChildren<Camera>();
        playerCamera.enabled = false;
        currentCamera = topCamera;

        var playerStartingPosition = playerController.player.GetPlayerPositionById(1);
        var vec = tilemap.WorldToCell(playerStartingPosition);
        playerController.player.SetCharacterPosition(vec, 1);

        previousStartPos = playerController.player.GetCharacterPosition(1);
        var t = tilemap.GetTile<MapTile>(vec);
        if (t != null)
        {
            SetStartPoint(vec, t);
        }

        botStartPoints = playerController.GetBotsCharacterPositions();

        characterStartPoints = new()
        {
            { 1, vec },
        };

        foreach (var item in characterStartPoints)
        {
            tilemap.SetTile(item.Value, StartTile);
        }

        foreach (var bot in botStartPoints)
        {
            foreach (var position in bot.Value)
            {
                tilemap.SetTile(position.Value, StartTile);
            }
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

    private Vector3 previousCameraPosition;

    private Quaternion previousCameraRotation;

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

            if (t != null && Input.GetMouseButtonDown(0))
            {
                ClearTiles();
                if (t.IsTraversable)
                {
                    SetEndPoint(cellPos, t);
                    FindPath();
                }
                else
                {
                    foreach (var bot in playerController.bots)
                    {
                        var character = bot.GetComponent<PlayerModel>().GetCurrentPlayerCharacter();

                        if (character.characterPosition == cellPos)
                        {
                            SetEndPoint(cellPos, t);
                            FindPath();
                        }
                    }
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.Return))
        {
            EndTurn();
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

        List<NPCModel> botModels = new();
        playerController.bots.ForEach(o => botModels.Add(o.GetComponent<NPCModel>()));
        var botModel = botModels.Single(o => o.playerId == currentPlayerId);

        chosenWorldBuilding = botModel.ChosenBuilding != null ? botModel.ChosenBuilding : null;

        pathfindingResult = new(path);
        if (pathfindingResult.Count == 0) return;

        var cellPos = pathfindingResult[pathfindingResult.Count - 1].position;

        var tile = tilemap.GetTile<MapTile>(cellPos);

        SetEndPoint(cellPos, tile);
        DisplayPath(pathfindingResult);

        MovePlayer(botModel.GetCurrentPlayerCharacter());
    }

    public void HandleBotMove(List<Tuple<int, List<TileInfo>>> botResults)
    {
        //Move bots 
        int currentPlayerId = playerController.currentPlayerId;

        List<NPCModel> botModels = new();
        playerController.bots.ForEach(o => botModels.Add(o.GetComponent<NPCModel>()));
        var botModel = botModels.Single(o => o.playerId == currentPlayerId);

        foreach (var characterResult in botResults)
        {
            var character = botModel.GetCharacterById(characterResult.Item1);
            pathfindingResult = characterResult.Item2;

            if (pathfindingResult.Count != 0)
            {
                var cellPos = pathfindingResult[pathfindingResult.Count - 1].position;

                var tile = tilemap.GetTile<MapTile>(cellPos);

                SetEndPoint(cellPos, tile);
                pathfindingResult.RemoveAt(pathfindingResult.Count - 1);
                DisplayPath(pathfindingResult);
                previousStartPos = botStartPoints[currentPlayerId][character.characterId];

                tilemap.SetTile(previousStartPos, PathTile);
                MovePlayer(character);
            }
        }
    }

    public void HandleBotMoveSingle(Tuple<int, List<TileInfo>> botResult)
    {
        //Move bots 
        int currentPlayerId = playerController.currentPlayerId;

        //List<PlayerModel> botModels = new();
        //playerController.bots.ForEach(o => botModels.Add(o.GetComponent<PlayerModel>()));
        //var botModel = botModels.Single(o => o.playerId == currentPlayerId);

        var botModel = PlayerRegistry.Instance.GetAllPlayers().SingleOrDefault(o => o.playerId == currentPlayerId);

        if (botModel == null) return;

        var character = botModel.GetCurrentPlayerCharacter();
        pathfindingResult = botResult.Item2;

        if (pathfindingResult.Count != 0)
        {
            var cellPos = pathfindingResult[pathfindingResult.Count - 1].position;

            var tile = tilemap.GetTile<MapTile>(cellPos);

            SetEndPoint(cellPos, tile);
            pathfindingResult.RemoveAt(pathfindingResult.Count - 1);
            DisplayPath(pathfindingResult);
            previousStartPos = botStartPoints[currentPlayerId][character.characterId];

            tilemap.SetTile(previousStartPos, PathTile);
            MovePlayer(character);
        }
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
        start = tilemap.GetTile<MapTile>(previousStartPos);
        end = tilemap.GetTile<MapTile>(previousEndPos);

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

        foreach (var bot in botStartPoints)
        {
            foreach (var position in bot.Value)
            {
                tilemap.SetTile(position.Value, StartTile);
            }
        }
    }

    public static Action OnWeekEnd;

    public void EndTurn()
    {
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

    private Vector3 DeepCopyVector3(Vector3 vec)
        => new(vec.x, vec.y, vec.z);

    private Quaternion DeepCopyQuaternion(Quaternion q)
        => new(q.x, q.y, q.z, q.w);

    public void ClearTile(Vector3 tilePosition)
        => tilemap.SetTile(tilemap.WorldToCell(tilePosition), PathTile);
}