using System;
using System.Collections.Generic;
using UnityEngine;

public class LogicBoard
{
    public LogicCell[,] cells = new LogicCell[9, 9];

    public List<LogicPiece> pieces = new();

    public List<LogicPiece> allPieces = new();

    private readonly LogicBoardManager manager = new();

    public void CloneFromReal(GridGame grid)
    {
        pieces.Clear();
        allPieces.Clear();
        cells = new LogicCell[9, 9];
        for (int y = 0; y < 9; y++)
        {
            for (int x = 0; x < 9; x++)
            {
                var cell = grid.GetGridCell(x, y);
                cells[x, y] = new LogicCell(cell);
                if (cell.objectInThisGridSpace != null)
                {
                    var piece = cell.objectInThisGridSpace.GetComponent<Piece>();
                    LogicPiece p = new(piece);
                    if (p.GetIsBlack())
                    {
                        //p.ReverseMovementMatrix();
                        pieces.Add(p);
                    }
                    allPieces.Add(p);
                    cells[x, y].piece = p;
                }
            }
        }
    }

    public void CloneFromLogic(LogicBoard grid)
    {
        pieces.Clear();
        allPieces.Clear();
        cells = new LogicCell[9, 9];
        for (int y = 0; y < 9; y++)
        {
            for (int x = 0; x < 9; x++)
            {
                var cell = grid.cells[x, y];
                cells[x, y] = new LogicCell(cell);
                if (cell.piece != null)
                {
                    var piece = cell.piece;
                    LogicPiece p = new(piece);
                    if (p.GetIsBlack())
                    {
                        pieces.Add(p);
                    }
                    allPieces.Add(p);
                    cells[x, y].piece = p;
                }
            }
        }
    }

    public int EvaluateBoard()
    {
        int score = 0;
        foreach (var p in allPieces)
        {
            int pieceValue = p.value;
            score += p.GetIsBlack() ? pieceValue : -pieceValue;
        }
        return score;
    }

    public void ApplyMove(Position src, Position dst)
    {
        var movedPiece = cells[src.x, src.y].piece;
        cells[src.x, src.y].piece = null;
        movedPiece.MovePiece(dst);
        cells[dst.x, dst.y].piece = new(movedPiece);
    }

    public List<Tuple<Position, Position>> CalculateLogicPossibleMoves()
    {
        List<Tuple<Position, Position>> logicSrcDstMoves = new();
        foreach (var p in pieces)
        {
            if (p.GetIsBlack())
            {
                var moves = manager.CalculatePossibleMoves(p.GetPosition(), p.GetMoveset(), p.GetIsBlack(), cells);
                if (moves != null)
                {
                    Position src = p.GetPosition();
                    foreach (var m in moves)
                    {
                        if (m != null)
                        {
                            Position dst = m;
                            logicSrcDstMoves.Add(new(src, dst));
                        }
                    }
                }
            }
        }
        return logicSrcDstMoves;
    }

    public void DisplayBoardState()
    {
        string whole = "";
        for (int y = 8; y >= 0; y--)
        {
            string rowStr = "|";
            for (var x = 0; x < 9; x++)
            {
                var cell = cells[x, y];
                if (cell.piece != null)
                {
                    rowStr += $"[{cell.piece.pieceName.Substring(0, 1)}]";
                }
                else
                {
                    rowStr += "[ ]";
                }
            }
            rowStr += "|";
            whole += rowStr + "\n";
        }

        Debug.Log(whole);
    }
}