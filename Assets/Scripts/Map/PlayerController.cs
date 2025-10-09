using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public PlayerModel player;

    public int currentPlayer;

    private void OnEnable()
    {
        InteractibleBuilding.AddResourcesToCapturer += HandleAddResources;
        DoubleClickHandler.OnDoubleClick += HandleDoubleClick;
    }

    private void OnDisable()
    {
        InteractibleBuilding.AddResourcesToCapturer -= HandleAddResources;
        DoubleClickHandler.OnDoubleClick -= HandleDoubleClick;
    }

    private void HandleDoubleClick(DoubleClickHandler handler)
    {
        player.PlayerBeginMove();
    }

    private void HandleAddResources(InteractibleBuilding building)
    {
        player.HandleAddResources(building);
    }

    public void SetCharacterPosition(Vector3Int newPosition)
        => player.SetCharacterPosition(newPosition);

    public void SetCharacterPath(List<Vector3> positions, List<Vector3Int> tilesPositions)
        => player.SetCharacterPath(positions, tilesPositions);


    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            player.PlayerBeginMove();
        }
    }
}