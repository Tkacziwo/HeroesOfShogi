using System;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public class LogicBoard
{
    public LogicCell[,] cells = new LogicCell[9, 9];

    public List<LogicPiece> pieces = new();

    public List<LogicPiece> allPieces = new();

    private readonly LogicBoardManager manager = new();

    private readonly LogicKingManager kingManager = new();

    public bool kingInDanger;

    public Position attackerPos = null;

    public List<Position> extendedDangerMoves = new();

    public LogicCell[,] dropCells = new LogicCell[9, 3];

    public void CloneFromReal(GridGame grid, bool kingInDanger, Position attackerPos, List<Position> extendedDangerMoves)
    {
        this.kingInDanger = kingInDanger;
        this.attackerPos = new(attackerPos);
        this.extendedDangerMoves = new(extendedDangerMoves);
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

        //if (grid.eCamp.capturedPieces.Count != 0)
        //{
        //    foreach (var d in grid.eCamp.capturedPieces)
        //    {
        //        drops.Add(new(d));
        //    }
        //}

        var campGrid = grid.eCamp.campGrid;

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                var cell = campGrid[j, i].GetComponent<GridCell>();
                dropCells[j, i] = new(cell);
                if (cell.objectInThisGridSpace != null)
                {
                    dropCells[j, i].piece = new(cell.objectInThisGridSpace.GetComponent<Piece>());
                }
            }
        }
    }

    public void CloneFromLogic(LogicBoard grid, bool kingInDanger, Position attackerPos, List<Position> extendedDangerMoves)
    {
        this.kingInDanger = kingInDanger;
        this.attackerPos = new(attackerPos);
        this.extendedDangerMoves = new(extendedDangerMoves);
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

        //if (grid.drops.Count != 0)
        //{
        //    foreach (var d in grid.drops)
        //    {
        //        drops.Add(new(d));
        //    }
        //}

        var campGrid = grid.dropCells;

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                dropCells[j, i] = new(campGrid[j, i]);
                if (campGrid[j, i].piece != null)
                {
                    LogicPiece logicPiece = new(campGrid[j, i].piece);
                    dropCells[j, i].piece = new();
                    dropCells[j, i].piece = logicPiece;
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
        if (src.x > 9 || src.y > 9)
        {
            var movedPiece = dropCells[src.x - 200, src.y - 200].piece;
            dropCells[src.x - 200, src.y - 200].piece = null;
            movedPiece.MovePiece(dst);
            cells[dst.x, dst.y].piece = new(movedPiece);
        }
        else
        {
            var movedPiece = cells[src.x, src.y].piece;
            cells[src.x, src.y].piece = null;
            movedPiece.MovePiece(dst);
            cells[dst.x, dst.y].piece = new(movedPiece);
        }
    }

    public List<Tuple<Position, Position>> CalculateLogicPossibleMoves()
    {
        List<Tuple<Position, Position>> logicSrcDstMoves = new();
        List<Tuple<LogicPiece, Position>> logicDropDstMoves = new();
        if (kingInDanger)
        {
            return HandleKingInDanger();
        }
        else
        {
            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < 9; x++)
                {
                    if (dropCells[x, y] != null && dropCells[x, y].piece != null)
                    {
                        var piece = dropCells[x, y].piece;
                        var dropsMoves = manager.CalculatePossibleDrops(cells, piece);
                        if (dropsMoves != null)
                        {
                            Position src = piece.GetPosition();
                            foreach (var m in dropsMoves)
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
            }

            foreach (var p in pieces)
            {
                List<Position> moves = new();
                if (p.isKing)
                {
                    moves = kingManager.CloseScan(p.GetPosition(), cells);

                    var copy = new List<Position>();
                    copy.AddRange(moves);
                    foreach (var pMoves in moves)
                    {
                        var res = kingManager.FarScanForKing(pMoves, p.GetIsBlack(), ref attackerPos, cells);
                        if (res)
                        {
                            copy.Remove(pMoves);
                        }
                    }
                    moves = copy;
                }
                else
                {
                    moves = manager.CalculatePossibleMoves(p.GetPosition(), p.GetMoveset(), p.GetIsBlack(), cells);
                }
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

    private List<Tuple<Position, Position>> HandleKingInDanger()
    {
        List<Tuple<Position, Position>> logicSrcDstMoves = new();
        foreach (var p in pieces)
        {
            if (p.isKing)
            {
                var moves = kingManager.CloseScan(p.GetPosition(), cells);
                var attacker = cells[attackerPos.x, attackerPos.y].piece;
                var attackerProtected = kingManager.FarScan(attackerPos, attacker.GetIsBlack(), cells);
                var additionalDangerMoves = kingManager.KingDangerMovesScan(moves, p.GetIsBlack(), cells);

                List<Position> aPos = new()
                {
                    attackerPos
                };

                if (additionalDangerMoves != null)
                {
                    moves = manager.CalculateOverlappingMoves(moves, additionalDangerMoves, false);
                }

                if (attackerProtected)
                {
                    moves = manager.CalculateOverlappingMoves(moves, aPos, false);
                }

                if (extendedDangerMoves != null)
                {
                    moves = manager.CalculateOverlappingMoves(moves, extendedDangerMoves, false);
                }

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
            else
            {
                //bodyguard checking
                //foreach (var b in bodyguards)
                //{
                //    if (hoveredCell.GetPositionTuple().Equals(b.GetPositionTuple()))
                //    {
                //        PossibleMovesCalculationHandler(piece, hoveredCell, true);
                //        break;
                //    }
                //}
                //drop checking
                //if (piece.GetIsDrop())
                //{
                //    PossibleMovesCalculationHandler(piece, hoveredCell);
                //}
                //sacrifice checking
                //if (sacrifices != null)
                //{
                //    foreach (var s in sacrifices)
                //    {
                //        if (hoveredCell.GetPositionTuple().Equals(s.GetPositionTuple()))
                //        {
                //            if (endangeredMoves != null)
                //            {
                //                possibleMoves = kingManager.CalculateProtectionMoves(piece.GetPositionClass(), piece.GetMoveset(), piece.GetIsBlack(), endangeredMoves); ;
                //            }

                //            foreach (var r in possibleMoves)
                //            {
                //                var cell = gameGrid.gameGrid[r.Item1, r.Item2].GetComponent<GridCell>();
                //                cell.SetIsPossibleMove();
                //                cell.GetComponentInChildren<SpriteRenderer>().material.color = Color.black;
                //            }
                //            break;
                //        }
                //    }
                //}
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