using System.Collections.Generic;

/// <summary>
/// Same as KingManager but operates on LogicBoard and LogicCells.
/// </summary>
public class LogicKingManager
{
    private readonly LogicBoardManager boardManager = new();

    public List<LogicPiece> FindGuards(Position attackerPos, List<LogicPiece> piecesList, LogicCell[,] cells)
    {
        List<LogicPiece> guards = new();
        foreach (var piece in piecesList)
        {
            if (!piece.isKing)
            {
                var piecePossibleMoves = boardManager.CalculatePossibleMoves(piece, cells);

                if (piecePossibleMoves.Contains(attackerPos))
                {
                    guards.Add(piece);
                }
            }
        }
        return guards;
    }

    public List<LogicPiece> FindSacrifices(List<Position> dangerMoves, List<LogicPiece> piecesList, LogicCell[,] cells)
    {
        if (dangerMoves == null || dangerMoves.Count == 0) { return null; }

        List<LogicPiece> sacrifices = new();
        foreach (var piece in piecesList)
        {
            var piecePossibleMoves = boardManager.CalculatePossibleMoves(piece, cells);
            foreach (var m in dangerMoves)
            {
                if (piecePossibleMoves.Contains(m))
                    sacrifices.Add(piece);
            }
        }
        return sacrifices;
    }

    public List<Position> ValidMovesScan(LogicPiece king, List<LogicPiece> enemyPieces, LogicCell[,] cells)
    {
        List<Position> kingPMoves = boardManager.CalculatePossibleMoves(king, cells);

        foreach (var piece in enemyPieces)
        {
            List<Position> enemyPiecePMoves = boardManager.CalculatePossibleMoves(piece, cells);
            kingPMoves = boardManager.CalculateOverlappingMoves(kingPMoves, enemyPiecePMoves, false);
        }

        return kingPMoves;
    }

    public bool IsAttackerProtected(LogicPiece attacker, List<LogicPiece> friendlyPieces, LogicCell[,] cells)
    {
        var attackerPosition = attacker.GetPosition();
        foreach (var piece in friendlyPieces)
        {
            if (!piece.GetIsDrop())
            {
                List<Position> friendlyPiecePMovesInverted = boardManager.CalculatePossibleMovesInverted(piece, cells);

                if (friendlyPiecePMovesInverted.Contains(attackerPosition))
                {
                    return true;
                }
            }
        }

        return false;
    }

    public List<Position> KingDangerMovesScan(List<Position> pos, bool isBlack, LogicCell[,] cells)
    {
        List<Position> dangerMoves = new();
        foreach (var p in pos)
        {
            int rowOperator = 1;
            int colOperator = -1;
            for (int i = 1; i <= 9; i++)
            {
                var res = FarKingDirectionScanDangerMoves(rowOperator, colOperator, p, isBlack, cells);
                if (res != null)
                {
                    dangerMoves.AddRange(res);
                }
                colOperator++;
                if (i % 3 == 0 && i != 0)
                {
                    rowOperator--;
                    colOperator = -1;
                }
            }
        }
        return dangerMoves;
    }

    public List<Position> FarKingDirectionScanDangerMoves(int rowOperator, int colOperator, Position source, bool isBlack, LogicCell[,] cells)
    {
        if (rowOperator == 0 && colOperator == 0) { return null; }
        int destX = source.x + colOperator;
        int destY = source.y + rowOperator;
        List<Position> temp = new()
        {
            new(source.x, source.y)
        };
        while (true)
        {
            if (boardManager.IsInBoard(destX, destY))
            {
                if (!boardManager.IsCellFree(destX, destY, cells))
                {
                    if (!boardManager.IsEnemy(destX, destY, isBlack, cells))
                    {
                        //friend
                        return null;
                    }
                    else
                    {
                        //enemy found
                        var enemyPiece = cells[destX, destY].piece;
                        if (enemyPiece.GetIsSpecial())
                        {
                            return temp;
                        }
                        else
                        {
                            return null;
                        }
                    }
                }
                else
                {
                    temp.Add(new(destX, destY));
                }
            }
            else
            {
                return null;
            }
            destX += colOperator;
            destY += rowOperator;
        }
    }

    public List<Position> CalculateEndangeredMoves(LogicPiece attacker, Position kingPos = null)
    {
        var attackerMoveset = attacker.GetMoveset();
        int rowOperator = 1;
        int colOperator = -1;
        List<Position> moves = new();
        for (int i = 1; i <= 9; i++)
        {
            if (attackerMoveset[i - 1] == 2)
            {
                if (kingPos != null)
                {
                    moves = ScanMoves(rowOperator, colOperator, attacker.GetPosition(), kingPos);
                    if (moves != null)
                    {
                        return moves;
                    }
                }
                else
                {
                    moves.AddRange(ScanMoves(rowOperator, colOperator, attacker.GetPosition(), kingPos));
                }
            }
            colOperator++;
            if (i % 3 == 0 && i != 0)
            {
                rowOperator--;
                colOperator = -1;
            }
        }
        return moves;
    }

