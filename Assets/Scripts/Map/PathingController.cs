using NUnit.Framework.Constraints;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.ShaderGraph;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileInfo
{
    public bool isTraversable;

    public Vector3Int position;

    public float gCost;

    public float hCost;

    public float fCost;

    public Position positionInArray;

    public bool isBuilding;

    public TileInfo parent;

    public void CalculateGCost(float previousGCost, bool isDiagonal)
    {
        float add = isDiagonal ? 1.4f : 1;
        gCost = previousGCost + add;
    }

    public void CalculateHCost(Vector3Int endPos)
    {
        hCost = Mathf.Sqrt(Mathf.Pow(Mathf.Abs(this.position.x - endPos.x), 2) + Mathf.Pow(Mathf.Abs(this.position.y - endPos.y), 2));
    }

    public void CalculateFCost()
    {
        fCost = gCost + hCost;
    }
}

public class PathingController
{
    private int MapWidth { get; set; }

    public int MapHeight { get; set; }

    private TileInfo[,] MapTiles { get; set; }

    private List<TileInfo> closedList = new();

    private TileInfo start;

    private Vector3Int startPos;

    private Vector3Int endPos;

    private TileInfo end;

    public void SetParameters(Tilemap grid, Vector3Int start, Vector3Int end)
    {
        MapWidth = grid.cellBounds.xMax;
        MapHeight = grid.cellBounds.yMax;
        MapTiles = new TileInfo[MapHeight, MapWidth];
        int virtualPosX = 0, virtualPosY = 0;
        try
        {
            for (int y = 0; y < MapHeight; y++)
            {

                for (int x = 0; x < MapWidth; x++)
                {
                    Vector3Int vec = new(x, y, grid.cellBounds.z);
                    var tile = grid.GetTile<MapTile>(vec);
                    TileInfo tileInfo = new()
                    {
                        isTraversable = tile != null && tile.IsTraversable,
                        position = vec,
                        parent = null,
                        positionInArray = new(virtualPosX, virtualPosY)
                    };
                    if (vec == start)
                    {
                        tileInfo.isTraversable = true;
                        this.start = tileInfo;
                    }
                    else if (vec == end)
                    {
                        tileInfo.isTraversable = true;
                        this.end = tileInfo;
                    }

                    MapTiles[virtualPosY, virtualPosX] = tileInfo;
                    virtualPosX++;
                }
                virtualPosX = 0;
                virtualPosY++;
            }

            this.startPos = start;
            this.endPos = end;
        }
        catch (Exception ex)
        {
            Debug.Log(ex.Message + ex.InnerException + ex.StackTrace);
            throw;
        }
    }

    public bool IsInGrid(Position pos)
    {
        if (pos.x >= 0 && pos.x < MapWidth && pos.y >= 0 && pos.y < MapHeight)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public bool IsDiagonal(Vector3Int ancestor, Vector3Int successor)
    {
        if (ancestor.x != successor.x && ancestor.y != successor.y)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private List<TileInfo> FindNeighbours(TileInfo tileInfo)
    {
        List<TileInfo> neighbours = new();
        var pos = tileInfo.positionInArray;
        for (int row = -1; row <= 1; row++)
        {
            for (int col = -1; col <= 1; col++)
            {
                int destX = col + pos.x;
                int destY = row + pos.y;
                if (IsInGrid(new(destX, destY)))
                {
                    var info = MapTiles[destY, destX];
                    if (info.isTraversable &&
                        !(info.positionInArray.x == pos.x && info.positionInArray.y == pos.y))
                    {
                        neighbours.Add(info);
                    }
                }
            }
        }

        return neighbours;
    }

    public List<TileInfo> TraceBackPath()
    {
        List<TileInfo> path = new();
        if(closedList.Count == 0)
        {
            return path;
        }

        var last = closedList.Last();
        path.Add(last);
        TileInfo parent = last.parent;
        while (true)
        {
            var next = closedList.SingleOrDefault(o => o.position.x == parent.position.x && o.position.y == parent.position.y);
            if (next != null)
            {
                path.Add(next);
                parent = next.parent;
            }
            else
            {
                break;
            }
        }

        return path;
    }

    public List<TileInfo> FindPath()
    {
        closedList.Clear();
        var s = start.positionInArray;
        int iterations = 0;

        Vector3Int vec = new(s.x, s.y);
        // Holds unexplored paths
        List<TileInfo> openList = new();

        TileInfo startTile = MapTiles[s.y, s.x];
        openList.Add(startTile);

        // Setting init values to starting point
        MapTiles[s.y, s.x].gCost = 0;
        MapTiles[s.y, s.x].CalculateHCost(endPos);
        MapTiles[s.y, s.x].fCost = 0 + MapTiles[s.y, s.x].hCost;

        while (openList.Count != 0 && iterations < int.MaxValue)
        {
            // Find tile with lowest fCost
            var q = FindLowestFCostTile(openList);
            openList.Remove(q);

            // Generating neighbours
            var neighbours = FindNeighbours(q);

            // For each neighbour set their parent to previous (first iteration is start).
            foreach (var n in neighbours)
            {
                if (n.position.x == endPos.x && n.position.y == endPos.y)
                {
                    //Found path;
                    closedList.Add(q);
                    closedList.Remove(startTile);
                    var path = TraceBackPath();
                    path.Reverse();
                    return path;
                }
                else
                {
                    HandleNeighbourCalculations(n, q, endPos);
                }
               
                if (closedList.Select(o => o.positionInArray).Contains(n.positionInArray))
                {
                    continue;
                }
                else if (openList.Select(o => o.positionInArray).Contains(n.positionInArray))
                {
                    var tile = openList.Single(o => o.positionInArray.x == n.positionInArray.x && o.positionInArray.y == n.positionInArray.y);
                    if (tile.fCost < n.fCost)
                    {
                        tile.gCost = n.gCost;
                        tile.hCost = n.hCost;
                        tile.fCost = n.fCost;
                        tile.parent = n.parent;
                    }
                }
                else
                {
                    n.parent = q;
                    openList.Add(n);
                }

            }
            iterations++;
            closedList.Add(q);
        }
        Debug.Log("Could not find the path");
        return new();
    }

    public void HandleNeighbourCalculations(TileInfo successor, TileInfo ancestor, Vector3Int endPosition)
    {
        var gCost = ancestor.gCost;
        bool diagonal = IsDiagonal(ancestor.position, successor.position);

        successor.CalculateGCost(gCost, diagonal);
        successor.CalculateHCost(endPosition);
        successor.fCost = successor.gCost + successor.hCost;
    }

    public TileInfo FindLowestFCostTile(List<TileInfo> openList)
    {
        float min = float.MaxValue;
        TileInfo minTile = openList[0];
        foreach (var item in openList)
        {
            if (item.fCost < min)
            {
                min = item.fCost;
                minTile = item;
            }
        }

        return minTile;
    }
}