using System;
using System.Collections.Generic;

public static class AIEvents
{
    public static Action<Tuple<int, List<TileInfo>>> OnBotMove;
}