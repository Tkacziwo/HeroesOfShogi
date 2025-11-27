using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerModel : MonoBehaviour
{
    public int playerId;

    public bool isRealPlayer;

    public Color playerColor;

    private List<PlayerCharacterController> playerCharacters;

    private PlayerCharacterController character;

    [SerializeField] private GameObject playerModel;

    public PlayerResources playerResources;

    private CameraController cameraController;

    [SerializeField] uint maxCharacters = 3;

    public static Action<PlayerResources> UpdateResourceUI;

    private void OnEnable()
    {
        CityViewController.OnTakePlayerResources += HandleBuildingUpgraded;
    }

    private void OnDisable()
    {
        if (character != null) character.OnPlayerMoveUpdateCameraPosition -= UpdateCameraPosition;
        CityViewController.OnTakePlayerResources -= HandleBuildingUpgraded;

    }

    private void HandleBuildingUpgraded(int id, RequiredResources resources)
    {
        if (playerId != id) return;

        playerResources.Wood -= (int)resources.wood;
        playerResources.Stone -= (int)resources.stone;
        playerResources.Gold -= (int)resources.gold;
        UpdateResourceUI?.Invoke(this.playerResources);
        //playerResources.LifeResin -= (int)resources.goldResin;
    }

    public CameraController GetCameraController()
       => cameraController;

    private void SubscribeToCharacterEvents()
    {
        character.OnPlayerMoveUpdateCameraPosition += UpdateCameraPosition;
    }

    private void UpdateCameraPosition(Transform transform)
    {
        if (cameraController.isCameraFocusedOnPlayer)
            cameraController.UpdateCameraPosition(transform);
    }

    public void PlayerBeginMove()
        => PlayerEvents.OnPlayerBeginMove?.Invoke(character);

    public void InitPlayer(int playerId)
    {
        cameraController = this.GetComponentInChildren<CameraController>();
        cameraController.InitCamera();
        this.playerId = playerId;
        isRealPlayer = true;
        playerResources = new();
        cameraController.isCameraFocusedOnPlayer = true;
    }

    public void InitBot(int botId)
    {
        this.playerId = botId;
        playerResources = new();
    }

    public void SpawnOneCharacter()
    {
        var p = Instantiate(playerModel);
        character = p.GetComponent<PlayerCharacterController>();
        character.playerId = this.playerId;
        character.playerColor = playerColor;
        character.characterId = 1;
        character.movementPoints = 20;
        character.AssignedUnits.Add(new Unit()
        {
            UnitName = UnitEnum.King,
            HealthPoints = 3,
            AttackPower = 2,
            SizeInArmy = 0,
            isKing = true,
            UnitSprite = StaticData.unitIcons.SingleOrDefault(o => o.name == UnitEnum.King.ToString())

        });
        var vec = new Vector3Int(39, 13, 0);
        character.SetPlayerPosition(vec);
        character.SetTargetPosition(vec);
    }

    public void SpawnBotPlayer(Vector3Int targetPos)
    {
        var p = Instantiate(playerModel);

        character = p.GetComponent<PlayerCharacterController>();
        character.playerId = this.playerId;
        character.playerColor = playerColor;
        character.characterId = 1;
        character.movementPoints = 20;

        character.AssignedUnits.Add(new Unit()
        {
            UnitName = UnitEnum.King,
            HealthPoints = 3,
            AttackPower = 2,
            SizeInArmy = 0,
            isKing = true,
            UnitSprite = StaticData.unitIcons.SingleOrDefault(o => o.name == UnitEnum.King.ToString())
        });

        character.SetPlayerPosition(targetPos);
        character.SetTargetPosition(targetPos);

        playerCharacters = new List<PlayerCharacterController>() { character };
    }

    public void SpawnPlayer(Vector3Int targetPos)
    {
        var p = Instantiate(playerModel);

        character = p.GetComponent<PlayerCharacterController>();
        character.playerId = this.playerId;
        character.playerColor = playerColor;
        character.characterId = 1;
        character.movementPoints = 20;

        character.AssignedUnits.Add(new Unit()
        {
            UnitName = UnitEnum.King,
            HealthPoints = 3,
            AttackPower = 2,
            SizeInArmy = 0,
            isKing = true,
            UnitSprite = StaticData.unitIcons.SingleOrDefault(o => o.name == UnitEnum.King.ToString())
        });

        character.SetPlayerPosition(targetPos);
        character.SetTargetPosition(targetPos);
        cameraController.UpdateCameraPosition(character.transform);

        playerCharacters = new List<PlayerCharacterController>()
        {
            character,
        };

        SubscribeToCharacterEvents();
    }

    public void ChangeCharacters(int characterId)
    {
        character = playerCharacters[characterId - 1];
        character.OnPlayerMoveUpdateCameraPosition += UpdateCameraPosition;
        cameraController.UpdateCameraPosition(character.transform);
    }

    public Vector3Int GetCharacterPosition(int characterIndex = 0)
    {
        return playerCharacters[characterIndex - 1].characterPosition;
    }

    public void SetCharacterPosition(Vector3Int newPosition, int characterIndex = 0)
    {
        playerCharacters[characterIndex - 1].characterPosition = newPosition;
    }

    public PlayerCharacterController GetCurrentPlayerCharacter()
        => character;

    public List<PlayerCharacterController> GetPlayerCharacters()
        => playerCharacters;

    public void SetCharacterPath(List<Vector3> positions, List<Vector3Int> tilesPositions, int characterIndex = 0)
        => character.SetPath(positions, tilesPositions);

    public PlayerCharacterController GetCharacterById(int id)
    {
        return playerCharacters.Single(o => o.characterId == id);
    }

    public Vector3 GetPlayerPositionById(int characterId)
        => playerCharacters[characterId - 1].characterPosition;

    public void HandleAddResourcesFromCity(City city)
    {
        if (city.capturerId != playerId) return;

        foreach (var building in city.cityBuildings)
        {
            switch (building.producedResource)
            {
                case WorldResource.Wood:
                    {
                        playerResources.Wood += (int)building.producedAmount;
                        break;
                    }
                case WorldResource.Stone:
                    {
                        playerResources.Stone += (int)building.producedAmount;
                        break;
                    }
                case WorldResource.Gold:
                    {
                        playerResources.Gold += (int)building.producedAmount;
                        break;
                    }
                case WorldResource.LifeResing:
                    {
                        playerResources.LifeResin += (int)building.producedAmount;
                        break;
                    }
            }
        }

    }

    public void HandleAddResources(WorldBuilding building)
    {
        if (building.capturerId != playerId) return;

        switch (building.buildingResource)
        {
            case WorldResource.Wood:
                {
                    playerResources.Wood += building.ResourceAmount;
                    break;
                }
            case WorldResource.Stone:
                {
                    playerResources.Stone += building.ResourceAmount;
                    break;
                }
            case WorldResource.Gold:
                {
                    playerResources.Gold += building.ResourceAmount;
                    break;
                }
            case WorldResource.LifeResing:
                {
                    playerResources.LifeResin += building.ResourceAmount;
                    break;
                }
        }
    }

    public List<City> GetPlayerCities()
    {
        var allCities = FindObjectsByType<City>(FindObjectsSortMode.InstanceID);
        return allCities.Where(o => o.capturerId == playerId).ToList();
    }
}

public static class PlayerEvents
{
    public static Action<PlayerCharacterController> OnPlayerBeginMove;

    public static Action<PlayerCharacterController> OnPlayerEndMove;
}

public class PlayerResources
{
    public int Wood;

    public int Stone;

    public int Gold;

    public int LifeResin;
}

public enum WorldResource
{
    Wood = 1,
    Stone = 2,
    Gold = 3,
    LifeResing = 4
}