using System.Collections.Generic;
using System;

public class LogicBoardManager
{
    public List<Position> CalculatePossibleMoves(Position pos,
        int[] moveset, bool isBlack, LogicCell[,] cells)
    {
        List<Position> possibleMoves = new();
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
                    && (IsCellFree(destX, destY, cells) || IsEnemy(destX, destY, isBlack, cells)))
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
                    && (IsCellFree(destX, destY, cells) || IsEnemy(destX, destY, isBlack, cells)))
                {
                    possibleMoves.Add(new(destX, destY));
                }
            }
            //Special pieces: Rook, Bishop
            else if (moveset[i - 1] == 2)
            {
                ExtendSpecialPiecePossibleMoves(row, col, pos, isBlack, ref possibleMoves, cells);
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

    public List<Position> CalculateOverlappingMoves(List<Position> first, List<Position> comparer, bool overlap)
    {
        List<Position> overlappingMoves = new();
        List<Position> firstCopy = new(first);
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


    public void ExtendSpecialPiecePossibleMoves(int row, int col, Position pos,
                                                bool isBlack, ref List<Position> possibleMoves, LogicCell[,] cells)
    {
        int destX = col + pos.x;
        int destY = row + pos.y;
        while (true)
        {
            if (IsInBoard(destX, destY) && IsCellFree(destX, destY, isBlack, cells))
            {
                possibleMoves.Add(new(destX, destY));

                if (IsEnemy(destX, destY, isBlack, cells))
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


    public List<Position> CalculatePossibleDrops(LogicCell[,] cells, LogicPiece piece)
    {
        List<Position> moves = new();

        if (piece.GetName() == "Pawn")
        {
            List<int> badX = new();


            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    if (!IsCellFree(x, y, cells) && cells[x, y].piece.GetName() == "Pawn")
                    {
                        badX.Add(x);
                    }
                }
            }

            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    if (IsCellFree(x, y, cells) && !badX.Contains(x))
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
                    if (IsCellFree(x, y, cells))
                    {
                        moves.Add(new(x, y));
                    }
                }
            }
        }
        return moves;
    }

    public bool IsCellFree(int destX, int destY, bool isBlack, LogicCell[,] cells)
    {
        var cell = cells[destX, destY];

        if (cell.piece != null
            && cell.piece.GetIsBlack() == isBlack)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public bool IsCellFree(int destX, int destY, LogicCell[,] cells)
    {
        var cell = cells[destX, destY];
        return cell.piece == null;
    }

    public bool IsEnemy(int destX, int destY, bool isBlack, LogicCell[,] cells)
    {
        var cell = cells[destX, destY];
        if (cell.piece != null)
        {
            var pieceColorInDestination = cell.piece.GetIsBlack();
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

    public void ApplyPromotion(Piece piece)
    {
        if (!piece.isKing)
        {
            if (!piece.GetIsSpecial())
            {
                int[] gg = { 1, 1, 1, 1, 0, 1, 0, 1, 0 };
                piece.Promote(gg);
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
}