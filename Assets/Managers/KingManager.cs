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

    public List<Tuple<int, int>> FindBodyguards(bool isBlack, Tuple<int, int> attackerPos)
    {
        List<Tuple<int, int>> bodyguardsPos = new();
        for (int y = 0; y < 9; y++)
        {
            for (int x = 0; x < 9; x++)
            {
                //is not free and is not enemy - [todo] -> make one function for that
                if (!boardManager.IsCellFree(x, y) && !boardManager.IsEnemy(x, y, isBlack))
                {
                    Piece potentialGuard = gridGame.GetPieceInGrid(x, y).GetComponent<Piece>();
                    if (potentialGuard != null && CanPieceKillAttacker(potentialGuard, attackerPos))
                    {
                        bodyguardsPos.Add(potentialGuard.GetComponent<Piece>().GetPositionTuple());
                    }
                }

            }
        }
        return bodyguardsPos;
    }

    public List<Piece> FindBodyguardsPieces(bool isBlack, Tuple<int, int> attackerPos)
    {
        List<Piece> bodyguards = new();
        for (int y = 0; y < 9; y++)
        {
            for (int x = 0; x < 9; x++)
            {
                if (!boardManager.IsCellFree(x, y) && !boardManager.IsEnemy(x, y, isBlack))
                {
                    Piece potentialGuard = gridGame.GetPieceInGrid(x, y).GetComponent<Piece>();
                    if (potentialGuard != null && CanPieceKillAttacker(potentialGuard, attackerPos))
                    {
                        bodyguards.Add(potentialGuard.GetComponent<Piece>());
                    }
                }

            }
        }
        return bodyguards;
    }

    public List<Tuple<int, int>> FindSacrifices(Piece king, Piece attacker)
    {
        if (!attacker.GetIsSpecial())
        {
            return new List<Tuple<int, int>>();
        }
        else
        {
            List<Tuple<int, int>> sacrificesPositions = new();
            List<Tuple<int, int>> endangeredMoves = CalculateEndangeredMoves(attacker, king.GetPositionTuple());
            for (int y = 0; y < 9; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    if (!boardManager.IsCellFree(x, y) && !boardManager.IsEnemy(x, y, king.GetIsBlack()))
                    {
                        //[todo] find a better way to write that
                        var piece = gridGame.GetPieceInGrid(x, y).GetComponent<Piece>();
                        if (piece.isKing)
                        {
                            break;
                        }
                        if (endangeredMoves != null)
                        {
                            var pieceMoves = boardManager.CalculatePossibleMoves(piece.GetPosition(), piece.GetMoveset(), piece.GetIsBlack());
                            for (int z = 0; z < pieceMoves.Count; z++)
                            {
                                for (int k = 0; k < endangeredMoves.Count; k++)
                                {
                                    if (pieceMoves[z].Equals(endangeredMoves[k]))
                                    {
                                        sacrificesPositions.Add(piece.GetPositionTuple());
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return sacrificesPositions;
        }
    }

    public List<Tuple<int, int>> CalculateProtectionMoves(Vector2Int pos, int[] moveset, bool isBlack, List<Tuple<int, int>> dangerMoves)
    {
        var moves = boardManager.CalculatePossibleMoves(pos, moveset, isBlack);
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

    public List<Tuple<int, int>> CalculateEscapeMoves(Tuple<int, int> kingPos, List<Tuple<int, int>> dangerMoves)
    {
        List<Tuple<int, int>> moves = new();

        return moves;
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
        int destX = source.Item1 + colOperator;
        int destY = source.Item2 + rowOperator;
        List<Tuple<int, int>> temp = new()
        {
            new(source.Item1, source.Item2)
        };
        while (true)
        {
            if(rowOperator == 0 && colOperator == 0)
            {
                return null;
            }
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
                    var pieceMoves = boardManager.CalculatePossibleMoves(piece.GetPosition(), piece.GetMoveset(), piece.GetIsBlack());

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
    public bool FarEnemyDirectionScan(int rowOperator, int colOperator, Tuple<int, int> source, bool isBlack)
    {
        int destX = source.Item1 + colOperator;
        int destY = source.Item2 + rowOperator;
        while (true)
        {
            if (boardManager.IsInBoard(destX, destY))
            {
                if (!boardManager.IsCellFree(destX, destY))
                {
                    var piece = gridGame.GetPieceInGrid(destX, destY).GetComponent<Piece>();
                    if (!boardManager.IsEnemy(destX, destY, isBlack))
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
        var kingPosVector = new Vector2Int(kingPos.Item1, kingPos.Item2);
        var possibleMoves = boardManager.CalculatePossibleMoves(kingPosVector, kingPiece.GetMoveset(), kingPiece.GetIsBlack());
        List<Tuple<int, int>> overlappingMoves = new();
        List<Tuple<int, int>> enemiesProtectionMoves = new();
        var rowOperator = 2;
        var colOperator = -2;

        for (int i = 0; i < 25; i++)
        {
            int destX = kingPos.Item1 + colOperator;
            int destY = kingPos.Item2 + rowOperator;

            if (boardManager.IsInBoard(destX, destY)
                && !boardManager.IsCellFree(destX, destY)
                && boardManager.IsEnemy(destX, destY, kingPiece.GetIsBlack()))
            {
                Vector2Int enemyPos = new(destX, destY);
                var enemyPiece = gridGame.GetPieceInGrid(destX, destY).GetComponent<Piece>();
                var enemyPossibleMoves = boardManager.CalculatePossibleMoves(enemyPos, enemyPiece.GetMoveset(), enemyPiece.GetIsBlack());
                var enemyProtectionMoves = boardManager.CalculatePossibleMoves(enemyPos, enemyPiece.GetMoveset(), !enemyPiece.GetIsBlack());
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
                    moves = ScanMoves(rowOperator, colOperator, kingPos, attacker.GetPositionTuple());

                    if (moves != null)
                    {
                        return moves;
                    }
                }
                else
                {
                    moves.AddRange(ScanMoves(rowOperator, colOperator, kingPos, attacker.GetPositionTuple()));
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

    public List<Tuple<int, int>> ScanMoves(int rowOperator, int colOperator, Tuple<int, int> destination, Tuple<int, int> source)
    {
        int destX = source.Item1 + colOperator;
        int destY = source.Item2 + rowOperator;

        List<Tuple<int, int>> scan = new();
        while (true)
        {
            if (rowOperator == 0 && colOperator == 0)
            {
                return null;
            }
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
                return scan;
            }
        }
    }

    public bool CanPieceKillAttacker(Piece guardPiece, Tuple<int, int> attackerPos)
    {
        var gMoves = boardManager.CalculatePossibleMoves(guardPiece.GetPosition(), guardPiece.GetMoveset(), guardPiece.GetIsBlack());
        foreach (var m in gMoves)
        {
            if (m.Equals(attackerPos))
            {
                return true;
            }
        }
        return false;
    }
}