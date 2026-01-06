using UnityEngine;

/// <summary>
/// Cloneable cell used in minimax;
/// </summary>
public class LogicCell
{
    private readonly Position pos;

    public Unit unit = null;

    public LogicCell(GridCell cell)
    {
        pos = new(cell.GetPosition());
    }

    public LogicCell(LogicCell cell)
    {
        pos = new(cell.GetPosition());
    }

    public Position GetPosition()
        => pos;
}