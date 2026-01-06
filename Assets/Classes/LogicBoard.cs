using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Logic counterpart of the Grid class. Holds all cells, pieces and camps for drops.
/// </summary>
public class LogicBoard
{
    public List<Unit> pieces = new();

    public List<Unit> allPieces = new();

    public List<Unit> enemyPieces = new();

    public LogicCell[,] dropCells = new LogicCell[9, 3];

    private readonly int battleWidth = StaticData.battleMapWidth;

    private readonly int battleHeight = StaticData.battleMapHeight;

    public LogicCell[,] cells = new LogicCell[StaticData.battleMapWidth, StaticData.battleMapHeight];
    
    /// <summary>
    /// Clones board from Grid instance to LogicBoard instance and sets all data inside LogicBoard.
    /// The clone is virtually the same as the real counterpart.
    /// </summary>
    /// <param name="grid">Real board</param>
    public void CloneFromReal(Grid grid)
    {
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
                if (cell.unitInCell != null)
                {
                    Unit unit = new(cell.unitInCell.Unit);
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
        var campGrid = grid.eCamp.campGrid;

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                var cell = campGrid[j, i].GetComponent<GridCell>();
                dropCells[j, i] = new(cell);
                if (cell.unitInCell != null)
                {
                    dropCells[j, i].unit = new(cell.unitInCell.Unit);
                    var dropPiecePos = dropCells[j, i].unit.GetPosition();
                    dropCells[j, i].unit.MovePiece(new(dropPiecePos.x + j, dropPiecePos.y + i));
                }
            }
        }
    }

    /// <summary>
    /// Clones another LogicBoard instance into a new one.
    /// Subsequent clones are different from real board, because of Minimax algorithm searching for best possible move.
    /// </summary>
    /// <param name="grid">LogicBoard instance</param>
    /// <param name="kingInDanger">King's state</param>
    /// <param name="attackerPos">optional attacker position</param>
    public void CloneFromLogic(LogicBoard grid)
    {
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

        var campGrid = grid.dropCells;

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                dropCells[j, i] = new(campGrid[j, i]);
                if (campGrid[j, i].unit != null)
                {
                    Unit unit = new(campGrid[j, i].unit);
                    dropCells[j, i].unit = new();
                    dropCells[j, i].unit = unit;
                }
            }
        }
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

            var king = pieces.SingleOrDefault(o => o.UnitName == UnitEnum.King);

            if (king == null)
            {
                score -= 1000000000;
            }
            else if (BoardOperations.IsKingThreatened(king.GetPosition(), enemyPieces, cells))
            {
                score -= 1000000;
            }


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
            var movedPiece = dropCells[src.x - 200, src.y - 200].unit;
            dropCells[src.x - 200, src.y - 200].unit = null;
            movedPiece.MovePiece(dst);
            movedPiece.ResetIsDrop();
            cells[dst.x, dst.y].unit = new(movedPiece);
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
                                destination = BoardOperations.FindPositionBeforeEnemy(row, col, unitPos, defender.GetPosition());
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

        //rebuild state
        allPieces.Clear();
        pieces.Clear();
        enemyPieces.Clear();

        for (int y = 0; y < battleHeight; y++)
        {
            for (int x = 0; x < battleWidth; x++)
            {
                var u = cells[x, y].unit;
                if (u == null) continue;
                allPieces.Add(u);
                if (u.GetIsBlack()) pieces.Add(u);
                else enemyPieces.Add(u);
            }
        }
    }

    public void ResetMovedFlagsForSide(List<Unit> sideUnits)
    {
        foreach (var u in sideUnits)
            u.MovedInTurn = false;
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

        foreach (var p in usedPieces)
        {
            if (p.MovedInTurn) continue;

            List<Position> moves = new();
           
            moves = BoardOperations.CalculatePossibleMoves(p, cells);
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

        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < 9; x++)
            {
                if (dropCells[x, y] != null && dropCells[x, y].unit != null)
                {
                    var unit = dropCells[x, y].unit;
                    var dropsMoves = BoardOperations.CalculatePossibleDrops(unit, cells);
                    dropsMoves = BoardOperations.MultiplyDropsByWeight(dropsMoves);
                    if (dropsMoves != null || dropsMoves.Count != 0)
                    {
                        Position src = unit.GetPosition();
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

        return logicSrcDstMoves;
    }
}