using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public PlayerModel player;

    public int currentPlayer;

    private int buildingsCount;

    private int cityCount;

    private int cityEventFires;

    private int eventFires;

    public static event Action TurnEnded;

    public static event Action<Tuple<Vector3Int, Vector3Int>> PlayerCharacterChanged;

    public static event Action<Camera> CameraChanged;

    public static event Action<PlayerModel> PlayerSpawned;

    private void OnEnable()
    {
        WorldBuilding.AddResourcesToCapturer += HandleAddResources;
        DoubleClickHandler.OnDoubleClick += HandleDoubleClick;
        PanelController.PlayerChanged += HandleCharacterChanged;
    }

    private void OnDisable()
    {
        WorldBuilding.AddResourcesToCapturer -= HandleAddResources;
        DoubleClickHandler.OnDoubleClick -= HandleDoubleClick;
        PanelController.PlayerChanged -= HandleCharacterChanged;
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
        player.SpawnPlayer();

        PlayerSpawned?.Invoke(player);
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

    public PlayerCharacterController GetCurrentPlayerCharacter()
        => player.GetCurrentPlayerCharacter();

    public PlayerModel GetCurrentPlayer()
        => player;
}