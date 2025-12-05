using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRegistry : MonoBehaviour
{
    public List<PlayerModel> players = new();

    public static PlayerRegistry Instance { get; set; }

    private void Awake()
    {
        Instance = this;
    }

    public void Register(PlayerModel b)
    {
        if (!players.Contains(b))
            players.Add(b);
    }

    public void Unregister(PlayerModel b)
    {
        players.Remove(b);
    }

    public List<PlayerModel> GetAllPlayers()
    {
        return players;
    }
}