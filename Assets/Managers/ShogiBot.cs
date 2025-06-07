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

    private LogicBoard logicBoard = new();

    private int botDifficulty;

    public void GetBoardState(GridGame grid, bool kingInDanger, Position attackerPos)
    {
        logicBoard.CloneFromReal(grid, kingInDanger, attackerPos);
    }

    //include in board state captured pieces for drops
    //calculate all possibleMoves and include drops

    public void InitializeBot(int botDifficulty)
    {
        this.botDifficulty = botDifficulty;
    }


    public Tuple<Position, Position> ApplyMoveToRealBoard()
    {
        var move = GetBestMove(logicBoard, depth);
        return move;
    }

    public void Display()
    {
        Debug.Log("logic board state");
        logicBoard.DisplayBoardState();
    }

    public Tuple<Position, Position> GetBestMove(LogicBoard board, int depth)
    {
        System.Random random = new();
        int chance = random.Next(0, 10);

        bool maximizing;

        if (chance <= botDifficulty)
        {
            maximizing = true;
        }
        else
        {
            maximizing = false;
        }

        var res = Minimax(board, depth, true, int.MinValue, int.MaxValue);

        return res.Item2;
    }

    public Tuple<int, Tuple<Position, Position>> Minimax(LogicBoard board, int depth, bool maximizing, int alpha, int beta)
    {
        if (depth == 0)
        {
            return new(board.EvaluateBoard(), null);
        }

        var moves = board.CalculateLogicPossibleMoves(maximizing);

        if (moves.Count == 0)
        {
            //no more pieces left
            Debug.Log("koniec gry");
            return new(board.EvaluateBoard(), null);
        }

        if (board.kingInDanger)
        {
            depth = 1;
        }

        Tuple<Position, Position> bestMoves = null;

        if (maximizing)
        {
            int maxEval = int.MinValue;
            foreach (var m in moves)
            {
                LogicBoard simulatedBoard = new();
                simulatedBoard.CloneFromLogic(board, board.kingInDanger, board.attackerPos);

                simulatedBoard.ApplyMove(m.Item1, m.Item2);

                var res = Minimax(simulatedBoard, depth - 1, false, alpha, beta);
                int eval = res.Item1;
                if (eval > maxEval)
                {
                    maxEval = eval;
                    bestMoves = m;
                }

                alpha = Math.Max(alpha, eval);
                if (beta <= alpha)
                {
                    break;
                }
            }

            return new(maxEval, bestMoves);
        }
        else
        {
            int minEval = int.MaxValue;
            foreach (var m in moves)
            {
                LogicBoard simulatedBoard = new();
                simulatedBoard.CloneFromLogic(board, board.kingInDanger, board.attackerPos);
                simulatedBoard.ApplyMove(m.Item1, m.Item2);

                var res = Minimax(simulatedBoard, depth - 1, true, alpha, beta);
                int eval = res.Item1;
                if (eval < minEval)
                {
                    minEval = eval;
                    bestMoves = m;
                }

                beta = Math.Min(beta, eval);
                if (beta <= alpha)
                {
                    break;
                }

            }
            return new(minEval, bestMoves);
        }
    }
}