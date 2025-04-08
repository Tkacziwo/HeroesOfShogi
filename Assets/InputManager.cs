using System;
using System.Collections.Generic;
using UnityEngine;
using static System.Net.Mime.MediaTypeNames;

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
        var hoveredCell = MouseOverCell();
        if (hoveredCell != null)
        {
            gameGrid.ClearPossibleMoves(possibleMoves);
            hoveredCell.GetComponentInChildren<SpriteRenderer>().material.color = Color.green;

            if (Input.GetMouseButtonDown(0))
            {
                if (chosenPiece)
                {
                    HandleMovePiece(hoveredCell);
                }
                else if(hoveredCell.objectInThisGridSpace != null)
                {
                    var pieceGameObject = hoveredCell.objectInThisGridSpace;
                    var piece = pieceGameObject.GetComponent<Piece>();
                    Vector2Int piecePosition = piece.GetPosition();
                    possibleMoves = boardManager.CalculatePossibleMoves(piecePosition.x, piecePosition.y, piece.GetMoveset());
                    foreach (var r in possibleMoves)
                    {
                        var cell = gameGrid.gameGrid[r.Item1, r.Item2].GetComponent<GridCell>();
                        cell.SetIsPossibleMove();
                        cell.GetComponentInChildren<SpriteRenderer>().material.color = Color.black;
                    }
                    CellWhichHoldsPiece = hoveredCell;
                    chosenPiece = true;
                }
            }
        }
        else
        {
            gameGrid.ClearPossibleMoves(possibleMoves);
        }
    }

    private void HandleMovePiece(GridCell hoveredCell)
    {
        if (hoveredCell.GetIsPossibleMove())
        {
            Piece piece = CellWhichHoldsPiece.objectInThisGridSpace.GetComponent<Piece>();
            piece.MovePiece(hoveredCell.GetPosition());
            hoveredCell.SetAndMovePiece(CellWhichHoldsPiece.objectInThisGridSpace, hoveredCell.GetWorldPosition());

            CellWhichHoldsPiece.objectInThisGridSpace = null;
            RemovePossibleMoves();
            chosenPiece = false;
        }
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

    private void MovePiece(GameObject pieceGameObject)
    {
        Piece piece = pieceGameObject.GetComponent<Piece>();
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
