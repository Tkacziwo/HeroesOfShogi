using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private PlayerCharacterController character;

    public static Action<PlayerController> onPlayerBeginMove;

    [SerializeField] private GameObject playerModel;

    public PlayerResources playerResources;

    private void OnEnable()
    {
        DoubleClickHandler.OnDoubleClick += (DoubleClickHandler handler) => onPlayerBeginMove?.Invoke(this);
    }

    private void OnDisable()
    {
        DoubleClickHandler.OnDoubleClick -= (DoubleClickHandler handler) => onPlayerBeginMove?.Invoke(this);
    }

    public void SpawnPlayer()
    {
        var p = Instantiate(playerModel);
        character = p.GetComponent<PlayerCharacterController>();
        var vec = new Vector3Int(12, 1, 0);
        character.SetPlayerPosition(vec);
    }
    public Vector3Int GetCharacterPosition(int characterIndex = 0)
        => character.characterPosition;

    public void SetCharacterPosition(Vector3Int newPosition,  int characterIndex = 0)
        => character.characterPosition = newPosition;

    public void SetCharacterPath(List<Vector3> positions, List<Vector3Int> tilesPositions, int characterIndex = 0)
        => character.SetPath(positions, tilesPositions);

    public Vector3 GetPlayerPosition()
        => character.transform.position;

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            onPlayerBeginMove?.Invoke(this);
        }
    }
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