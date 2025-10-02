using System;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Class which holds Shogi bot and Minimax algorithm.
/// </summary>
public class ShogiBot : MonoBehaviour
{
    [SerializeField] private BoardManager boardManager;

    [SerializeField] private KingManager kingManager;

    [SerializeField] private int depth = 3;

    public List<Tuple<int, int>> allPossibleMoves = new();

    public List<Tuple<int, int>> logicPossibleMoves = new();

    private LogicBoard logicBoard = new();

    private int botDifficulty;

    /// <summary>
    /// Clones board state from real board into logic representation - LogicBoard.
    /// </summary>
    /// <param name="grid">Real GameGrid</param>
    /// <param name="kingInDanger">bool current state of King</param>
    /// <param name="attackerPos">Optional attacker position</param>
    public void GetBoardState(Grid grid, bool kingInDanger, Position attackerPos)
    {
        logicBoard.CloneFromReal(grid, kingInDanger, attackerPos);
    }

    //include in board state captured pieces for drops
    //calculate all possibleMoves and include drops

    public void InitializeBot(int botDifficulty)
    {
        this.botDifficulty = botDifficulty;
    }

    /// <summary>
    /// After calculation of the best move from Minimax algorithm it returns best source and destination move.
    /// </summary>
    /// <returns>Best source and destination position</returns>
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

    /// <summary>
    /// Executes Minimax algorithm.
    /// </summary>
    /// <param name="depth">Tree depth</param>
    /// <returns>Best source and destination Position</returns>
    public Tuple<Position, Position> GetBestMove(LogicBoard board, int depth)
    {
        var res = Minimax(board, depth, true, int.MinValue, int.MaxValue);
        return res.Item2;
    }

    /// <summary>
    /// Minimax algorithm for choosing the best possible move with alpha-beta pruning.
    /// </summary>
    /// <param name="board">LogicBoard instance</param>
    /// <param name="depth">Tree depth</param>
    /// <param name="maximizing">Maximizing/Minimazing depends on players turn</param>
    /// <returns>Best source and destination position and board score</returns>
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