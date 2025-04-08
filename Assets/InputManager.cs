using System;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    GridGame gameGrid;
    [SerializeField] private LayerMask whatIsAGridLayer;

    [SerializeField] private GameObject shogiPiece;

    private FileManager fileManager;

    private BoardManager boardManager;

    private IList<Tuple<int, int>> possibleMoves;

    private bool chosenPiece;

    private GridCell CellWhichHoldsPiece;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameGrid = FindFirstObjectByType<GridGame>();
        fileManager = FindFirstObjectByType<FileManager>();
        boardManager = FindFirstObjectByType<BoardManager>();
        chosenPiece = false;
        CellWhichHoldsPiece = null;
    }

    // Update is called once per frame
    void Update()
    {
        gameGrid.ClearPossibleMoves(possibleMoves);
        var hoveredCell = MouseOverCell();
        if (hoveredCell != null)
        {
            hoveredCell.GetComponentInChildren<SpriteRenderer>().material.color = Color.green;

            if (Input.GetMouseButtonDown(0))
            {
                if (chosenPiece)
                {
                    HandleMovePiece(hoveredCell);
                }
                else if (hoveredCell.objectInThisGridSpace != null)
                {
                    HandlePieceClicked(hoveredCell);
                }
            }
        }
    }

    private void HandleMovePiece(GridCell hoveredCell)
    {
        if (hoveredCell.GetIsPossibleMove())
        {
            HandlePieceMove(hoveredCell);
        }
        else if(CellWhichHoldsPiece.GetPosition() == hoveredCell.GetPosition())
        {
            HandleUnclickPiece(hoveredCell);
        }
        else if (CellWhichHoldsPiece.objectInThisGridSpace.GetComponent<Piece>().GetIsBlack()
                == hoveredCell.objectInThisGridSpace.GetComponent<Piece>().GetIsBlack())
        {
            HandlePieceClicked(hoveredCell);
        }
    }

    public void HandlePieceClicked(GridCell hoveredCell)
    {
        if (chosenPiece)
        {
            RemovePossibleMoves();
        }

        var piece = hoveredCell.objectInThisGridSpace.GetComponent<Piece>();
        possibleMoves = boardManager.CalculatePossibleMoves(piece.GetPosition(), piece.GetMoveset(), piece.GetIsBlack());
        foreach (var r in possibleMoves)
        {
            var cell = gameGrid.gameGrid[r.Item1, r.Item2].GetComponent<GridCell>();
            cell.SetIsPossibleMove();
            cell.GetComponentInChildren<SpriteRenderer>().material.color = Color.black;
        }
        CellWhichHoldsPiece = hoveredCell;
        chosenPiece = true;
    }

    public void HandleUnclickPiece(GridCell hoveredCell)
    {
        hoveredCell = CellWhichHoldsPiece;
        CellWhichHoldsPiece = null;
        RemovePossibleMoves();
        chosenPiece = false;
    }

    public void HandlePieceMove(GridCell hoveredCell)
    {
        Piece piece = CellWhichHoldsPiece.objectInThisGridSpace.GetComponent<Piece>();
        piece.MovePiece(hoveredCell.GetPosition());
        hoveredCell.SetAndMovePiece(CellWhichHoldsPiece.objectInThisGridSpace, hoveredCell.GetWorldPosition());

        CellWhichHoldsPiece.objectInThisGridSpace = null;
        RemovePossibleMoves();
        chosenPiece = false;
    }

    private void RemovePossibleMoves()
    {
        foreach (var r in possibleMoves)
        {
            var cell = gameGrid.gameGrid[r.Item1, r.Item2].GetComponent<GridCell>();
            cell.ResetIsPossibleMove();
            cell.GetComponentInChildren<SpriteRenderer>().material.color = Color.black;
        }
        possibleMoves = null;
    }

    private GridCell MouseOverCell()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        var hit = Physics.Raycast(ray, out RaycastHit info);
        if (hit)
        {
            return info.transform.GetComponent<GridCell>();
        }
        else
        {
            return null;
        }
    }
}
