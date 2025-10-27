using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.UIElements;

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

    private void OnDisable()
    {
        if (character != null) character.OnPlayerMoveUpdateCameraPosition -= UpdateCameraPosition;
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
        => PlayerEvents.OnPlayerBeginMove?.Invoke(this);

    public void InitPlayer(int playerId)
    {
        cameraController = this.GetComponentInChildren<CameraController>();
        cameraController.InitCamera();
        this.playerId = playerId;
        isRealPlayer = true;
        playerResources = new();
        cameraController.isCameraFocusedOnPlayer = true;
    }

    public void SpawnPlayer()
    {
        var p = Instantiate(playerModel);

        character = p.GetComponent<PlayerCharacterController>();
        character.playerId = this.playerId;
        character.playerColor = playerColor;
        character.characterId = 1;
        character.movementPoints = 20;

        var vec = new Vector3Int(12, 1, 0);
        character.SetPlayerPosition(vec);
        cameraController.UpdateCameraPosition(character.transform);

        var p2 = Instantiate(playerModel);
        var character2 = p2.GetComponent<PlayerCharacterController>();
        var vec2 = new Vector3Int(1, 8, 0);
        character2.playerId = this.playerId;
        character2.playerColor = playerColor;
        character2.characterId = 2;
        character2.movementPoints = 20;
        character2.SetPlayerPosition(vec2);

        playerCharacters = new List<PlayerCharacterController>()
        {
            character,
            character2
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

    public void SetCharacterPath(List<Vector3> positions, List<Vector3Int> tilesPositions, int characterIndex = 0)
        => character.SetPath(positions, tilesPositions);

    public Vector3 GetPlayerPosition()
        => character.characterPosition;

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
}

public static class PlayerEvents
{
    public static Action<PlayerModel> OnPlayerBeginMove;

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