using System;
using UnityEngine;
using System.Collections.Generic;

public class ShogiBot : MonoBehaviour
{
    [SerializeField] private BoardManager boardManager;

    [SerializeField] private KingManager kingManager;

    [SerializeField] private int depth = 3;

    public List<Tuple<int, int>> allPossibleMoves = new();

    public List<Tuple<int, int>> logicPossibleMoves = new();

    private Tuple<int, int> source;

    private Tuple<int, int> destination;

    private LogicBoard logicBoard = new();

    //get board state on initialization (maybe)
    //for now get board state every turn (not optimal)
    //create logicBoard to apply minimax
    public void GetBoardState(GridGame grid)
    {
        logicBoard.CloneFromReal(grid);
    }

    //include in board state captured pieces for drops


    //calculate all possibleMoves
    public void CalculateAllPossibleMoves(bool kingInDanger,
        Tuple<int, int> kingPos = null,
        List<Piece> bodyguards = null,
        List<Piece> sacrifices = null,
        GridCell cellWhichHoldsAttacker = null,
        List<Tuple<int, int>> extendedDangerMoves = null)
    {
        allPossibleMoves = new();

        //if (kingInDanger)
        //{
        //    Piece king = grid.GetPieceInGrid(kingPos).GetComponent<Piece>();

        //    var kingMoves = kingManager.CloseScan(kingPos);
        //    var attacker = cellWhichHoldsAttacker.objectInThisGridSpace.GetComponent<Piece>();
        //    var attackerProtected = kingManager
        //        .FarScan(attacker.GetPositionTuple(), attacker.GetIsBlack());
        //    var additionalDangerMoves = kingManager.KingDangerMovesScan(kingMoves, king.GetIsBlack());
        //    List<Tuple<int, int>> attackerPos = new()
        //        {
        //            attacker.GetPositionTuple()
        //        };

        //    if (additionalDangerMoves != null)
        //    {
        //        kingMoves = boardManager.CalculateOverlappingMoves(kingMoves, additionalDangerMoves, false);
        //    }

        //    if (attackerProtected)
        //    {
        //        kingMoves = boardManager.CalculateOverlappingMoves(kingMoves, attackerPos, false);
        //    }

        //    if (extendedDangerMoves != null)
        //    {
        //        kingMoves = boardManager.CalculateOverlappingMoves(kingMoves, extendedDangerMoves, false);
        //    }

        //    List<Tuple<int, int>> bodyguardsPossibleMoves = new();
        //    //bodyguardsPossibleMoves.Add(attacker.GetPositionTuple());

        //    foreach (var b in bodyguards)
        //    {
        //        bodyguardsPossibleMoves.Add(attacker.GetPositionTuple());
        //    }
        //    allPossibleMoves.AddRange(kingMoves);
        //    allPossibleMoves.AddRange(bodyguardsPossibleMoves);
        //}
        //else
        //{
        //    foreach (var p in pieces)
        //    {
        //        if (p.GetIsBlack() && p.isKing)
        //        {
        //            Piece king = grid.GetPieceInGrid(p.GetPositionTuple()).GetComponent<Piece>();

        //            var kingMoves = kingManager.CloseScan(p.GetPositionTuple());

        //            var additionalDangerMoves = kingManager.KingDangerMovesScan(kingMoves, king.GetIsBlack());

        //            if (additionalDangerMoves != null)
        //            {
        //                kingMoves = boardManager.CalculateOverlappingMoves(kingMoves, additionalDangerMoves, false);
        //            }

        //            if (extendedDangerMoves != null)
        //            {
        //                kingMoves = boardManager.CalculateOverlappingMoves(kingMoves, extendedDangerMoves, false);
        //            }
        //            allPossibleMoves.AddRange(kingMoves);
        //        }
        //        else
        //        {
        //            var moves = boardManager.CalculatePossibleMoves(p.GetPositionClass(), p.GetMoveset(), p.GetIsBlack());
        //            if (moves != null)
        //            {
        //                allPossibleMoves.AddRange(moves);
        //            }
        //        }
        //    }
        //}
    }


    public void CalculateLogicPossibleMoves()
    {
        logicPossibleMoves.Clear();
        foreach (var p in logicBoard.pieces)
        {
            if (p.GetIsBlack())
            {
                var moves = boardManager.CalculatePossibleMoves(p.GetPosition(), p.GetMoveset(), p.GetIsBlack());
                if (moves != null)
                {
                    logicPossibleMoves.AddRange(moves);
                }
            }
        }
    }

    //calculate all possibleMoves and include drops

    //do random move for now
    public void MakeRandomMove()
    {
        Tuple<int, int> randomMove = logicPossibleMoves[UnityEngine.Random.Range(0, allPossibleMoves.Count)];

        foreach (var p in logicBoard.pieces)
        {
            if (p.GetIsBlack())
            {
                var pMoves = boardManager.CalculatePossibleMoves(p.GetPosition(), p.GetMoveset(), p.GetIsBlack());

                if (pMoves.Contains(randomMove))
                {
                    source = p.GetPositionTuple();
                    destination = randomMove;
                    logicBoard.ApplyMove(new(source), new(destination));
                    break;
                }
            }
        }
    }

    public Tuple<Position, Position> ApplyMoveToRealBoard()
    {
        var move = GetBestMove(logicBoard, depth);
        if (move != null)
        {
            return move;
        }
        else
        {
            return null;
        }
    }

    public void Display()
    {
        Debug.Log("logic board state");
        logicBoard.DisplayBoardState();
    }

    public Tuple<Position, Position> GetBestMove(LogicBoard board, int depth)
    {
        var res = Minimax(board, depth);

        return res.Item2;
    }

    public Tuple<int, Tuple<Position, Position>> Minimax(LogicBoard board, int depth)
    {
        if (depth == 0)
        {
            return new(board.EvaluateBoard(), null);
        }


        var moves = board.CalculateLogicPossibleMoves();

        if (moves.Count == 0)
        {
            //no more pieces left
            return new(board.EvaluateBoard(), null);
        }

        int maxEval = int.MinValue;
        Tuple<Position, Position> bestMoves = null;

        foreach (var m in moves)
        {
            LogicBoard simulatedBoard = new();
            simulatedBoard.CloneFromLogic(board);

            simulatedBoard.ApplyMove(m.Item1, m.Item2);

            var res = Minimax(simulatedBoard, depth - 1);
            int eval = res.Item1;
            if (eval > maxEval)
            {
                maxEval = eval;
                bestMoves = m;
            }
        }

        return new(maxEval, bestMoves);
    }

    //choose move and execute move

    //get source and destination
    //send it to inputManager
    public List<Tuple<int, int>> GetSourceAndDestination()
    {
        return new List<Tuple<int, int>>()
        {
            source,
            destination
        };
    }

    public Tuple<Position, Position> GetSourceAndDestinationLogic()
        => new(new(source), new(destination));
}