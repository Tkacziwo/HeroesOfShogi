using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Logic counterpart of the Grid class. Holds all cells, pieces and camps for drops.
/// </summary>
public class LogicBoard
{
    public List<Unit> pieces = new();

    public List<Unit> allPieces = new();

    public List<Unit> enemyPieces = new();

    private readonly LogicBoardManager manager = new();

    private readonly LogicKingManager kingManager = new();

    public bool kingInDanger;

    public Position attackerPos = null;

    public LogicCell[,] dropCells = new LogicCell[9, 3];

    private readonly int battleWidth = StaticData.battleMapWidth;

    private readonly int battleHeight = StaticData.battleMapHeight;

    public int remainingMoves = 3;

    public bool isPlayerTurn = true;

    public LogicCell[,] cells = new LogicCell[StaticData.battleMapWidth, StaticData.battleMapHeight];
    /// <summary>
    /// Clones board from Grid instance to LogicBoard instance and sets all data inside LogicBoard.
    /// The clone is virtually the same as the real counterpart.
    /// </summary>
    /// <param name="grid">Real board</param>
    /// <param name="kingInDanger">state of King</param>
    /// <param name="attackerPos">optional attacker position</param>
    public void CloneFromReal(Grid grid, bool kingInDanger, Position attackerPos)
    {
        this.kingInDanger = kingInDanger;
        this.attackerPos = new(attackerPos);
        pieces.Clear();
        allPieces.Clear();
        enemyPieces.Clear();

        cells = new LogicCell[battleWidth, battleHeight];

        for (int y = 0; y < battleHeight; y++)
        {
            for (int x = 0; x < battleWidth; x++)
            {
                var cell = grid.GetGridCell(x, y);
                cells[x, y] = new LogicCell(cell);
                if (cell.unitInGridCell != null)
                {
                    Unit unit = new(cell.unitInGridCell.Unit);
                    unit.MovedInTurn = false;
                    if (unit.GetIsBlack())
                    {
                        pieces.Add(unit);
                    }
                    else
                    {
                        enemyPieces.Add(unit);
                    }

                    allPieces.Add(unit);
                    cells[x, y].unit = unit;
                }
            }
        }

        //[ToDo] rethink drops
        //var campGrid = grid.eCamp.campGrid;

        //for (int i = 0; i < 3; i++)
        //{
        //    for (int j = 0; j < 9; j++)
        //    {
        //        var cell = campGrid[j, i].GetComponent<GridCell>();
        //        dropCells[j, i] = new(cell);
        //        if (cell.objectInThisGridSpace != null)
        //        {
        //            dropCells[j, i].piece = new(cell.objectInThisGridSpace.GetComponent<Piece>());
        //            var dropPiecePos = dropCells[j, i].piece.GetPosition();
        //            dropCells[j, i].piece.SetPosition(new(dropPiecePos.x + j, dropPiecePos.y + i));
        //        }
        //    }
        //}
    }

    /// <summary>
    /// Clones another LogicBoard instance into a new one.
    /// Subsequent clones are different from real board, because of Minimax algorithm searching for best possible move.
    /// </summary>
    /// <param name="grid">LogicBoard instance</param>
    /// <param name="kingInDanger">King's state</param>
    /// <param name="attackerPos">optional attacker position</param>
    public void CloneFromLogic(LogicBoard grid, bool kingInDanger, Position attackerPos)
    {
        this.kingInDanger = kingInDanger;
        this.attackerPos = new(attackerPos);
        pieces.Clear();
        allPieces.Clear();
        enemyPieces.Clear();

        cells = new LogicCell[battleWidth, battleHeight];
        for (int y = 0; y < battleHeight; y++)
        {
            for (int x = 0; x < battleWidth; x++)
            {
                var cell = grid.cells[x, y];
                cells[x, y] = new LogicCell(cell);
                if (cell.unit != null)
                {
                    Unit unit = new(cell.unit);
                    unit.MovedInTurn = false;
                    if (unit.GetIsBlack())
                    {
                        pieces.Add(unit);
                    }
                    else
                    {
                        enemyPieces.Add(unit);
                    }
                    allPieces.Add(unit);
                    cells[x, y].unit = unit;
                }
            }
        }

        //var campGrid = grid.dropCells;

        //for (int i = 0; i < 3; i++)
        //{
        //    for (int j = 0; j < 9; j++)
        //    {
        //        dropCells[j, i] = new(campGrid[j, i]);
        //        if (campGrid[j, i].piece != null)
        //        {
        //            LogicPiece logicPiece = new(campGrid[j, i].piece);
        //            dropCells[j, i].piece = new();
        //            dropCells[j, i].piece = logicPiece;
        //        }
        //    }
        //}
    }

