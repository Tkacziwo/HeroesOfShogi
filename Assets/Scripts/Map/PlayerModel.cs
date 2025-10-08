using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerModel : MonoBehaviour
{
    public int playerId;

    public bool isRealPlayer;

    public Color playerColor;

    private PlayerCharacterController character;

    [SerializeField] private GameObject playerModel;

    public PlayerResources playerResources;

    public void PlayerBeginMove()
        => PlayerEvents.OnPlayerBeginMove?.Invoke(this);

    public void SpawnPlayer(int playerId)
    {
        this.playerId = playerId;
        isRealPlayer = true;
        var p = Instantiate(playerModel);
        character = p.GetComponent<PlayerCharacterController>();
        var vec = new Vector3Int(12, 1, 0);
        playerResources = new();
        character.SetPlayerPosition(vec);
    }
    public Vector3Int GetCharacterPosition(int characterIndex = 0)
        => character.characterPosition;

    public void SetCharacterPosition(Vector3Int newPosition, int characterIndex = 0)
        => character.characterPosition = newPosition;

    public void SetCharacterPath(List<Vector3> positions, List<Vector3Int> tilesPositions, int characterIndex = 0)
        => character.SetPath(positions, tilesPositions);

    public Vector3 GetPlayerPosition()
        => character.transform.position;

    public void HandleAddResources(InteractibleBuilding building)
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

        Debug.Log("Player resources: ");
        Debug.Log("Wood: " + playerResources.Wood);
        Debug.Log("Stone: " + playerResources.Stone);
        Debug.Log("Gold: " + playerResources.Gold);
        Debug.Log("LifeResin: " + playerResources.LifeResin);
    }
}

public static class PlayerEvents
{
    public static Action<PlayerModel> OnPlayerBeginMove;
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