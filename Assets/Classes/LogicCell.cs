using System;

public class LogicCell
{
    private Position pos;

    private bool isPossibleMove = false;

    public LogicPiece piece = null;

    public LogicCell(GridCell cell)
    {
        pos = new(cell.GetPosition());
    }

    public LogicCell(LogicCell cell)
    {
        pos = new(cell.GetPosition());
        //piece = new();
        //if (cell.piece != null)
        //{
        //    var piece = cell.piece;
        //    this.piece = new(piece);
        //}
    }

    public void InitializeGridCell(int x, int y, float cellSize)
    {
        pos = new Position(x, y);
        isPossibleMove = false;
    }

    public void SetPosition(int x, int y)
    {
        pos.SetPosition(x, y);
    }

    public void SetPiece(LogicPiece piece)
    {
        this.piece = piece;
    }

    public void SetAndMovePiece(LogicPiece piece)
    {
        this.piece = piece;
    }

    public Position GetPosition()
        => pos;

    public bool GetIsPossibleMove()
        => isPossibleMove;

    public bool SetIsPossibleMove()
        => isPossibleMove = true;

    public bool ResetIsPossibleMove()
        => isPossibleMove = false;
}