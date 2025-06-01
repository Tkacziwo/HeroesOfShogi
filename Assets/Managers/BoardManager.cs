using System;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [SerializeField] private FileManager fileManager;

    private List<MoveInfo> previousMoves;

    GridGame gameGrid;

    void Start()
    {
        previousMoves = new();
        gameGrid = FindFirstObjectByType<GridGame>();
    }

    public List<Tuple<int, int>> CalculatePossibleMoves(Piece piece, bool unrestricted = false)
    {
        var moveset = piece.GetMoveset();
        var pos = piece.GetPositionClass();
        var isBlack = piece.GetIsBlack();
        List<Tuple<int, int>> possibleMoves = new();
        int row = 1;
        int col = -1;
        for (int i = 1; i <= 9; i++)
        {
            //Regular pieces
            if (moveset[i - 1] == 1)
            {
                int destX = col + pos.x;
                int destY = row + pos.y;
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
                    destY = row - 1 + pos.y;
                }
                else
                {
                    destY = row + 1 + pos.y;
                }
                int destX = col + pos.x;
                if (IsInBoard(destX, destY)
                    && (IsCellFree(destX, destY) || IsEnemy(destX, destY, isBlack)))
                {
                    possibleMoves.Add(new(destX, destY));
                }
            }
            //Special pieces: Rook, Bishop
            else if (moveset[i - 1] == 2)
            {
                ExtendSpecialPiecePossibleMoves(row, col, pos, unrestricted, ref possibleMoves, isBlack);
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
        List<Tuple<int, int>> firstCopy = new(first);
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
                        firstCopy.Remove(overlappingMoves[j]);
                    }
                }
            }

            return firstCopy;
        }
    }


    public void ExtendSpecialPiecePossibleMoves(
        int row,
        int col,
        Position pos,
        bool unrestricted,
        ref List<Tuple<int, int>> possibleMoves,
        bool isBlack = false)
    {
        Position destPos = new(col + pos.x, row + pos.y);
        if (unrestricted)
        {
            while (true)
            {
                if (IsInBoard(destPos))
                {
                    possibleMoves.Add(new(destPos.x, destPos.y));

                    destPos.x += col;
                    destPos.y += row;
                }
                else
                {
                    break;
                }
            }
        }
        else
        {
            while (true)
            {
                if (IsInBoard(destPos) && IsCellFree(destPos.x, destPos.y, isBlack))
                {
                    possibleMoves.Add(new(destPos.x, destPos.y));

                    if (IsEnemy(destPos.x, destPos.y, isBlack))
                    {
                        break;
                    }

                    destPos.x += col;
                    destPos.y += row;
                }
                else
                {
                    break;
                }
            }
        }
    }

    public List<Tuple<int, int>> CalculatePossibleDrops(Piece piece)
    {
        List<Tuple<int, int>> moves = new();
        if (piece.GetName() == "Pawn")
        {
            List<int> badX = new();

            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    if (!IsCellFree(x, y) && gameGrid.GetPieceInGrid(x, y).GetComponent<Piece>().GetName() == "Pawn")
                    {
                        badX.Add(x);
                    }
                }
            }

            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    if (IsCellFree(x, y) && !badX.Contains(x))
                    {
                        moves.Add(new(x, y));
                    }
                }
            }
        }
        else
        {
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
        }
        return moves;
    }

    public bool IsCellFree(int destX, int destY, bool isBlack)
    {
        var cell = gameGrid.gameGrid[destX, destY].GetComponent<GridCell>();

        return cell.objectInThisGridSpace != null
            && cell.objectInThisGridSpace.GetComponent<Piece>().GetIsBlack() == isBlack
            ? false
            : true;
    }

    public bool IsCellFree(int destX, int destY)
        => gameGrid.GetGridCell(destX, destY).objectInThisGridSpace == null;

    public bool IsCellFree(Position pos)
        => gameGrid.GetGridCell(pos).objectInThisGridSpace == null;

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

    public bool IsEnemy(Position pos, bool isBlack)
    {
        var cell = gameGrid.GetGridCell(pos);
        if (cell.objectInThisGridSpace != null)
        {
            var pieceColorInDestination = gameGrid.GetGridCell(pos)
                .objectInThisGridSpace.GetComponent<Piece>().GetIsBlack();
            return pieceColorInDestination != isBlack;
        }
        else
        {
            return false;
        }
    }

    public bool IsInBoard(int row, int col)
    {
        if (row > -1 && row < 9 && col > -1 && col < 9)
            return true;
        else
            return false;
    }

    public bool IsInBoard(Position p)
        => p.y > -1 && p.y < 9 && p.x > -1 && p.x < 9;

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
                piece.BackupOriginalMoveset(piece.GetMoveset());
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

    public void RegisterMove(Tuple<Tuple<int, int>, Tuple<int, int>> sourceDestination, bool isDrop)
    {

        MoveInfo i = new(sourceDestination, isDrop);
        previousMoves.Add(i);
    }

    public MoveInfo UndoMove()
    {
        int top = previousMoves.Count - 1;
        var move = previousMoves[top];
        previousMoves.RemoveAt(top);
        return move;
    }
}