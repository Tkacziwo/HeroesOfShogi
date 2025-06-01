using System.Collections.Generic;
using System;
using UnityEngine;

public class KingManager : MonoBehaviour
{
    private BoardManager boardManager;

    private GridGame gridGame;

    void Start()
    {
        boardManager = FindFirstObjectByType<BoardManager>();
        gridGame = FindFirstObjectByType<GridGame>();
    }

    public List<Piece> FindGuards(Tuple<int, int> attackerPos, List<Piece> piecesList)
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

    public List<Piece> FindSacrifices(List<Tuple<int, int>> dangerMoves, List<Piece> piecesList)
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

    public List<Tuple<int, int>> CalculateProtectionMoves(Piece piece, List<Tuple<int, int>> dangerMoves)
    {
        var moves = boardManager.CalculatePossibleMoves(piece);
        List<Tuple<int, int>> protectionMoves = new();
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

    public List<Tuple<int, int>> KingDangerMovesScan(List<Tuple<int, int>> pos, bool isBlack)
    {
        List<Tuple<int, int>> dangerMoves = new();
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

    public List<Tuple<int, int>> FarKingDirectionScanDangerMoves(int rowOperator, int colOperator, Tuple<int, int> source, bool isBlack)
    {
        if (rowOperator == 0 && colOperator == 0) { return null; }
        int destX = source.Item1 + colOperator;
        int destY = source.Item2 + rowOperator;
        List<Tuple<int, int>> temp = new()
        {
            new(source.Item1, source.Item2)
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

    public bool FarScanForKing(Tuple<int, int> pos, bool isBlack, ref Tuple<int, int> attackerPos)
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

    public bool FarKingDirectionScan(int rowOperator, int colOperator, Tuple<int, int> source, bool isBlack, ref Tuple<int, int> attackerPos)
    {
        int destX = source.Item1 + colOperator;
        int destY = source.Item2 + rowOperator;
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

    public bool FarScan(Tuple<int, int> enemyPos, bool isBlack)
    {
        //used for special piece scanning
        int rowOperator = 1;
        int colOperator = -1;
        for (int i = 1; i <= 9; i++)
        {
            if (FarEnemyDirectionScan(rowOperator, colOperator, enemyPos, isBlack))
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

    public bool CloseScanForKing(Piece king, Tuple<int, int> attackerPos)
    {
        var possibleMoves = boardManager.CalculatePossibleMoves(king);
        return possibleMoves.Contains(attackerPos);
    }

    public bool FarEnemyDirectionScan(int rowOperator, int colOperator, Tuple<int, int> source, bool isBlack)
    {
        if (rowOperator == 0 && colOperator == 0) { return false; }
        int destX = source.Item1 + colOperator;
        int destY = source.Item2 + rowOperator;
        while (true)
        {
            if (boardManager.IsInBoard(destX, destY))
            {
                if (!boardManager.IsCellFree(destX, destY))
                {
                    var piece = gridGame.GetPieceInGrid(destX, destY).GetComponent<Piece>();
                    if (!boardManager.IsEnemy(destX, destY, isBlack) && destX != source.Item1 && destY != source.Item2)
                    {
                        if (piece.GetIsSpecial())
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                    else if (boardManager.IsEnemy(destX, destY, isBlack))
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

    public List<Tuple<int, int>> CloseScan(Tuple<int, int> kingPos)
    {
        var kingPiece = gridGame.GetPieceInGrid(kingPos.Item1, kingPos.Item2).GetComponent<Piece>();
        var possibleMoves = boardManager.CalculatePossibleMoves(kingPiece);
        List<Tuple<int, int>> overlappingMoves = new();
        List<Tuple<int, int>> enemiesProtectionMoves = new();
        var rowOperator = 2;
        var colOperator = -2;

        for (int i = 0; i < 25; i++)
        {
            Position destPos = new(kingPos.Item1 + colOperator, kingPos.Item2 + rowOperator);

            if (boardManager.IsInBoard(destPos)
                && !boardManager.IsCellFree(destPos)
                && boardManager.IsEnemy(destPos, kingPiece.GetIsBlack()))
            {
                var enemyPiece = gridGame.GetPieceInGrid(destPos).GetComponent<Piece>();
                var enemyPossibleMoves = boardManager.CalculatePossibleMoves(enemyPiece);
                var enemyProtectionMoves = boardManager.CalculatePossibleMoves(enemyPiece);
                enemiesProtectionMoves.AddRange(enemyProtectionMoves);
                overlappingMoves.AddRange(boardManager.CalculateOverlappingMoves(possibleMoves, enemyPossibleMoves, true));
            }
            colOperator++;
            if (i % 5 == 0 && i != 0)
            {
                rowOperator--;
                colOperator = -2;
            }
        }
        possibleMoves = boardManager.CalculateOverlappingMoves(possibleMoves, overlappingMoves, false);
        possibleMoves = boardManager.CalculateOverlappingMoves(possibleMoves, enemiesProtectionMoves, false);

        return possibleMoves;
    }

    public List<Tuple<int, int>> CalculateEndangeredMoves(Piece attacker, Tuple<int, int> kingPos = null)
    {
        var attackerMoveset = attacker.GetMoveset();
        int rowOperator = 1;
        int colOperator = -1;
        List<Tuple<int, int>> moves = new();
        for (int i = 1; i <= 9; i++)
        {
            if (attackerMoveset[i - 1] == 2)
            {
                if (kingPos != null)
                {
                    moves = ScanMoves(rowOperator, colOperator, attacker.GetPositionTuple(), kingPos);
                    if (moves != null)
                    {
                        return moves;
                    }
                }
                else
                {
                    moves.AddRange(ScanMoves(rowOperator, colOperator, attacker.GetPositionTuple(), kingPos));
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

    public List<Tuple<int, int>> ScanMoves(int rowOperator, int colOperator, Tuple<int, int> source, Tuple<int, int> destination = null)
    {
        if (rowOperator == 0 && colOperator == 0) { return null; }
        int destX = source.Item1 + colOperator;
        int destY = source.Item2 + rowOperator;

        List<Tuple<int, int>> scan = new();
        while (true)
        {
            if (boardManager.IsInBoard(destX, destY))
            {
                scan.Add(new(destX, destY));
                if (destination != null && destX == destination.Item1 && destY == destination.Item2)
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
}