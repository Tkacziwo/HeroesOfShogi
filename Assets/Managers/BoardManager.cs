using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using UnityEditor.XR;
using UnityEngine;
using UnityEngine.Timeline;
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

    public List<Tuple<int, int>> CalculatePossibleMoves(Vector2Int piecePos, int[] moveset, bool isBlack)
    {
        List<Tuple<int, int>> possibleMoves = new();
        int row = 1;
        int col = -1;
        for (int i = 1; i <= 9; i++)
        {
            if (moveset[i - 1] == 1)
            {
                int destX = col + piecePos.x;
                int destY = row + piecePos.y;
                if (IsInBoard(destX, destY) && IsCellFree(destX, destY, isBlack))
                {
                    possibleMoves.Add(new(destX, destY));
                }
            }
            else if (moveset[i - 1] == 2)
            {
                ExtendSpecialPiecePossibleMoves(row, col, piecePos, isBlack, ref possibleMoves);
            }
            else if (moveset[i - 1] == 3)
            {
                int destX = col + piecePos.x;
                int destY = row + 1 + piecePos.y;
                if (IsInBoard(destX, destY) && IsCellFree(destX, destY, isBlack))
                {
                    possibleMoves.Add(new(destX, destY));
                }
            }
            col++;
            if (i % 3 == 0 && i != 0)
            {
                row--;
                col = -1;
            }
        }
        return possibleMoves;
    }

    public void ExtendSpecialPiecePossibleMoves(int row, int col, Vector2Int piecePos,
                                                bool isBlack, ref List<Tuple<int, int>> possibleMoves)
    {
        int destX = col + piecePos.x;
        int destY = row + piecePos.y;
        while (true)
        {
            if (IsInBoard(destX, destY) && IsCellFree(destX, destY, isBlack))
            {
                possibleMoves.Add(new(destX, destY));
                destX += col;
                destY += row;
            }
            else
            {
                break;
            }
        }
    }

    public bool IsCellFree(int destX, int destY, bool isBlack)
    {
        var cell = gameGrid.gameGrid[destX, destY].GetComponent<GridCell>();

        return cell.objectInThisGridSpace != null
            && cell.objectInThisGridSpace.GetComponent<Piece>().GetIsBlack() == isBlack
            ? false
            : true;
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
