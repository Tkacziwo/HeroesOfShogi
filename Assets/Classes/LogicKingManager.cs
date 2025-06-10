using System.Collections.Generic;

public class LogicKingManager
{
    private readonly LogicBoardManager boardManager = new();

    public List<LogicPiece> FindGuards(Position attackerPos, List<LogicPiece> piecesList, LogicCell[,] cells)
    {
        List<LogicPiece> guards = new();
        foreach (var piece in piecesList)
        {
            var piecePossibleMoves = boardManager.CalculatePossibleMoves(piece, cells);

            if (piecePossibleMoves.Contains(attackerPos))
                guards.Add(piece);
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
}