using System.Collections.Generic;
using System;
using UnityEngine;
using Unity.VisualScripting;

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
                    GameObject potentialGuard = gridGame.GetPieceInGrid(x, y);
                    if (potentialGuard != null && CanPieceKillAttacker(potentialGuard, attackerPos))
                    {
                        bodyguardsPos.Add(potentialGuard.GetComponent<Piece>().GetPositionTuple());
                    }
                }

            }
        }
        return bodyguardsPos;
    }

    public List<Tuple<int, int>> FindSacrifices(Piece king, Piece attacker)
    {
        List<Tuple<int, int>> sacrificesPositions = new();
        List<Tuple<int, int>> endangeredMoves = CalculateEndangeredMoves(king.GetPositionTuple(), attacker);
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
        return sacrificesPositions;
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

    public List<Tuple<int, int>> CalculateEndangeredMoves(Tuple<int, int> kingPos, Piece attacker)
    {
        var attackerMoveset = attacker.GetMoveset();
        int rowOperator = 1;
        int colOperator = -1;
        for (int i = 0; i < 9; i++)
        {

            if (attackerMoveset[i] == 2)
            {
                var moves = ScanMoves(rowOperator, colOperator, kingPos, attacker.GetPositionTuple());
                if (moves != null)
                {
                    return moves;
                }
            }
            colOperator++;
            if (i % 3 == 0 && i != 0)
            {
                rowOperator--;
                colOperator = -1;
            }
        }
        return null;
    }

    public List<Tuple<int, int>> ScanMoves(int rowOperator, int colOperator, Tuple<int, int> destination, Tuple<int, int> source)
    {
        int destX = source.Item1 + colOperator;
        int destY = source.Item2 + rowOperator;

        List<Tuple<int, int>> scan = new();
        while (true)
        {
            if (boardManager.IsInBoard(destX, destY))
            {
                scan.Add(new(destX, destY));
                if (destX == destination.Item1 && destY == destination.Item2)
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

    public bool CanPieceKillAttacker(GameObject guardPiece, Tuple<int, int> attackerPos)
    {
        var gPiece = guardPiece.GetComponent<Piece>();
        var gMoves = boardManager.CalculatePossibleMoves(gPiece.GetPosition(), gPiece.GetMoveset(), gPiece.GetIsBlack());
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