using System;
using System.Collections.Generic;

public class DirectionPossibleMoves
{
    public List<Position> North { get; set; } = new();

    public List<Position> East { get; set; } = new();

    public List<Position> South { get; set; } = new();

    public List<Position> West { get; set; } = new();

    public List<Position> North_West { get; set; } = new();

    public List<Position> North_East { get; set; } = new();

    public List<Position> South_West { get; set; } = new();

    public List<Position> South_East { get; set; } = new();
}