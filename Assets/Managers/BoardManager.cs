using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [SerializeField] private FileManager fileManager;

    private List<MoveInfo> previousMoves;

    GridGame gameGrid;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        previousMoves = new();
        gameGrid = FindFirstObjectByType<GridGame>();
    }

    public List<Tuple<int, int>> CalculatePossibleMoves(Vector2Int piecePos, int[] moveset, bool isBlack)
    {
        List<Tuple<int, int>> possibleMoves = new();
        int row = 1;
        int col = -1;
        for (int i = 1; i <= 9; i++)
        {
            //Regular pieces
            if (moveset[i - 1] == 1)
            {
                int destX = col + piecePos.x;
                int destY = row + piecePos.y;
                if (IsInBoard(destX, destY)
                    && (IsCellFree(destX, destY) || IsEnemy(destX, destY, isBlack)))
                {
                    possibleMoves.Add(new(destX, destY));
                }
            }
            //Horse
            else if (moveset[i - 1] == 3)
            {
                int destY;
                if (isBlack)
                {
                    destY = row - 1 + piecePos.y;
                }
                else
                {
                    destY = row + 1 + piecePos.y;
                }
                int destX = col + piecePos.x;
                if (IsInBoard(destX, destY)
                    && (IsCellFree(destX, destY) || IsEnemy(destX, destY, isBlack)))
                {
                    possibleMoves.Add(new(destX, destY));
                }
            }
            //Special pieces: Rook, Bishop
            else if (moveset[i - 1] == 2)
            {
                ExtendSpecialPiecePossibleMoves(row, col, piecePos, isBlack, ref possibleMoves);
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

    public List<Tuple<int, int>> CalculateOverlappingMoves(List<Tuple<int, int>> first, List<Tuple<int, int>> comparer, bool overlap)
    {
        List<Tuple<int, int>> overlappingMoves = new();
        for (int i = 0; i < first.Count; i++)
        {
            for (int j = 0; j < comparer.Count; j++)
            {
                if (first[i].Equals(comparer[j]))
                {
                    overlappingMoves.Add(first[i]);
                }
            }
        }
        if (overlap)
        {
            return overlappingMoves;
        }
        else
        {
            for (int i = 0; i < first.Count; i++)
            {
                for (int j = 0; j < overlappingMoves.Count; j++)
                {
                    if (first[i].Equals(overlappingMoves[j]))
                    {
                        first.Remove(overlappingMoves[j]);
                    }
                }
            }

            return first;
        }
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

                if (IsEnemy(destX, destY, isBlack))
                {
                    break;
                }

                destX += col;
                destY += row;
            }
            else
            {
                break;
            }
        }
    }


    public List<Tuple<int, int>> CalculatePossibleDrops()
    {
        List<Tuple<int, int>> moves = new();
        for (int y = 0; y < 9; y++)
        {
            for (int x = 0; x < 9; x++)
            {
                if (IsCellFree(x, y))
                {
                    moves.Add(new(x, y));
                }
            }
        }
        return moves;
    }

    public bool IsCellFree(int destX, int destY, bool isBlack)
    {
        var cell = gameGrid.gameGrid[destX, destY].GetComponent<GridCell>();

        //if (cell.objectInThisGridSpace != null 
        //    && cell.objectInThisGridSpace.GetComponent<Piece>().GetIsBlack() == isBlack)
        //{
        //    return false;
        //}
        //else
        //{
        //    return true;
        //}

        return cell.objectInThisGridSpace != null
            && cell.objectInThisGridSpace.GetComponent<Piece>().GetIsBlack() == isBlack
            ? false
            : true;
    }

    public bool IsCellFree(int destX, int destY)
    {
        var objectInCell = gameGrid.gameGrid[destX, destY].GetComponent<GridCell>().objectInThisGridSpace;
        return objectInCell == null;
    }

    public bool IsEnemy(int destX, int destY, bool isBlack)
    {
        var cell = gameGrid.gameGrid[destX, destY].GetComponent<GridCell>();
        if (cell.objectInThisGridSpace != null)
        {
            var pieceColorInDestination = gameGrid.gameGrid[destX, destY].GetComponent<GridCell>()
                .objectInThisGridSpace.GetComponent<Piece>().GetIsBlack();
            return pieceColorInDestination != isBlack;
        }
        else
        {
            return false;
        }
    }

    public bool IsCellNotFreeAndContainsEnemy(int destX, int destY, bool isBlack)
    {
        var cell = gameGrid.GetGridCell(destX, destY);
        if (cell.objectInThisGridSpace == null) return false;
        else
        {
            var piece = gameGrid.GetPieceInGrid(destX, destY).GetComponent<Piece>();
            return piece.GetIsBlack() != isBlack;
        }
    }

    public bool IsInBoard(int row, int col)
    {
        if (row > -1 && row < 9 && col > -1 && col < 9)
            return true;
        else
            return false;
    }

    public void ApplyPromotion(Piece piece)
    {
        if (!piece.isKing)
        {
            if (!piece.GetIsSpecial())
            {
                piece.Promote(fileManager.GetMovesetByPieceName("GoldGeneral"));
            }
            else
            {
                piece.BackupOriginalMoveset();
                int[] moveset = piece.GetMoveset();
                for (int i = 0; i < 9; i++)
                {
                    if (moveset[i] != 2 && i != 4)
                    {
                        moveset[i]++;
                    }
                }
                piece.Promote(moveset);
            }
        }
    }

    public void HandleRegisterMove(Tuple<MoveInfo, MoveInfo> sourceDestination, bool isDrop)
    {
    }

    public void RegisterMove(Tuple<Tuple<int, int>, Tuple<int, int>> sourceDestination, bool isDrop)
    {

        MoveInfo i = new(sourceDestination, isDrop);
        previousMoves.Add(i);
    }

    public MoveInfo UndoMove()
    {
        int top = previousMoves.Count - 1;
        var move =  previousMoves[top];
        previousMoves.RemoveAt(top);
        return move;
    }
}