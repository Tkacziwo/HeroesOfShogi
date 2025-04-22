using System;
using UnityEngine;
using System.Collections.Generic;

public class ShogiBot : MonoBehaviour
{
    private GridGame grid;

    [SerializeField] private BoardManager boardManager;

    public List<Tuple<int, int>> allPossibleMoves;

    public List<Piece> pieces;

    private Tuple<int, int> source;

    private Tuple<int, int> destination;


    public void Start()
    {
        grid = FindFirstObjectByType<GridGame>();
    }


    //get board state on initialization (maybe)
    //for now get board state every turn (not optimal)
    public void GetBoardState()
    {
        pieces = null;
        pieces = new List<Piece>();
        for (int y = 0; y < 9; y++)
        {
            for (int x = 0; x < 9; x++)
            {
                var piece = grid.GetPieceInGrid(x, y);
                if (piece != null)
                {
                    var p = piece.GetComponent<Piece>();
                    if (p.GetIsBlack())
                    {
                        pieces.Add(p);
                    }
                }
            }
        }
    }

    //include in board state captured pieces for drops


    //calculate all possibleMoves
    public void CalculateAllPossibleMoves(bool kingInDanger,
        Tuple<int, int> kingPos = null,
        List<Tuple<int, int>> bodyguards = null,
        List<Tuple<int, int>> sacrifices = null)
    {
        allPossibleMoves = new();
        if (kingInDanger)
        {
           
        }
        else
        {
            foreach (var p in pieces)
            {
                var moves = boardManager.CalculatePossibleMoves(p.GetPosition(), p.GetMoveset(), p.GetIsBlack());
                if (moves != null)
                {
                    allPossibleMoves.AddRange(moves);
                }
            }
        }
    }

    //calculate all possibleMoves and include drops

    //do random move for now
    public void MakeRandomMove()
    {
        Tuple<int, int> randomMove = allPossibleMoves[UnityEngine.Random.Range(0, allPossibleMoves.Count)];

        foreach (var p in pieces)
        {
            var pMoves = boardManager.CalculatePossibleMoves(p.GetPosition(), p.GetMoveset(), p.GetIsBlack());

            if (pMoves.Contains(randomMove))
            {
                source = p.GetPositionTuple();
                destination = randomMove;
            }
        }
    }

    //choose move and execute move

    //get source and destination
    //send it to inputManager
    public List<Tuple<int, int>> GetSourceAndDestination()
    {
        return new List<Tuple<int, int>>()
        {
            source,
            destination
        };
    }


    public void Update()
    {

    }
}