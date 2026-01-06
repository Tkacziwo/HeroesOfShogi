using System.Collections.Generic;

/// <summary>
/// Contains operations used on board during battle.
/// </summary>
public static class BoardOperations
{
    public static List<Position> CalculatePossibleMoves(ShogiPiece piece, LogicCell[,] cells, bool unrestricted = false)
    {
        var moveset = piece.GetMoveset();
        var pos = piece.GetPosition();
        var isBlack = piece.GetIsBlack();
        List<Position> possibleMoves = new();
        int row = 1;
        int col = -1;

        if (moveset == null || moveset.Length == 0) return new();

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
                int destY = isBlack ? row - 1 + pos.y : row + 1 + pos.y;
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
                ExtendSpecialPiecePossibleMoves(row, col, pos, unrestricted, ref possibleMoves, cells, isBlack);
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

    public static void ExtendSpecialPiecePossibleMoves(
        int row,
        int col,
        Position pos,
        bool unrestricted,
        ref List<Position> possibleMoves,
        LogicCell[,] cells,
        bool isBlack = false)
    {
        Position destPos = new(col + pos.x, row + pos.y);

        if (unrestricted)
        {
            while (true)
            {
                if (IsInBoard(destPos.x, destPos.y))
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

    }

    public static Position FindPositionBeforeEnemy(int row, int col, Position pos, Position target)
    {
        Position found = null;
        int destX = col + pos.x;
        int destY = row + pos.y;

        if (row == 0 && col == 0) return found;

        Position previous = new(destX, destY);
        while (true)
        {
            if (IsInBoard(destX, destY))
            {
                if (target.Equals(new(destX, destY)))
                {
                    found = new(previous);

                    break;
                }

                previous = new(destX, destY);
                destX += col;
                destY += row;
            }
            else
            {
                break;
            }
        }
        return found;
    }

    public static List<Position> MultiplyDropsByWeight(List<Position> moves)
    {
        List<Position> betterDrops = new();
        foreach (var move in moves)
        {
            if ((move.y > 0 && move.y <= 3) || (move.y < 7 && move.y >= 5))
            {
                var random = new System.Random();
                var randomNumber = random.Next(0, 2);
                if (randomNumber == 0)
                {
                    betterDrops.Add(move);
                }
            }
            else if (move.y > 3 && move.y < 5)
            {
                betterDrops.Add(move);
            }
        }

        return betterDrops;
    }

    public static List<Position> CalculatePossibleDrops(Unit unit, LogicCell[,] cells)
    {
        List<Position> moves = new();

        if (unit.GetName() == "Pawn")
        {
            List<int> badX = new();
            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    if (!IsCellFree(x, y, cells))
                    {
                        var gridPiece = cells[x, y].unit;
                        if (gridPiece.GetName() == "Pawn" && gridPiece.GetIsBlack() == unit.GetIsBlack())
                        {
                            badX.Add(x);
                        }
                    }
                }
            }

            if (unit.GetIsBlack())
            {
                for (int y = 2; y < 9; y++)
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
                for (int y = 0; y < 7; y++)
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
        }
        else
        {
            for (int y = 2; y < 7; y++)
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

    public static bool IsCellFree(int destX, int destY, bool isBlack, LogicCell[,] cells)
    {
        var cell = cells[destX, destY];

        if (cell.unit != null
            && cell.unit.GetIsBlack() == isBlack)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public static bool IsCellFree(int destX, int destY, LogicCell[,] cells)
    {
        var cell = cells[destX, destY];
        return cell.unit == null;
    }

    public static bool IsEnemy(int destX, int destY, bool isBlack, LogicCell[,] cells)
    {
        var cell = cells[destX, destY];
        if (cell.unit != null)
        {
            var pieceColorInDestination = cell.unit.GetIsBlack();
            return pieceColorInDestination != isBlack;
        }
        else
        {
            return false;
        }
    }

    public static bool IsInBoard(int row, int col)
    {
        if (row > -1 && row < StaticData.battleMapHeight && col > -1 && col < StaticData.battleMapWidth)
            return true;
        else
            return false;
    }
    public static int[] GetPromotedUnitMoveset(Unit unit)
    {
        if (unit.UnitName == UnitEnum.King) return null;

        if (unit.UnitName == UnitEnum.Bishop || unit.UnitName == UnitEnum.Rook)
        {
            //piece.BackupOriginalMoveset(piece.GetMoveset());
            int[] moveset = unit.GetMoveset();
            int[] originalMoveset = unit.GetOriginalMoveset();

            for (int i = 0; i < moveset.Length; i++)
            {
                moveset[i] = originalMoveset[i];
            }

            for (int i = 0; i < 9; i++)
            {
                if (moveset[i] != 2 && i != 4)
                {
                    moveset[i]++;
                }
            }
            return moveset;
        }
        else
        {
            int[] gg = { 1, 1, 1, 1, 0, 1, 0, 1, 0 };
            return gg;
        }
    }

    public static bool IsKingThreatened(Position kingPos, List<Unit> units, LogicCell[,] cells)
    {
        foreach (var unit in units)
        {
            var moves = CalculatePossibleMoves(unit, cells);

            if (moves.Contains(kingPos))
            {
                return true;
            }
        }

        return false;
    }
}