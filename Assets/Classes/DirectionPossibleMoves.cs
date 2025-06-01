using System;
using System.Collections.Generic;

public class DirectionPossibleMoves
{
    public List<Tuple<int, int>> North { get; set; } = new();

    public List<Tuple<int, int>> East { get; set; } = new();

    public List<Tuple<int, int>> South { get; set; } = new();

    public List<Tuple<int, int>> West { get; set; } = new();

    public List<Tuple<int, int>> North_West { get; set; } = new();

    public List<Tuple<int, int>> North_East { get; set; } = new();

    public List<Tuple<int, int>> South_West { get; set; } = new();

    public List<Tuple<int, int>> South_East { get; set; } = new();
}