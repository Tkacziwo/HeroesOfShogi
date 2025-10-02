using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    [SerializeField] private FileManager fileManager;

    [SerializeField] private Grid gameGrid;

    public List<Position> CalculatePossibleMovesInverted(Piece piece)
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
                    && (IsCellFree(destX, destY) || IsEnemy(destX, destY, isBlack)))
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
                    && (IsCellFree(destX, destY) || IsEnemy(destX, destY, isBlack)))
                {
                    possibleMoves.Add(new(destX, destY));
                }
            }
            //Special pieces: Rook, Bishop
            else if (moveset[i - 1] == 2)
            {
                ExtendSpecialPiecePossibleMovesInverted(row, col, pos, ref possibleMoves, isBlack);
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

    /// <summary>
    /// Calculates possible moves for every piece with moveset.
    /// </summary>
    /// <param name="unrestricted">Whether it should stop at first piece in its path or continue until board dimensions</param>
    /// <returns>List of positions</returns>
    public List<Position> CalculatePossibleMoves(Piece piece, bool unrestricted = false)
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
                if (IsInBoard(destX, destY))
                {
                    if (unrestricted)
                    {
                        possibleMoves.Add(new(destX, destY));
                    }
                    else if (IsCellFree(destX, destY) || IsEnemy(destX, destY, isBlack))
                    {
                        possibleMoves.Add(new(destX, destY));
                    }
                }
            }
            //Horse
            else if (moveset[i - 1] == 3)
            {
                int destY = isBlack ? row - 1 + pos.y : row + 1 + pos.y;
                int destX = col + pos.x;

                if (IsInBoard(destX, destY))
                {
                    if (unrestricted)
                    {
                        possibleMoves.Add(new(destX, destY));
                    }
                    else if (IsCellFree(destX, destY) || IsEnemy(destX, destY, isBlack))
                    {
                        possibleMoves.Add(new(destX, destY));
                    }
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

    /// <summary>
    /// Checks if the provided possible moves are legal with Shogi rules.
    /// </summary>
    /// <param name="pMoves">Possible moves. Passed by reference</param>
    /// <param name="piece">Piece</param>
    public void CheckIfMovesAreLegal(ref List<Position> pMoves, Piece piece)
    {
        var king = piece.GetIsBlack() ? gameGrid.GetBotKing() : gameGrid.GetPlayerKing();
        var enemyPieces = piece.GetIsBlack() ? gameGrid.GetPlayerPieces() : gameGrid.GetBotPieces();
        List<Piece> specialEnemyPieces = new();
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
            var enemyPMoves = CalculatePossibleMovesWithDirection(enemy);
            var enemyPMovesUnrestricted = CalculatePossibleMovesWithDirection(enemy, true);

            bool canKill = pMoves.Contains(enemyPosition);

            if (enemyPMoves.North.Contains(piecePosition) && enemyPMovesUnrestricted.North.Contains(kingPosition))
            {
                if (CalculateSameColorPiecesInDirection(enemyPMovesUnrestricted.North, piece.GetIsBlack()) == 1)
                    pMoves = CalculateOverlappingMoves(pMoves, enemyPMovesUnrestricted.North, true);
            }
            else if (enemyPMoves.East.Contains(piecePosition) && enemyPMovesUnrestricted.East.Contains(kingPosition))
            {
                if (CalculateSameColorPiecesInDirection(enemyPMovesUnrestricted.East, piece.GetIsBlack()) == 1)
                    pMoves = CalculateOverlappingMoves(pMoves, enemyPMovesUnrestricted.East, true);
            }
            else if (enemyPMoves.South.Contains(piecePosition) && enemyPMovesUnrestricted.South.Contains(kingPosition))
            {
                if (CalculateSameColorPiecesInDirection(enemyPMovesUnrestricted.South, piece.GetIsBlack()) == 1)
                    pMoves = CalculateOverlappingMoves(pMoves, enemyPMovesUnrestricted.South, true);
            }
            else if (enemyPMoves.West.Contains(piecePosition) && enemyPMovesUnrestricted.West.Contains(kingPosition))
            {
                if (CalculateSameColorPiecesInDirection(enemyPMovesUnrestricted.West, piece.GetIsBlack()) == 1)
                    pMoves = CalculateOverlappingMoves(pMoves, enemyPMovesUnrestricted.West, true);
            }
            else if (enemyPMoves.North_West.Contains(piecePosition) && enemyPMovesUnrestricted.North_West.Contains(kingPosition))
            {
                if (CalculateSameColorPiecesInDirection(enemyPMovesUnrestricted.North_West, piece.GetIsBlack()) == 1)
                    pMoves = CalculateOverlappingMoves(pMoves, enemyPMovesUnrestricted.North_West, true);
            }
            else if (enemyPMoves.North_East.Contains(piecePosition) && enemyPMovesUnrestricted.North_East.Contains(kingPosition))
            {
                if (CalculateSameColorPiecesInDirection(enemyPMovesUnrestricted.North_East, piece.GetIsBlack()) == 1)
                    pMoves = CalculateOverlappingMoves(pMoves, enemyPMovesUnrestricted.North_East, true);
            }
            else if (enemyPMoves.South_West.Contains(piecePosition) && enemyPMovesUnrestricted.South_West.Contains(kingPosition))
            {
                if (CalculateSameColorPiecesInDirection(enemyPMovesUnrestricted.South_West, piece.GetIsBlack()) == 1)
                    pMoves = CalculateOverlappingMoves(pMoves, enemyPMovesUnrestricted.South_West, true);
            }
            else if (enemyPMoves.South_East.Contains(piecePosition) && enemyPMovesUnrestricted.South_East.Contains(kingPosition))
            {
                if (CalculateSameColorPiecesInDirection(enemyPMovesUnrestricted.South_East, piece.GetIsBlack()) == 1)
                    pMoves = CalculateOverlappingMoves(pMoves, enemyPMovesUnrestricted.South_East, true);
            }

            if (canKill)
            {
                pMoves.Add(enemy.GetPosition());
            }
        }
    }

    /// <summary>
    /// Used by King logic. Calculates how many pieces of the same color are protecting King.
    /// </summary>
    /// <param name="movesLine">Moves in the passed direction</param>
    /// <param name="isBlack">Color of piece</param>
    /// <returns>int</returns>
    public int CalculateSameColorPiecesInDirection(List<Position> movesLine, bool isBlack)
    {
        var sum = 0;
        for (int i = 0; i < movesLine.Count; i++)
        {
            if (!IsCellFree(movesLine[i]))
            {
                var pieceInCell = gameGrid.GetPieceInGrid(movesLine[i]).GetComponent<Piece>();

                if (!pieceInCell.isKing && pieceInCell.GetIsBlack() == isBlack)
                {
                    sum++;
                }
            }
        }
        return sum;
    }

    /// <summary>
    /// Calculates possible moves in specified direction.
    /// </summary>
    /// <param name="piece">Piece</param>
    /// <param name="unrestricted">Whether it should stop at first piece in its path or continue until board dimensions</param>
    /// <returns>DirectionPossibleMoves</returns>
    public DirectionPossibleMoves CalculatePossibleMovesWithDirection(Piece piece, bool unrestricted = false)
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
                ExtendSpecialPiecePossibleMoves(row, col, pos, unrestricted, ref possibleMoves, isBlack);


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

    /// <summary>
    /// Calculates overlapping moves or not overlapping moves depending on overlap parameter.
    /// </summary>
    /// <param name="first">first list of positions</param>
    /// <param name="comparer">Comparer</param>
    /// <param name="overlap">Whether moves should overlap or not. First case intersection of sets (A * B), second is difference of set (A/B)</param>
    /// <returns>List of positions</returns>
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

    public void ExtendSpecialPiecePossibleMovesInverted(
        int row,
        int col,
        Position pos,
        ref List<Position> possibleMoves,
        bool isBlack)
    {
        Position destPos = new(col + pos.x, row + pos.y);
        while (true)
        {
            if (IsInBoard(destPos))
            {
                if (IsCellFree(destPos))
                {
                    possibleMoves.Add(new(destPos));
                }
                else
                {
                    if (IsEnemy(destPos, isBlack))
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

    /// <summary>
    /// Extends special piece possible moves.
    /// </summary>
    /// <param name="row">row operator</param>
    /// <param name="col">col operator</param>
    /// <param name="pos">piece position</param>
    /// <param name="unrestricted">whether piece ignores other pieces or not</param>
    /// <param name="possibleMoves">reference possible moves</param>
    /// <param name="isBlack">piece color</param>
    public void ExtendSpecialPiecePossibleMoves(
        int row,
        int col,
        Position pos,
        bool unrestricted,
        ref List<Position> possibleMoves,
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

    /// <summary>
    /// Calculates possible drops for clicked piece.
    /// </summary>
    /// <param name="piece">piece</param>
    /// <returns>Possible drops list</returns>
    public List<Position> CalculatePossibleDrops(Piece piece)
    {
        List<Position> moves = new();
        if (piece.GetName() == "Pawn")
        {
            List<int> badX = new();

            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    if (!IsCellFree(x, y) && gameGrid.GetPieceInGrid(x, y).GetComponent<Piece>().GetName() == "Pawn")
                    {
                        var gridPiece = gameGrid.GetPieceInGrid(x, y).GetComponent<Piece>();
                        if (gridPiece.GetName() == "Pawn" && !gridPiece.GetIsBlack() != piece.GetIsBlack())
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

    /// <summary>
    /// Checks if cell is free - meaning not containing any pieces. Takes piece color into consideration. Enemy piece will show as free cell.
    /// </summary>
    /// <param name="destX">pos x</param>
    /// <param name="destY">pos y</param>
    /// <param name="isBlack">color of piece</param>
    /// <returns>bool</returns>
    public bool IsCellFree(int destX, int destY, bool isBlack)
    {
        var cell = gameGrid.GetGridCell(destX, destY);
        return cell.objectInThisGridSpace != null
            && cell.objectInThisGridSpace.GetComponent<Piece>().GetIsBlack() == isBlack
            ? false
            : true;
    }

    /// <summary>
    /// Checks if cell is free - meaning not containing any pieces.
    /// </summary>
    /// <param name="destX">pos x</param>
    /// <param name="destY">pos y</param>
    /// <returns>bool</returns>
    public bool IsCellFree(int destX, int destY)
        => gameGrid.GetGridCell(destX, destY).objectInThisGridSpace == null;

    /// <summary>
    /// Checks if cell is free - meaning not containing any pieces.
    /// </summary>
    /// <param name="pos">checked position</param>
    /// <returns>bool</returns>
    public bool IsCellFree(Position pos)
        => gameGrid.GetGridCell(pos).objectInThisGridSpace == null;

    /// <summary>
    /// Checks if piece in grid is enemy or not.
    /// </summary>
    /// <param name="destX">pos x</param>
    /// <param name="destY">pos y</param>
    /// <param name="isBlack">color of clicked piece</param>
    /// <returns>bool</returns>
    public bool IsEnemy(int destX, int destY, bool isBlack)
    {
        var cell = gameGrid.GetGridCell(destX, destY);
        if (cell.objectInThisGridSpace != null)
        {
            var pieceColorInDestination = gameGrid.GetPieceInGrid(destX, destY).GetComponent<Piece>().GetIsBlack();
            return pieceColorInDestination != isBlack;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if piece in grid is enemy or not.
    /// </summary>
    /// <param name="pos">position of checked piece</param>
    /// <param name="isBlack">color of clicked piece</param>
    /// <returns>bool</returns>
    public bool IsEnemy(Position pos, bool isBlack)
    {
        var cell = gameGrid.GetGridCell(pos);
        if (cell.objectInThisGridSpace != null)
        {
            var pieceColorInDestination = gameGrid.GetPieceInGrid(pos).GetComponent<Piece>().GetIsBlack();
            return pieceColorInDestination != isBlack;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Checks if position is in board.
    /// </summary>
    /// <returns>bool</returns>
    public bool IsInBoard(int row, int col)
    {
        if (row > -1 && row < 9 && col > -1 && col < 9)
            return true;
        else
            return false;
    }

    /// <summary>
    /// Checks if position is in board.
    /// </summary>
    /// <returns>bool</returns>
    public bool IsInBoard(Position p)
        => p.y > -1 && p.y < 9 && p.x > -1 && p.x < 9;

    /// <summary>
    /// Applies promotion to piece.
    /// </summary>
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
}