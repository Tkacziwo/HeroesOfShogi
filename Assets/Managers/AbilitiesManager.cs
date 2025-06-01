using System;
using System.Collections.Generic;
using UnityEngine;

public class AbilitiesManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    [SerializeField] private GridGame game;

    [SerializeField] private BoardManager boardManager;

    [SerializeField] private FileManager fileManager;

    //ability for dragon
    public Position Inferno(Position killedPiecePosition, bool killedPieceColor)
    {
        List<Position> potentialKills = new();
        int row = 1;
        int col = -1;
        for (int i = 1; i <= 3; i++)
        {
            int destX = col + killedPiecePosition.x;
            int destY = row + killedPiecePosition.y;

            if (boardManager.IsInBoard(destX, destY))
            {
                if (!boardManager.IsCellFree(destX, destY) && !boardManager.IsEnemy(destX, destY, killedPieceColor))
                {
                    potentialKills.Add(new(destX, destY));
                }
            }

            col++;
            if (i % 3 == 0 && i != 0)
            {
                row--;
                col = -1;
            }
        }

        if (potentialKills != null)
        {
            return potentialKills[UnityEngine.Random.Range(0, potentialKills.Count)];

        }
        else
        {
            return null;
        }
    }

    //ability for horse
    public List<Tuple<Position, Position>> Regroup(Position horsePosition, bool horseColor)
    {
        List<Position> friendPositions = new();
        int row = 1;
        int col = -1;
        for (int i = 1; i <= 9; i++)
        {
            int destX = col + horsePosition.x;
            int destY = row + horsePosition.y;

            if (boardManager.IsInBoard(destX, destY))
            {
                if (!boardManager.IsCellFree(destX, destY) && !boardManager.IsEnemy(destX, destY, horseColor))
                {
                    Position pos = new(destX, destY);

                    if (!pos.Equals(horsePosition))
                    {
                        friendPositions.Add(pos);
                    }
                }
            }

            col++;
            if (i % 3 == 0 && i != 0)
            {
                row--;
                col = -1;
            }
        }
        if (friendPositions != null)
        {
            List<Tuple<Position, Position>> validFriendPositions = new();
            foreach (var p in friendPositions)
            {
                var piece = game.GetPieceInGrid(p.x, p.y).GetComponent<Piece>();
                int destY;
                if (piece.GetIsBlack())
                {
                    destY = p.y + 1;
                }
                else
                {
                    destY = p.y - 1;
                }
                bool permitted = false;
                foreach (var g in friendPositions)
                {
                    if (g.Equals(new(p.x, destY)))
                    {
                        permitted = true;
                    }
                }

                if (boardManager.IsInBoard(p.x, destY) && (boardManager.IsCellFree(p.x, destY) || permitted))
                {
                    Tuple<Position, Position> srcDst = new(p,new(p.x, destY));
                    validFriendPositions.Add(srcDst);
                }
            }

            return validFriendPositions;
        }
        else
        {
            return new();
        }
    }

    //gold gen
    public List<Tuple<Position, Position>> Onward(Position goldGenPosition, bool goldGenColor)
    {
        List<Position> friendPositions = new();
        int row = 1;
        int col = -1;
        for (int i = 1; i <= 3; i++)
        {
            int destX = col + goldGenPosition.x;
            int destY = row + goldGenPosition.y;

            if (boardManager.IsInBoard(destX, destY))
            {
                if (!boardManager.IsCellFree(destX, destY) && !boardManager.IsEnemy(destX, destY, goldGenColor))
                {
                    Position pos = new(destX, destY);

                    if (!pos.Equals(goldGenPosition))
                    {
                        friendPositions.Add(pos);
                    }
                }
            }

            col++;
            if (i % 3 == 0 && i != 0)
            {
                row--;
                col = -1;
            }
        }
        if (friendPositions != null)
        {
            List<Tuple<Position, Position>> validFriendPositions = new();
            foreach (var p in friendPositions)
            {
                var piece = game.GetPieceInGrid(p.x, p.y).GetComponent<Piece>();
                int destY;
                if (piece.GetIsBlack())
                {
                    destY = p.y - 1;
                }
                else
                {
                    destY = p.y + 1;
                }

                bool permitted = false;
                foreach (var g in friendPositions)
                {
                    if (g.Equals(new(p.x, destY)))
                    {
                        permitted = true;
                    }
                }

                if (boardManager.IsInBoard(p.x, destY) && (boardManager.IsCellFree(p.x, destY) || permitted))
                {
                    Tuple<Position, Position> srcDst = new(p, new(p.x, destY));
                    validFriendPositions.Add(srcDst);
                }
            }

            return validFriendPositions;
        }
        else
        {
            return new();
        }
    }

    //silver gen
    public List<Tuple<int, int>> Rush(Position silverGenPosition)
    {
        var piece = game.GetPieceInGrid(silverGenPosition).GetComponent<Piece>();

        var pMoves = boardManager.CalculatePossibleMoves(piece);
        return pMoves;
    }

    public void KingPromote(Position src, Position dst)
    {
        var srcPiece = game.GetPieceInGrid(src).GetComponent<Piece>();
        var dstPiece = game.GetPieceInGrid(dst).GetComponent<Piece>();
        var isBlack = srcPiece.GetIsBlack();
        srcPiece.Demote();
        dstPiece.Promote(fileManager.GetMovesetByPieceName("GoldGeneral"));

        var pieceList = isBlack ? game.GetBotPieces() : game.GetPlayerPieces();
        foreach (var p in pieceList)
        {
            if (isBlack)
            {
                p.GetComponentInChildren<MeshRenderer>().material.color = Color.black;
            }
            else
            {
                p.GetComponentInChildren<MeshRenderer>().material.color = Color.white;
            }
        }
    }
}