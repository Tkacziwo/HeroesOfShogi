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

    private int botDifficulty;

    public void GetBoardState(GridGame grid, bool kingInDanger, Position attackerPos, List<Position> extendedDangerMoves)
    {
        logicBoard.CloneFromReal(grid, kingInDanger, attackerPos, extendedDangerMoves);
    }

    //include in board state captured pieces for drops
    //calculate all possibleMoves and include drops

    public void InitializeBot(int botDifficulty)
    {
        this.botDifficulty = botDifficulty;
    }


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
            Debug.Log("koniec gry");
            return new(board.EvaluateBoard(), null);
        }

        int maxEval = int.MinValue;
        Tuple<Position, Position> bestMoves = null;
        if (board.kingInDanger)
        {
            depth = 1;
        }

        foreach (var m in moves)
        {
            LogicBoard simulatedBoard = new();
            simulatedBoard.CloneFromLogic(board, board.kingInDanger, board.attackerPos, board.extendedDangerMoves);

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