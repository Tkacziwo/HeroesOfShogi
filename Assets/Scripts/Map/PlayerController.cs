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

    public static event Action<PlayerModel> PlayerSpawned;

    [SerializeField] private GameObject botGM;

    private readonly Unit kingTemplate = StaticData.unitTemplates.SingleOrDefault(o => o.UnitName == UnitEnum.King) ?? null;

    private void OnEnable()
    {
        WorldBuilding.AddResourcesToCapturer += HandleAddResources;
        DoubleClickHandler.OnDoubleClick += HandleDoubleClick;
        OverworldMapController.onTurnEnd += HandleEndTurn;
        ReturnControlToPlayerControllerAction.OnReturnControlToController += RunNextAI;
    }

    private void OnDisable()
    {
        WorldBuilding.AddResourcesToCapturer -= HandleAddResources;
        DoubleClickHandler.OnDoubleClick -= HandleDoubleClick;
        OverworldMapController.onTurnEnd -= HandleEndTurn;
        ReturnControlToPlayerControllerAction.OnReturnControlToController -= RunNextAI;
    }

    private Tilemap tilemap;

    public void OnTilemapShared(Tilemap incoming)
    {
        this.tilemap = incoming;
    }

    private void HandleDoubleClick(DoubleClickHandler handler)
        => OnPlayerBeginMove();

    private void HandleAddResources(InteractibleBuilding building)
    {
        if (building is City city)
        {
            player.HandleAddResourcesFromCity(city);
        }
        else if (building is WorldBuilding worldBuilding)
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

    private void Start()
    {
        eventFires = cityEventFires = 0;
        buildingsCount = FindObjectsByType<InteractibleBuilding>(FindObjectsSortMode.InstanceID).ToList().Count();
        cityCount = FindObjectsByType<City>(FindObjectsSortMode.InstanceID).ToList().Count();
    }

    public PlayerModel GetPlayerOverTile(Vector3Int position, int myPlayerId)
    {
        if (player.GetCurrentPlayerCharacter().characterPosition.Equals(position) && player.playerId != myPlayerId)
        {
            return player;
        }
        else
        {
            foreach (var bot in bots)
            {
                var model = bot.GetComponent<NPCModel>();

                if (model.GetCurrentPlayerCharacter().characterPosition.Equals(position) && model.playerId != myPlayerId)
                {
                    return model;
                }
            }
        }

        return null;
    }

    public bool IsPlayerOverTile(Vector3Int tilePosition)
    {
        if(player.GetCurrentPlayerCharacter().characterPosition.Equals(tilePosition)
            || bots[0].GetComponent<NPCModel>().GetCurrentPlayerCharacter().characterPosition.Equals(tilePosition))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnPlayerBeginMove();
        }
    }

    private readonly List<Vector3Int> playerStartingPositions = new()
    {
        new(7,3,0),
        new(36,15,0),
        new(15,40,0),
        new(43,49,0),
        new(53,3,0)
    };

    private readonly List<Vector3Int> playerChosenStartingPositions = new();

    private readonly System.Random rnd = new();

    public void SpawnPlayers(Tilemap tilemap, int botCount)
    {
        PlayerRegistry.Instance.NumberOfPlayers = botCount + 1;
        int playerId = 1;

        int index = rnd.Next(playerStartingPositions.Count);

        playerChosenStartingPositions.Add(playerStartingPositions[index]);
        var worldPosition = tilemap.GetCellCenterWorld(playerStartingPositions[index]);
        SpawnRealPlayer(playerId, playerStartingPositions[index], worldPosition);


        for (int i = 0; i < botCount; i++)
        {
            var vec = RandomBotPosition();
            worldPosition = tilemap.GetCellCenterWorld(vec);
            SpawnBot(i + 2, vec, worldPosition);
        }
    }

    public Vector3Int RandomBotPosition(int maxIterations = 100)
    {
        int iterations = 0;
        int nextRnd = rnd.Next(playerStartingPositions.Count);
        Vector3Int chosenVec = playerStartingPositions[nextRnd];
        if (!playerChosenStartingPositions.Contains(chosenVec))
        {
            playerChosenStartingPositions.Add(chosenVec);
            return chosenVec;
        }
        else
        {
            while (iterations < maxIterations)
            {
                nextRnd = rnd.Next(playerStartingPositions.Count);
                chosenVec = playerStartingPositions[nextRnd];
                if (!playerChosenStartingPositions.Contains(chosenVec))
                {
                    playerChosenStartingPositions.Add(chosenVec);
                    return chosenVec;
                }
                iterations++;
            }

            return new Vector3Int(0, 0, 0);
        }
    }

    public void SpawnRealPlayer(int playerId, Vector3Int targetPosition, Vector3 positionInWorld)
    {
        player = Instantiate(player);
        player.InitPlayer(playerId);
        player.SpawnPlayer(targetPosition, positionInWorld, kingTemplate);

        PlayerSpawned?.Invoke(player);
    }

    public void SpawnBot(int nextId, Vector3Int targetPosition, Vector3 positionInWorld)
    {
        var bot = Instantiate(botGM);
        bot.GetComponent<NPCModel>().InitBot(nextId);
        bot.GetComponent<NPCModel>().SpawnBotPlayer(targetPosition, positionInWorld, kingTemplate);
        bots.Add(bot);
    }

    public Dictionary<int, Dictionary<int, Vector3Int>> GetBotsCharacterPositions()
    {
        Dictionary<int, Dictionary<int, Vector3Int>> positions = new();

        foreach (var bot in bots)
        {
            var model = bot.GetComponent<NPCModel>();

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
        if (playerId == player.playerId)
        {
            return player.GetPlayerCharacters();
        }
        else
        {
            foreach (var bot in bots)
            {
                var model = bot.GetComponent<PlayerModel>();
                if (model.playerId == playerId)
                {
                    return model.GetPlayerCharacters();
                }
            }
        }

        return null;
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

    public void HandleEndTurn()
    {
        StartMapAI(0);
    }

    public void RunNextAI(int id)
    {
        StartMapAI(id - 1);
    }

    public void StartMapAI(int botId)
    {
        // start AI
        try
        {
            if (botId > bots.Count - 1) return;
            var bot = bots[botId];
            currentPlayerId = bot.GetComponent<NPCModel>().playerId;
            var agent = bot.GetComponent<BehaviorGraphAgent>();
            agent.BlackboardReference.SetVariableValue("tilemap", tilemap);
            agent.BlackboardReference.SetVariableValue("IsMyTurn", true);
            agent.BlackboardReference.SetVariableValue("reachedCity", false);
            agent.Start();
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
        }
    }

    public GameObject GetBot()
    {
        if(bots != null)
        {
            return bots[0];
        }
        return null;
    }

    public PlayerCharacterController GetCurrentPlayerCharacter()
        => player.GetCurrentPlayerCharacter();

    public PlayerModel GetCurrentPlayer()
        => player;
}