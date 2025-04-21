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

    private KingManager kingManager;

    private List<Tuple<int, int>> possibleMoves;

    public List<Tuple<int, int>> bodyguardsPositions;

    public List<Tuple<int, int>> sacrificesPositions;

    public List<Tuple<int, int>> endangeredMoves;

    public List<Tuple<int, int>> extendedDangerMoves;

    private bool chosenPiece;

    private bool kingInDanger;

    private Tuple<int, int> kingPos;

    private GridCell CellWhichHoldsPiece;

    private GridCell CellWhichHoldsAttacker;

    private Tuple<int, int> attackerPos;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameGrid = FindFirstObjectByType<GridGame>();
        fileManager = FindFirstObjectByType<FileManager>();
        boardManager = FindFirstObjectByType<BoardManager>();
        kingManager = FindFirstObjectByType<KingManager>();
        chosenPiece = false;
        kingInDanger = false;
        CellWhichHoldsPiece = null;
        CellWhichHoldsAttacker = null;
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
        else if (CellWhichHoldsPiece.GetPosition() == hoveredCell.GetPosition())
        {
            HandleUnclickPiece(hoveredCell);
        }
        else if ((hoveredCell.objectInThisGridSpace != null) && (CellWhichHoldsPiece.objectInThisGridSpace.GetComponent<Piece>().GetIsBlack()
                == hoveredCell.objectInThisGridSpace.GetComponent<Piece>().GetIsBlack()))
        {
            HandlePieceClicked(hoveredCell);
        }
    }

    public void HandlePieceClicked(GridCell hoveredCell)
    {
        //clicked piece
        var piece = hoveredCell.objectInThisGridSpace.GetComponent<Piece>();
        if (chosenPiece)
        {
            RemovePossibleMoves();
        }
        //handle king safety
        if (kingInDanger)
        {
            if (piece.isKing)
            {
                possibleMoves = kingManager.CloseScan(piece.GetPositionTuple());
                var attacker = CellWhichHoldsAttacker.objectInThisGridSpace.GetComponent<Piece>();
                var attackerProtected = kingManager
                    .FarScan(attacker.GetPositionTuple(), attacker.GetIsBlack());
                var additionalDangerMoves = kingManager.KingDangerMovesScan(possibleMoves, piece.GetIsBlack());
                List<Tuple<int, int>> attackerPos = new()
                {
                    attacker.GetPositionTuple()
                };

                if (additionalDangerMoves != null)
                {
                    possibleMoves = boardManager.CalculateOverlappingMoves(possibleMoves, additionalDangerMoves, false);
                }

                if (attackerProtected)
                {
                    possibleMoves = boardManager.CalculateOverlappingMoves(possibleMoves, attackerPos, false);
                }

                if (extendedDangerMoves != null)
                {
                    possibleMoves = boardManager.CalculateOverlappingMoves(possibleMoves, extendedDangerMoves, false);
                }
                foreach (var r in possibleMoves)
                {
                    var cell = gameGrid.gameGrid[r.Item1, r.Item2].GetComponent<GridCell>();
                    cell.SetIsPossibleMove();
                    cell.GetComponentInChildren<SpriteRenderer>().material.color = Color.black;
                }
                CellWhichHoldsPiece = hoveredCell;
                chosenPiece = true;
            }
            else
            {

                //bodyguard checking
                foreach (var b in bodyguardsPositions)
                {
                    if (hoveredCell.GetPositionTuple().Equals(b))
                    {
                        PossibleMovesCalculationHandler(piece, hoveredCell);
                        break;
                    }
                }
                //drop checking
                if (piece.GetIsDrop())
                {
                    PossibleMovesCalculationHandler(piece, hoveredCell);
                }
                //sacrifice checking
                foreach (var s in sacrificesPositions)
                {
                    if (hoveredCell.GetPositionTuple().Equals(s))
                    {
                        if (endangeredMoves != null)
                        {
                            possibleMoves = kingManager.CalculateProtectionMoves(piece.GetPosition(), piece.GetMoveset(), piece.GetIsBlack(), endangeredMoves); ;
                        }

                        foreach (var r in possibleMoves)
                        {
                            var cell = gameGrid.gameGrid[r.Item1, r.Item2].GetComponent<GridCell>();
                            cell.SetIsPossibleMove();
                            cell.GetComponentInChildren<SpriteRenderer>().material.color = Color.black;
                        }
                        CellWhichHoldsPiece = hoveredCell;
                        chosenPiece = true;
                        break;
                    }
                }
            }
        }
        else
        {
            PossibleMovesCalculationHandler(piece, hoveredCell);
        }
    }

    public void PossibleMovesCalculationHandler(Piece piece, GridCell hoveredCell)
    {
        if (piece.isKing)
        {
            possibleMoves = kingManager.CloseScan(piece.GetPositionTuple());
            //also check FarScan
            var copy = new List<Tuple<int, int>>();
            copy.AddRange(possibleMoves);
            foreach (var p in possibleMoves)
            {
                var res = kingManager.FarScanForKing(p, piece.GetIsBlack(), ref attackerPos);
                if (res)
                {
                    copy.Remove(p);
                }
            }
            possibleMoves = copy;
        }
        else if (piece.GetIsDrop())
        {
            possibleMoves = boardManager.CalculatePossibleDrops();
        }
        else
        {
            possibleMoves = boardManager.CalculatePossibleMoves(piece.GetPosition(), piece.GetMoveset(), piece.GetIsBlack());
        }
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
        if (kingInDanger)
        {
            var scanPieceGM = gameGrid.GetPieceInGrid(kingPos.Item1, kingPos.Item2);
            if (scanPieceGM.GetComponent<Piece>().GetIsBlack())
            {
                scanPieceGM.GetComponentInChildren<MeshRenderer>().material.color = Color.black;
            }
            else
            {
                scanPieceGM.GetComponentInChildren<MeshRenderer>().material.color = Color.white;
            }
            kingInDanger = false;
        }
        //check if drop
        if (piece.GetIsDrop())
        {
            if (piece.GetIsBlack())
            {
                piece.ResetIsBlack();
            }
            else
            {
                piece.SetIsBlack();
            }
            piece.ResetIsDrop();
        }
        //check for promotions
        else if (!piece.GetIsPromoted() && CheckForPromotion(hoveredCell, piece.GetIsBlack()))
        {
            if (!piece.isKing)
            {

                if (!piece.GetIsSpecial())
                {
                    piece.Promote(fileManager.GetMovesetByPieceName("GoldGeneral"));
                }
                else
                {
                    piece.BackupOriginalMoveset();
                    int[] moveset = piece.GetMoveset();
                    for (int i = 0; i < 9; i++)
                    {
                        if (moveset[i] != 2 && i != 4)
                        {
                            moveset[i]++;
                        }
                    }
                    piece.Promote(moveset);
                }
            }
        }

        //handle piece kill
        if (hoveredCell.objectInThisGridSpace != null &&
            hoveredCell.objectInThisGridSpace.GetComponent<Piece>().GetIsBlack() != piece.GetIsBlack())
        {
            HandlePieceKill(hoveredCell);
        }


        piece.MovePiece(hoveredCell.GetPosition());
        hoveredCell.SetAndMovePiece(CellWhichHoldsPiece.objectInThisGridSpace, hoveredCell.GetWorldPosition());
        CellWhichHoldsPiece.objectInThisGridSpace = null;
        RemovePossibleMoves();
        chosenPiece = false;


        var isBlack = piece.GetIsBlack();

        for (int y = 0; y < 9; y++)
        {
            for (int x = 0; x < 9; x++)
            {
                if (!boardManager.IsCellFree(x, y)
                    && boardManager.IsEnemy(x, y, isBlack)
                    && gameGrid.GetPieceInGrid(x, y).GetComponent<Piece>().isKing)
                {
                    var foundKing = gameGrid.GetPieceInGrid(x, y).GetComponent<Piece>();

                    var res = kingManager.FarScanForKing(foundKing.GetPositionTuple(), foundKing.GetIsBlack(),ref attackerPos);
                    if (res)
                    {
                        CellWhichHoldsAttacker = gameGrid.GetGridCell(attackerPos.Item1, attackerPos.Item2);
                        var attacker = gameGrid.GetPieceInGrid(attackerPos.Item1, attackerPos.Item2).GetComponent<Piece>();
                        kingInDanger = true;
                        kingPos = foundKing.GetPositionTuple();
                        foundKing.GetComponentInChildren<MeshRenderer>().material.color = Color.red;
                        bodyguardsPositions = kingManager.FindBodyguards(foundKing.GetIsBlack(), attacker.GetPositionTuple());
                        sacrificesPositions = kingManager.FindSacrifices(foundKing, attacker);
                        endangeredMoves = kingManager.CalculateEndangeredMoves(attacker, foundKing.GetPositionTuple());
                        extendedDangerMoves = kingManager.CalculateEndangeredMoves(attacker);
                    }
                }
            }
        }

        //handle king endangerment
        var moveScan = boardManager.CalculatePossibleMoves(piece.GetPosition(), piece.GetMoveset(), piece.GetIsBlack());
        foreach (var move in moveScan)
        {
            //var gridCell = gameGrid.GetGridCell(move.Item1, move.Item2);
            var scanPieceGM = gameGrid.GetPieceInGrid(move.Item1, move.Item2);
            if (scanPieceGM != null)
            {
                var scanPiece = scanPieceGM.GetComponent<Piece>();
                if (scanPiece != null && scanPiece.isKing && scanPiece.GetIsBlack() != piece.GetIsBlack())
                {
                    //[todo] -> fixing
                    //found king
                    kingInDanger = true;
                    CellWhichHoldsAttacker = hoveredCell;
                    //gridCell.GetComponentInChildren<MeshRenderer>().material.color = Color.magenta
                    kingPos = new(move.Item1, move.Item2);
                    scanPieceGM.GetComponentInChildren<MeshRenderer>().material.color = Color.red;
                    bodyguardsPositions = kingManager.FindBodyguards(scanPiece.GetIsBlack(), piece.GetPositionTuple());
                    sacrificesPositions = kingManager.FindSacrifices(scanPiece, piece);
                    endangeredMoves = kingManager.CalculateEndangeredMoves(piece, scanPiece.GetPositionTuple());
                    extendedDangerMoves = kingManager.CalculateEndangeredMoves(piece);
                }
            }
        }
    }

    public void HandlePieceKill(GridCell hoveredCell)
    {
        //var piece = hoveredCell.objectInThisGridSpace.GetComponent<Piece>();
        gameGrid.AddToCamp(hoveredCell.objectInThisGridSpace);
        hoveredCell.objectInThisGridSpace = null;
    }

    public bool CheckForPromotion(GridCell hoveredCell, bool isBlack)
    {
        if (!isBlack && hoveredCell.GetPosition().y > 5)
        {
            return true;
        }
        else if (isBlack && hoveredCell.GetPosition().y < 3)
        {
            return true;
        }
        else
        {
            return false;
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
