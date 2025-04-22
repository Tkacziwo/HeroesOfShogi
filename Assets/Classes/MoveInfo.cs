using System;

public class MoveInfo
{
    public Tuple<int, int> src;

    public Tuple<int, int> dst;

    public bool isDrop;

    public MoveInfo(Tuple<Tuple<int, int>, Tuple<int, int>> info)
    {
        src = info.Item1;
        dst = info.Item2;
        isDrop = false;
    }

    public MoveInfo(Tuple<Tuple<int, int>, Tuple<int, int>> info, bool isDrop)
    {
        src = info.Item1;
        dst = info.Item2;
        this.isDrop = isDrop;
    }
}