    /// <summary>
    /// Evaluates board score to determine best gain / loss
    /// </summary>
    /// <returns>int</returns>
    public int EvaluateBoard()
    {
        int score = 0;
        foreach (var p in allPieces)
        {
            int pieceValue = p.value;

            pieceValue += p.HealthPoints * 2;
            pieceValue += p.AttackPower * 3;


            score += p.GetIsBlack() ? pieceValue : -pieceValue;
        }
        return score;
    }

    /// <summary>
    /// Applies move to the logic board.
    /// </summary>
    /// <param name="src">source position</param>
    /// <param name="dst">destination position</param>
    public void ApplyMove(Position src, Position dst)
    {
        if (src.x > battleWidth || src.y > battleHeight)
        {
            //var movedPiece = dropCells[src.x - 200, src.y - 200].piece;
            //dropCells[src.x - 200, src.y - 200].piece = null;
            //movedPiece.MovePiece(dst);
            //movedPiece.ResetIsDrop();
            //cells[dst.x, dst.y].piece = new(movedPiece);
        }
        else
        {
            var movedUnit = cells[src.x, src.y].unit;

            var destinationCell = cells[dst.x, dst.y];
            if (destinationCell.unit == null)
            {
                cells[src.x, src.y].unit = null;
                movedUnit.MovePiece(dst);
                cells[dst.x, dst.y].unit = new(movedUnit);
            }
            else
            {
                Unit defender = destinationCell.unit;
                defender.ReduceHP(movedUnit.AttackPower);
                if (defender.HealthPoints <= 0)
                {
                    cells[src.x, src.y].unit = null;
                    movedUnit.MovePiece(dst);
                    cells[dst.x, dst.y].unit = new(movedUnit);
                }
                else
                {
                    if (movedUnit.UnitName == UnitEnum.Bishop || movedUnit.UnitName == UnitEnum.Rook || movedUnit.UnitName == UnitEnum.Lance)
                    {
                        var unitPos = movedUnit.GetPosition();


                        Position destination = null;

                        for (int row = -1; row <= 1; row++)
                        {
                            for (int col = -1; col <= 1; col++)
                            {
                                destination = manager.FindPositionBeforeEnemy(row, col, unitPos, cells, defender.GetPosition());
                            }
                        }

                        if (destination != null)
                        {
                            cells[src.x, src.y].unit = null;
                            movedUnit.MovePiece(destination);
                            cells[destination.x, destination.y].unit = new(movedUnit);
                        }
                    }
                }
            }
            movedUnit.MovedInTurn = true;
        }

        //allPieces.Clear();
        //pieces.Clear();
        //enemyPieces.Clear();

        //for (int y = 0; y < battleHeight; y++)
        //{
        //    for (int x = 0; x < battleWidth; x++)
        //    {
        //        var u = cells[x, y].unit;
        //        if (u == null) continue;
        //        allPieces.Add(u);
        //        if (u.GetIsBlack()) pieces.Add(u);
        //        else enemyPieces.Add(u);
        //    }
        //}
    }

