using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

public class BoardManager : MonoBehaviour
{
    [SerializeField] private FileManager fileManager;

    GridGame gameGrid;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameGrid = FindFirstObjectByType<GridGame>();
    }

    public IList<Tuple<int, int>> CalculatePossibleMoves(int posX, int posY, int[] moves)
    {
        IList<Tuple<int, int>> possibleMoves = new List<Tuple<int, int>>();
        int row = 1;
        int col = 0;
        for (int i = 1; i <= 9; i++)
        {
            if (moves[i - 1] == 1)
            {
                int destX = col - 1 + posX;
                int destY = row + posY;
                if (IsInBoard(destX, destY))
                {
                    possibleMoves.Add(new(destX, destY));
                }
            }
            col++;
            if (i % 3 == 0 && i != 0)
            {
                row--;
                col = 0;
            }
        }
        return possibleMoves;
    }

    public bool IsInBoard(int row, int col)
    {
        if (row > -1 && row < 9 && col > -1 && col < 9)
            return true;
        else
            return false;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
