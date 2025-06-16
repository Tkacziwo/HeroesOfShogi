using System.Collections.Generic;
using System;
using UnityEngine;

public class KingManager : MonoBehaviour
{
    private BoardManager boardManager;

    [SerializeField] private Grid gridGame;

    void Start()
    {
        boardManager = FindFirstObjectByType<BoardManager>();
    }

    public List<Piece> FindGuards(Position attackerPos, List<Piece> piecesList)
    {
        List<Piece> guards = new();
        foreach (var piece in piecesList)
        {
            var piecePossibleMoves = boardManager.CalculatePossibleMoves(piece);

            if (piecePossibleMoves.Contains(attackerPos))
                guards.Add(piece);
        }
        return guards;
    }

    public List<Piece> FindSacrifices(List<Position> dangerMoves, List<Piece> piecesList)
    {
        if (dangerMoves == null || dangerMoves.Count == 0) { return null; }

        List<Piece> sacrifices = new();
        foreach (var piece in piecesList)
        {
            var piecePossibleMoves = boardManager.CalculatePossibleMoves(piece);
            foreach (var m in dangerMoves)
            {
                if (piecePossibleMoves.Contains(m))
                    sacrifices.Add(piece);
            }
        }
        return sacrifices;
    }

    public List<Position> CalculateProtectionMoves(Piece piece, List<Position> dangerMoves)
    {
        var moves = boardManager.CalculatePossibleMoves(piece);
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

    public List<Position> KingDangerMovesScan(List<Position> pos, bool isBlack)
    {
        List<Position> dangerMoves = new();
        foreach (var p in pos)
        {
            int rowOperator = 1;
            int colOperator = -1;
            for (int i = 1; i <= 9; i++)
            {
                var res = FarKingDirectionScanDangerMoves(rowOperator, colOperator, p, isBlack);
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

    public List<Position> FarKingDirectionScanDangerMoves(int rowOperator, int colOperator, Position source, bool isBlack)
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
                if (!boardManager.IsCellFree(destX, destY))
                {
                    if (!boardManager.IsEnemy(destX, destY, isBlack))
                    {
                        //friend
                        return null;
                    }
                    else
                    {
                        //enemy found
                        var enemyPiece = gridGame.GetPieceInGrid(destX, destY).GetComponent<Piece>();
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

    public bool FarScanForKing(Position pos, bool isBlack, ref Position attackerPos)
    {
        int rowOperator = 1;
        int colOperator = -1;
        for (int i = 1; i <= 9; i++)
        {
            if (FarKingDirectionScan(rowOperator, colOperator, pos, isBlack, ref attackerPos))
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

    public bool FarKingDirectionScan(int rowOperator, int colOperator, Position source, bool isBlack, ref Position attackerPos)
    {
        int destX = source.x + colOperator;
        int destY = source.y + rowOperator;
        while (true)
        {
            if (rowOperator == 0 && colOperator == 0)
            {
                return false;
            }
            if (boardManager.IsInBoard(destX, destY))
            {
                if (!boardManager.IsCellFree(destX, destY))
                {
                    var piece = gridGame.GetPieceInGrid(destX, destY).GetComponent<Piece>();
                    var pieceMoves = boardManager.CalculatePossibleMoves(piece);

                    if (boardManager.IsEnemy(destX, destY, isBlack))
                    {
                        if (piece.GetIsSpecial())
                        {
                            foreach (var m in pieceMoves)
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
                    else if (!boardManager.IsEnemy(destX, destY, isBlack))
                    {
                        return false;
                    }
                }
                destX += colOperator;
                destY += rowOperator;
            }
            else
            {
                return false;
            }
        }
    }

    public bool CloseScanForKing(Piece king, Position attackerPos)
    {
        var possibleMoves = boardManager.CalculatePossibleMoves(king);
        return possibleMoves.Contains(attackerPos);
    }

    public List<Position> ValidMovesScan(Piece king)
    {
        var enemyPieces = king.GetIsBlack() ? gridGame.GetPlayerPieces() : gridGame.GetBotPieces();
        List<Position> kingPMoves = boardManager.CalculatePossibleMoves(king);

        foreach (var piece in enemyPieces)
        {
            List<Position> enemyPiecePMoves = boardManager.CalculatePossibleMoves(piece);
            kingPMoves = boardManager.CalculateOverlappingMoves(kingPMoves, enemyPiecePMoves, false);
        }

        return kingPMoves;
    }

    public bool IsAttackerProtected(Piece attacker)
    {
        var friendlyPieces = attacker.GetIsBlack() ? gridGame.GetBotPieces() : gridGame.GetPlayerPieces();
        var attackerPosition = attacker.GetPosition();
        foreach (var piece in friendlyPieces)
        {
            if (!piece.GetIsDrop())
            {
                List<Position> friendlyPiecePMovesInverted = boardManager.CalculatePossibleMovesInverted(piece);

                if (friendlyPiecePMovesInverted.Contains(attackerPosition))
                {
                    return true;
                }
            }
        }

        return false;
    }
   
    public List<Position> CalculateEndangeredMoves(Piece attacker, Position kingPos = null)
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

    public bool AttackerScanForKing(Piece king, Piece attacker)
    {
        var attackerMoves = boardManager.CalculatePossibleMoves(attacker);
        return attackerMoves.Contains(king.GetPosition());
    }
}