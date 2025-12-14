using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRegistry : MonoBehaviour
{
    public List<PlayerModel> players = new();

    public static PlayerRegistry Instance { get; set; }

    public int NumberOfPlayers { get; set; } = 0;

    public static Action OnPlayersLoaded;

    private void Awake()
    {
        Instance = this;
    }

    public void Register(PlayerModel b)
    {
        if (!players.Contains(b))
        {
            players.Add(b);
            CheckPlayersLoaded();
        }
    }

    public void Unregister(PlayerModel b)
    {
        players.Remove(b);
    }

    public List<PlayerModel> GetAllPlayers()
    {
        return players;
    }

    public void CheckPlayersLoaded()
    {
        if (NumberOfPlayers == players.Count)
        {
            OnPlayersLoaded?.Invoke();
        }
    }
}