    /// <summary>
    /// Calculates all possible legal moves for the current logic board state.
    /// </summary>
    /// <param name="maximizing">Whether algorithm maximizes/minimizes</param>
    /// <returns></returns>
    public List<Tuple<Position, Position>> CalculateLogicPossibleMoves(bool maximizing)
    {
        List<Tuple<Position, Position>> logicSrcDstMoves = new();
        List<Unit> usedPieces = maximizing ? new(pieces) : new(enemyPieces);

        //if (kingInDanger)
        //{
        //return HandleKingInDanger();
        //}
        //else
        //{
        //if (maximizing)
        //{
        //    for (int y = 0; y < 3; y++)
        //    {
        //        for (int x = 0; x < 9; x++)
        //        {
        //            if (dropCells[x, y] != null && dropCells[x, y].piece != null)
        //            {
        //                var piece = dropCells[x, y].piece;
        //                var dropsMoves = manager.CalculatePossibleDrops(cells, piece);
        //                dropsMoves = manager.MultiplyDropsByWeight(dropsMoves);
        //                if (dropsMoves != null || dropsMoves.Count != 0)
        //                {
        //                    Position src = piece.GetPosition();
        //                    foreach (var m in dropsMoves)
        //                    {
        //                        if (m != null)
        //                        {
        //                            Position dst = m;
        //                            logicSrcDstMoves.Add(new(src, dst));
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        foreach (var p in usedPieces)
        {
            List<Position> moves = new();
            //if (p.isKing)
            //{
            //    moves = kingManager.ValidMovesScan(p, enemyPieces, cells);
            //}
            //else
            //{
            moves = manager.CalculatePossibleMoves(p, cells);
            //manager.CheckIfMovesAreLegal(ref moves, p, allPieces, cells);
            //}
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
        //}
        return logicSrcDstMoves;
    }

    /// <summary>
    /// Handler for when King is in danger. Behaves similarly to InputManager handler counterpart.
    /// </summary>
    /// <returns>List of source and destination positions</returns>
    private List<Tuple<Position, Position>> HandleKingInDanger()
    {
        throw new NotImplementedException();
        //List<Tuple<Position, Position>> logicSrcDstMoves = new();

        //var endangeredMoves = new List<Position>();

        //foreach (var p in pieces)
        //{
        //    if (p.isKing)
        //    {
        //        var attacker = cells[attackerPos.x, attackerPos.y].piece;
        //        endangeredMoves = kingManager.CalculateEndangeredMoves(attacker, p.GetPosition());
        //    }
        //}

        //foreach (var p in pieces)
        //{
        //    if (p.isKing && p.GetIsBlack())
        //    {
        //        var moves = kingManager.ValidMovesScan(p, enemyPieces, cells);
        //        var attacker = cells[attackerPos.x, attackerPos.y].piece;

        //        bool attackerProtected = kingManager.IsAttackerProtected(attacker, enemyPieces, cells);
        //        if (attackerProtected && moves.Contains(attacker.GetPosition()))
        //        {
        //            moves.Remove(attacker.GetPosition());
        //        }

        //        var attackerPossibleMovesUnrestricted = manager.ScanMovesUnrestricted(attacker);
        //        if (attackerPossibleMovesUnrestricted != null)
        //        {
        //            moves = manager.CalculateOverlappingMoves(moves, attackerPossibleMovesUnrestricted, false);
        //        }

        //        var additionalDangerMoves = kingManager.KingDangerMovesScan(moves, p.GetIsBlack(), cells);
        //        if (additionalDangerMoves != null && additionalDangerMoves.Count != 0)
        //        {
        //            moves = manager.CalculateOverlappingMoves(moves, additionalDangerMoves, false);
        //        }

        //        // calculate all possible valid moves
        //        Position src = p.GetPosition();
        //        foreach (var m in moves)
        //        {
        //            if (m != null)
        //            {
        //                Position dst = m;
        //                logicSrcDstMoves.Add(new(src, dst));
        //            }
        //        }
        //    }
        //}

        ////bodyguard checking
        //var bodyguards = kingManager.FindGuards(attackerPos, pieces, cells);
        //if (bodyguards != null)
        //{
        //    foreach (var b in bodyguards)
        //    {
        //        logicSrcDstMoves.Add(new(b.GetPosition(), attackerPos));
        //    }
        //}

        ////drop checking
        //for (int y = 0; y < 3; y++)
        //{
        //    for (int x = 0; x < 9; x++)
        //    {
        //        if (dropCells[x, y] != null && dropCells[x, y].piece != null)
        //        {
        //            var piece = dropCells[x, y].piece;
        //            var dropsMoves = manager.CalculatePossibleDrops(cells, piece);
        //            if (dropsMoves != null)
        //            {
        //                Position src = piece.GetPosition();
        //                foreach (var dangerMove in endangeredMoves)
        //                {
        //                    if (!dangerMove.Equals(attackerPos))
        //                    {
        //                        logicSrcDstMoves.Add(new(src, dangerMove));
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        ////sacrifice checking
        //var sacrifices = kingManager.FindSacrifices(endangeredMoves, pieces, cells);
        //if (sacrifices != null && endangeredMoves != null)
        //{
        //    foreach (var s in sacrifices)
        //    {
        //        var sacrificeMoves = kingManager.CalculateProtectionMoves(s, endangeredMoves, cells);
        //        var sacrificePosition = s.GetPosition();
        //        foreach (var dst in sacrificeMoves)
        //        {
        //            logicSrcDstMoves.Add(new(sacrificePosition, dst));
        //        }
        //    }
        //}

        //return logicSrcDstMoves;
    }
}