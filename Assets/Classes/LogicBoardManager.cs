using System.Collections.Generic;

/// <summary>
/// Logic counterpart to BoardManager. Behaves the same.
/// </summary>
public class LogicBoardManager
{
    public List<Position> CalculatePossibleMovesInverted(LogicPiece piece, LogicCell[,] cells)
    {
        var moveset = piece.GetMoveset();
        var pos = piece.GetPosition();
        var isBlack = piece.GetIsBlack();
        isBlack = !isBlack;
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
                ExtendSpecialPiecePossibleMovesInverted(row, col, pos, ref possibleMoves, isBlack, cells);
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

    public List<Position> CalculatePossibleMoves(
        LogicPiece piece,
        LogicCell[,] cells,
        bool unrestricted = false)

    {
        var moveset = piece.GetMoveset();
        var pos = piece.GetPosition();
        var isBlack = piece.GetIsBlack();
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

    public void ExtendSpecialPiecePossibleMoves(
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

    public void ExtendSpecialPiecePossibleMovesInverted(
        int row,
        int col,
        Position pos,
        ref List<Position> possibleMoves,
        bool isBlack,
        LogicCell[,] cells)
    {
        Position destPos = new(col + pos.x, row + pos.y);
        while (true)
        {
            if (IsInBoard(destPos.x, destPos.y))
            {
                if (IsCellFree(destPos.x, destPos.y, cells))
                {
                    possibleMoves.Add(new(destPos));
                }
                else
                {
                    if (IsEnemy(destPos.x, destPos.y, isBlack, cells))
                    {
                        possibleMoves.Add(new(destPos));
                        break;
                    }
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

    public void CheckIfMovesAreLegal(ref List<Position> pMoves, LogicPiece piece, List<LogicPiece> allPieces, LogicCell[,] cells)
    {
        LogicPiece king = new();

        List<LogicPiece> enemyPieces = new();
        foreach (var p in allPieces)
        {
            if (!p.GetIsBlack())
            {
                enemyPieces.Add(p);
            }

            if (p.GetIsBlack() && p.isKing)
            {
                king = p;
            }
        }


        List<LogicPiece> specialEnemyPieces = new();
        foreach (var p in enemyPieces)
        {
            if (p.GetName() == "Lance" || p.GetName() == "Bishop" || p.GetName() == "Rook")
            {
                specialEnemyPieces.Add(p);
            }
        }

        var piecePosition = piece.GetPosition();
        var kingPosition = king.GetPosition();
        foreach (var enemy in specialEnemyPieces)
        {
            var enemyPosition = enemy.GetPosition();
            var enemyPMoves = CalculatePossibleMovesWithDirection(enemy, cells);
            var enemyPMovesUnrestricted = CalculatePossibleMovesWithDirection(enemy, cells, true);

            bool canKill = pMoves.Contains(enemyPosition);

            if (enemyPMoves.North.Contains(piecePosition) && enemyPMovesUnrestricted.North.Contains(kingPosition))
            {
                if (CalculateSameColorPiecesInDirection(enemyPMovesUnrestricted.North, piece.GetIsBlack(), cells) == 1)
                    pMoves = CalculateOverlappingMoves(pMoves, enemyPMovesUnrestricted.North, true);
            }
            else if (enemyPMoves.East.Contains(piecePosition) && enemyPMovesUnrestricted.East.Contains(kingPosition))
            {
                if (CalculateSameColorPiecesInDirection(enemyPMovesUnrestricted.East, piece.GetIsBlack(), cells) == 1)
                    pMoves = CalculateOverlappingMoves(pMoves, enemyPMovesUnrestricted.East, true);
            }
            else if (enemyPMoves.South.Contains(piecePosition) && enemyPMovesUnrestricted.South.Contains(kingPosition))
            {
                if (CalculateSameColorPiecesInDirection(enemyPMovesUnrestricted.South, piece.GetIsBlack(), cells) == 1)
                    pMoves = CalculateOverlappingMoves(pMoves, enemyPMovesUnrestricted.South, true);
            }
            else if (enemyPMoves.West.Contains(piecePosition) && enemyPMovesUnrestricted.West.Contains(kingPosition))
            {
                if (CalculateSameColorPiecesInDirection(enemyPMovesUnrestricted.West, piece.GetIsBlack(), cells) == 1)
                    pMoves = CalculateOverlappingMoves(pMoves, enemyPMovesUnrestricted.West, true);
            }
            else if (enemyPMoves.North_West.Contains(piecePosition) && enemyPMovesUnrestricted.North_West.Contains(kingPosition))
            {
                if (CalculateSameColorPiecesInDirection(enemyPMovesUnrestricted.North_West, piece.GetIsBlack(), cells) == 1)
                    pMoves = CalculateOverlappingMoves(pMoves, enemyPMovesUnrestricted.North_West, true);
            }
            else if (enemyPMoves.North_East.Contains(piecePosition) && enemyPMovesUnrestricted.North_East.Contains(kingPosition))
            {
                if (CalculateSameColorPiecesInDirection(enemyPMovesUnrestricted.North_East, piece.GetIsBlack(), cells) == 1)
                    pMoves = CalculateOverlappingMoves(pMoves, enemyPMovesUnrestricted.North_East, true);
            }
            else if (enemyPMoves.South_West.Contains(piecePosition) && enemyPMovesUnrestricted.South_West.Contains(kingPosition))
            {
                if (CalculateSameColorPiecesInDirection(enemyPMovesUnrestricted.South_West, piece.GetIsBlack(), cells) == 1)
                    pMoves = CalculateOverlappingMoves(pMoves, enemyPMovesUnrestricted.South_West, true);
            }
            else if (enemyPMoves.South_East.Contains(piecePosition) && enemyPMovesUnrestricted.South_East.Contains(kingPosition))
            {
                if (CalculateSameColorPiecesInDirection(enemyPMovesUnrestricted.South_East, piece.GetIsBlack(), cells) == 1)
                    pMoves = CalculateOverlappingMoves(pMoves, enemyPMovesUnrestricted.South_East, true);
            }

            if (canKill)
            {
                pMoves.Add(enemy.GetPosition());
            }
        }
    }

    public int CalculateSameColorPiecesInDirection(List<Position> movesLine, bool isBlack, LogicCell[,] cells)
    {
        var sum = 0;
        for (int i = 0; i < movesLine.Count; i++)
        {
            if (!IsCellFree(movesLine[i].x, movesLine[i].y, cells))
            {
                var pieceInCell = cells[movesLine[i].x, movesLine[i].y].piece;

                if (!pieceInCell.isKing && pieceInCell.GetIsBlack() == isBlack)
                {
                    sum++;
                }
            }
        }
        return sum;
    }

    public DirectionPossibleMoves CalculatePossibleMovesWithDirection(LogicPiece piece, LogicCell[,] cells, bool unrestricted = false)
    {
        DirectionPossibleMoves directionPossibleMoves = new();
        var moveset = piece.GetMoveset();
        var pos = piece.GetPosition();
        var isBlack = piece.GetIsBlack();
        List<Position> possibleMoves = new();
        int row = 1;
        int col = -1;
        for (int i = 1; i <= 9; i++)
        {
            if (moveset[i - 1] == 2)
            {
                ExtendSpecialPiecePossibleMoves(row, col, pos, unrestricted, ref possibleMoves, cells, isBlack);


                switch (row, col)
                {
                    case (1, 0):
                        {
                            directionPossibleMoves.North.AddRange(possibleMoves);
                            possibleMoves.Clear();
                            break;
                        }
                    case (0, 1):
                        {
                            directionPossibleMoves.East.AddRange(possibleMoves);
                            possibleMoves.Clear();
                            break;
                        }
                    case (-1, 0):
                        {
                            directionPossibleMoves.South.AddRange(possibleMoves);
                            possibleMoves.Clear();
                            break;
                        }
                    case (0, -1):
                        {
                            directionPossibleMoves.West.AddRange(possibleMoves);
                            possibleMoves.Clear();
                            break;
                        }
                    case (1, -1):
                        {
                            directionPossibleMoves.North_West.AddRange(possibleMoves);
                            possibleMoves.Clear();
                            break;
                        }
                    case (1, 1):
                        {
                            directionPossibleMoves.North_East.AddRange(possibleMoves);
                            possibleMoves.Clear();
                            break;
                        }
                    case (-1, -1):
                        {
                            directionPossibleMoves.South_West.AddRange(possibleMoves);
                            possibleMoves.Clear();
                            break;
                        }
                    case (-1, 1):
                        {
                            directionPossibleMoves.South_East.AddRange(possibleMoves);
                            possibleMoves.Clear();
                            break;
                        }
                }
            }
            col++;
            if (i % 3 == 0 && i != 0)
            {
                row--;
                col = -1;
            }
        }
        return directionPossibleMoves;
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

    public List<Position> ScanMovesUnrestricted(LogicPiece piece)
    {
        var moveset = piece.GetMoveset();
        var isBlack = piece.GetIsBlack();
        var pos = piece.GetPosition();
        List<Position> possibleMoves = new();
        int row = 1;
        int col = -1;
        for (int i = 1; i <= 9; i++)
        {
            //Regular pieces
            if (moveset[i - 1] == 1)
            {
                Position destPos = new(col + pos.x, row + pos.y);
                if (IsInBoard(destPos.x, destPos.y))
                {
                    possibleMoves.Add(new(destPos.x, destPos.y));
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
                if (IsInBoard(destX, destY))
                {
                    possibleMoves.Add(new(destX, destY));
                }
            }
            //Special pieces: Rook, Bishop
            else if (moveset[i - 1] == 2)
            {
                ExtendSpecialPiecePossibleMovesUnrestricted(row, col, pos, ref possibleMoves);
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

    public void ExtendSpecialPiecePossibleMovesUnrestricted(int row, int col, Position pos, ref List<Position> possibleMoves)
    {
        Position destPos = new(col + pos.x, row + pos.y);
        while (true)
        {
            if (IsInBoard(destPos.x, destPos.y))
            {
                possibleMoves.Add(new(destPos));

                destPos.x += col;
                destPos.y += row;
            }
            else
            {
                break;
            }
        }
    }

    public List<Position> MultiplyDropsByWeight(List<Position> moves)
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
                    if (!IsCellFree(x, y, cells))
                    {
                        var gridPiece = cells[x, y].piece;
                        if (gridPiece.GetName() == "Pawn" && gridPiece.GetIsBlack() != piece.GetIsBlack())
                        {
                            badX.Add(x);
                        }
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

    public void ApplyPromotion(LogicPiece piece)
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
                //piece.BackupOriginalMoveset(piece.GetMoveset());
                int[] moveset = piece.GetMoveset();
                int[] originalMoveset = piece.GetOriginalMoveset();

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
                piece.Promote(moveset);
            }
        }
    }
}