    public List<Position> ScanMoves(int rowOperator, int colOperator, Position source, Position destination = null)
    {
        if (rowOperator == 0 && colOperator == 0) { return null; }
        int destX = source.x + colOperator;
        int destY = source.y + rowOperator;

        List<Position> scan = new();
        while (true)
        {
            if (boardManager.IsInBoard(destX, destY))
            {
                scan.Add(new(destX, destY));
                if (destination != null && destX == destination.x && destY == destination.y)
                {
                    //scan complete reached destination
                    scan.Remove(destination);
                    return scan;
                }
                destY += rowOperator;
                destX += colOperator;
            }
            else
            {
                //scan complete did not reach destination
                return null;
            }
        }
    }

    public List<Position> CalculateProtectionMoves(LogicPiece piece, List<Position> dangerMoves, LogicCell[,] cells)
    {
        var moves = boardManager.CalculatePossibleMoves(piece, cells);
        List<Position> protectionMoves = new();
        for (int i = 0; i < moves.Count; i++)
        {
            for (int j = 0; j < dangerMoves.Count; j++)
            {
                if (moves[i].Equals(dangerMoves[j]))
                {
                    protectionMoves.Add(moves[i]);
                }
            }
        }
        return protectionMoves;
    }

    /// <summary>
    /// Scans if King is endangered in close proximity. Returns true if King endangered, false otherwise.
    /// </summary>
    /// <param name="king">King piece</param>
    /// <param name="attackerPos">Attacker position</param>
    /// <returns>bool</returns>
    public bool CloseScanForKing(Unit king, LogicCell[,] cells, Position attackerPos)
    {
        var possibleMoves = boardManager.NewCalculatePossibleMoves(king, cells);
        return possibleMoves.Contains(attackerPos);
    }

    /// <summary>
    /// Checks if King is endangered by long-range attacking piece. Returns true if King in danger and false otherwise.
    /// </summary>
    /// <param name="pos">King position</param>
    /// <param name="isBlack">King color</param>
    /// <param name="attackerPos">Attacker's position</param>
    /// <returns>bool</returns>
    public bool FarScanForKing(Position pos, bool isBlack, LogicCell[,] cells, List<Unit> enemies, ref Position attackerPos)
    {
        int rowOperator = 1;
        int colOperator = -1;
        for (int i = 1; i <= 9; i++)
        {
            if (FarKingDirectionScan(rowOperator, colOperator, cells, enemies, pos, isBlack, ref attackerPos))
            {
                return true;
            }
            colOperator++;
            if (i % 3 == 0 && i != 0)
            {
                rowOperator--;
                colOperator = -1;
            }
        }
        return false;
    }

    /// <summary>
    /// Same as FarScanForKing but with specified direction.
    /// </summary>
    /// <returns>bool</returns>
    public bool FarKingDirectionScan(int rowOperator, int colOperator, LogicCell[,] cells, List<Unit> enemies, Position source, bool isBlack, ref Position attackerPos)
    {
        int destX = source.x + colOperator;
        int destY = source.y + rowOperator;


        foreach (var enemy in enemies)
        {
            var moves = boardManager.NewCalculatePossibleMoves(enemy, cells);

            if (enemy.GetIsSpecial())
            {
                foreach (var m in moves)
                {
                    if (m.Equals(source))
                    {
                        attackerPos = new(destX, destY);
                        return true;
                    }
                }
            }
            else
            {
                return false;
            }
        }

        return false;

        //while (true)
        //{
        //    if (rowOperator == 0 && colOperator == 0)
        //    {
        //        return false;
        //    }
        //    if (boardManager.IsInBoard(destX, destY))
        //    {
        //        if (!boardManager.IsCellFree(destX, destY, cells))
        //        {
        //            var piece = gridGame.GetPieceInGrid(destX, destY).GetComponent<Piece>();
        //            var pieceMoves = boardManager.CalculatePossibleMoves(piece);

        //            if (boardManager.IsEnemy(destX, destY, isBlack, cells))
        //            {
        //                if (piece.GetIsSpecial())
        //                {
        //                    foreach (var m in pieceMoves)
        //                    {
        //                        if (m.Equals(source))
        //                        {
        //                            attackerPos = new(destX, destY);
        //                            return true;
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    return false;
        //                }
        //            }
        //            else if (!boardManager.IsEnemy(destX, destY, isBlack, cells))
        //            {
        //                return false;
        //            }
        //        }
        //        destX += colOperator;
        //        destY += rowOperator;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}
    }
}