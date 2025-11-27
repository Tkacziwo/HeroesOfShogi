using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
    public PlayerModel player;

    public List<GameObject> bots;

    public int currentPlayerId;

    private int buildingsCount;

    private int cityCount;

    private int cityEventFires;

    private int eventFires;

    public static event System.Action TurnEnded;

    public static event Action<Tuple<Vector3Int, Vector3Int>> PlayerCharacterChanged;

    public static event Action<Camera> CameraChanged;

    public static event Action<PlayerModel> PlayerSpawned;

    [SerializeField] private GameObject botGM;

    private void OnEnable()
    {
        WorldBuilding.AddResourcesToCapturer += HandleAddResources;
        DoubleClickHandler.OnDoubleClick += HandleDoubleClick;
        PanelController.PlayerChanged += HandleCharacterChanged;
        OverworldMapController.onTurnEnd += HandleTurnEnd;
    }

    private void OnDisable()
    {
        WorldBuilding.AddResourcesToCapturer -= HandleAddResources;
        DoubleClickHandler.OnDoubleClick -= HandleDoubleClick;
        PanelController.PlayerChanged -= HandleCharacterChanged;
        OverworldMapController.onTurnEnd -= HandleTurnEnd;
    }

    private Tilemap tilemap;

    public void OnTilemapShared(Tilemap incoming)
    {
        this.tilemap = incoming;
    }

    private void HandleDoubleClick(DoubleClickHandler handler)
        => OnPlayerBeginMove();

    private void HandleCharacterChanged(PlayerCharacterController character)
    {
        OnPlayerCharacterChanged(character.characterId);
    }

   
    private void HandleAddResources(InteractibleBuilding building)
    {
        if(building is City city)
        {
            player.HandleAddResourcesFromCity(city);
        }
        else if(building is WorldBuilding worldBuilding)
        {
            player.HandleAddResources(worldBuilding);
        }

        eventFires++;

        if (eventFires == buildingsCount)
        {
            eventFires = 0;
            TurnEnded?.Invoke();
        }
    }

    public void SetCharacterPosition(Vector3Int newPosition)
        => player.SetCharacterPosition(newPosition);

    public void SetCharacterPath(List<Vector3> positions, List<Vector3Int> tilesPositions)
        => player.SetCharacterPath(positions, tilesPositions);

    private void Start()
    {
        eventFires = cityEventFires = 0;
        buildingsCount = FindObjectsByType<InteractibleBuilding>(FindObjectsSortMode.InstanceID).ToList().Count();
        cityCount = FindObjectsByType<City>(FindObjectsSortMode.InstanceID).ToList().Count();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnPlayerBeginMove();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            OnCameraChanged();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            OnPlayerCharacterChanged(1);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            OnPlayerCharacterChanged(2);
        }
    }

    public void OnCameraChanged()
    {
        var playerCamera = this.player.GetCameraController().GetComponentInChildren<Camera>();
        playerCamera.enabled = !playerCamera.enabled;
        CameraChanged?.Invoke(playerCamera);
    }

    public void OnPlayerCharacterChanged(int characterId)
    {
        this.player.ChangeCharacters(characterId);
        var playerStartingPosition = this.player.GetPlayerPositionById(characterId);
        var vec = new Vector3Int((int)playerStartingPosition.x, (int)playerStartingPosition.y, (int)playerStartingPosition.z);
        player.SetCharacterPosition(vec, characterId);

        var previousStartPosition = this.player.GetCharacterPosition(characterId);
        PlayerCharacterChanged?.Invoke(new(previousStartPosition, vec));
        Debug.Log($"Changed character to: {characterId}");
    }

    public void SpawnRealPlayer(int playerId)
    {
        player = Instantiate(player);
        player.InitPlayer(playerId);
        Vector3Int target = new(6, 4, 0);
        player.SpawnPlayer(target);

        PlayerSpawned?.Invoke(player);
    }

    public void SpawnBots()
    {
        int iterator = 0;
        bots.Add(Instantiate(botGM));
        var player = bots[iterator].GetComponent<PlayerModel>();
        Vector3Int target = new(40, 15, 0);
        int nextId = GetNextPlayerId();
        player.InitBot(nextId);
        player.SpawnBotPlayer(target);
    }

    public Dictionary<int, Dictionary<int, Vector3Int>> GetBotsCharacterPositions()
    {
        Dictionary<int, Dictionary<int, Vector3Int>> positions = new();

        foreach (var bot in bots)
        {
            var model = bot.GetComponent<PlayerModel>();

            var characters = model.GetPlayerCharacters();

            Dictionary<int, Vector3Int> botCharacterPositions = new();
            foreach (var character in characters)
            {
                botCharacterPositions.Add(character.characterId, character.characterPosition);
            }

            positions.Add(model.playerId, botCharacterPositions);
        }

        return positions;
    }

    public List<PlayerCharacterController> GetCharactersForPlayer(int playerId)
    {
        if(playerId == player.playerId)
        {
            return player.GetPlayerCharacters();
        }
        else
        {
            foreach (var bot in bots)
            {
                var model = bot.GetComponent<PlayerModel>();
                if(model.playerId == playerId)
                {
                    return model.GetPlayerCharacters();
                }
            }
        }

        return null;
    }

    public int GetNextPlayerId()
    {
        var playerId = player.playerId;

        var botsModels = new List<PlayerModel>();

        foreach (var bot in bots)
        {
            botsModels.Add(bot.GetComponent<PlayerModel>());
        }

        var botMaxId = botsModels.Select(o => o.playerId).Max();

        if(playerId < botMaxId)
        {
            return botMaxId + 1;
        }
        else
        {
            return playerId + 1;
        }
    }

    public void OnPlayerBeginMove()
    {
        var character = player.GetCurrentPlayerCharacter();
        if (character.GetRemainingMovementPoints() <= 0)
        {
            Debug.Log("Not enough movement points");
        }
        else
        {
             player.PlayerBeginMove();
        }
    }

    public void HandleTurnEnd()
    {
        // start AI

        foreach (var bot in bots)
        {
            currentPlayerId = bot.GetComponent<PlayerModel>().playerId;
            var agent = bot.GetComponent<BehaviorGraphAgent>();
            agent.BlackboardReference.SetVariableValue("tilemap", tilemap);
            agent.BlackboardReference.SetVariableValue("IsMyTurn", true);
            agent.Start();
        }


      
    }

    public PlayerCharacterController GetCurrentPlayerCharacter()
        => player.GetCurrentPlayerCharacter();

    public PlayerModel GetCurrentPlayer()
        => player;
}