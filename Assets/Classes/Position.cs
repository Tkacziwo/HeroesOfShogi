using System;

public class Position : IEquatable<Position>
{
    public int x;
    public int y;

    public Position()
    {
        x = 999; y = 999;
    }

    public Position(int x, int y)
    {
        this.x = x; this.y = y;
    }

    public Position(Position o)
    {
        this.x = o.x; this.y = o.y;
    }

    public Position(Tuple<int, int> newPos)
    {
        if (newPos == null)
        {
            this.x = 100;
            this.y = 100;
        }
        else
        {
            this.x = newPos.Item1;
            this.y = newPos.Item2;
        }
    }

    public Position GetPosition()
        => this;

    public void SetPosition(int x, int y)
    {
        this.x = x; this.y = y;
    }

    public void SetPosition(Tuple<int, int> pos)
    {
        x = pos.Item1; y = pos.Item2;
    }

    public bool Equals(Position other)
    {
        return other != null && other.x == this.x && other.y == this.y;
    }
}