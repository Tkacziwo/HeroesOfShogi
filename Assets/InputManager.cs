using System;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    GridGame gameGrid;
    [SerializeField] private LayerMask whatIsAGridLayer;

    [SerializeField] private GameObject shogiPiece;

    private BoardManager boardManager;

    private KingManager kingManager;

    private List<Tuple<int, int>> possibleMoves;

    public List<Piece> bodyguards;

    public List<Piece> sacrifices;

    public List<Tuple<int, int>> endangeredMoves;

    public List<Tuple<int, int>> extendedDangerMoves;

    private bool chosenPiece;

    private bool kingInDanger;

    private Tuple<int, int> kingPos;

    private GridCell CellWhichHoldsPiece;

    private GridCell CellWhichHoldsAttacker;

    private Tuple<int, int> attackerPos;

    [SerializeField] private bool playerTurn;

    [SerializeField] private bool botEnabled;

    [SerializeField] private ShogiBot bot;

    [SerializeField] private Canvas canvas;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        gameGrid = FindFirstObjectByType<GridGame>();
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
        if (!canvas.isActiveAndEnabled)
        {
            if (!playerTurn && botEnabled)
            {
                Position aPosition = new(attackerPos);
                List<Position> extendedDangerMovesPositions = new();
                if (extendedDangerMoves != null)
                {
                    foreach (var e in extendedDangerMoves)
                    {
                        extendedDangerMovesPositions.Add(new(e));
                    }
                }
                bot.GetBoardState(gameGrid, kingInDanger, aPosition, extendedDangerMovesPositions);
                var botResult = bot.ApplyMoveToRealBoard();
                if (botResult.Item1.x > 9 || botResult.Item1.y > 9)
                {
                    CellWhichHoldsPiece = gameGrid.eCamp.campGrid[botResult.Item1.x - 200, botResult.Item1.y - 200].GetComponent<GridCell>();
                }
                else
                {
                    CellWhichHoldsPiece = gameGrid.GetGridCell(botResult.Item1.x, botResult.Item1.y);
                }
                var cell = gameGrid.GetGridCell(botResult.Item2.x, botResult.Item2.y);
                playerTurn = true;
                HandlePieceMove(cell);
            }
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
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                //undo move
                var undo = boardManager.UndoMove();
                var source = undo.src;
                var dest = undo.dst;
                CellWhichHoldsPiece = gameGrid.GetGridCell(dest.Item1, dest.Item2);
                var cell = gameGrid.GetGridCell(source.Item1, source.Item2);
                HandlePieceMove(cell, false);
            }
        }
    }

    private void HandleMovePiece(GridCell hoveredCell)
    {
        if (hoveredCell.GetIsPossibleMove())
        {
            HandlePieceMove(hoveredCell);
            if (playerTurn)
                playerTurn = false;
            else
                playerTurn = true;
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
        if ((playerTurn && !piece.GetIsBlack()) || (!playerTurn && piece.GetIsBlack()))
        {
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
                    foreach (var b in bodyguards)
                    {
                        if (hoveredCell.GetPositionTuple().Equals(b.GetPositionTuple()))
                        {
                            PossibleMovesCalculationHandler(piece, hoveredCell, true);
                            break;
                        }
                    }
                    //drop checking
                    if (piece.GetIsDrop())
                    {
                        PossibleMovesCalculationHandler(piece, hoveredCell);
                    }
                    //sacrifice checking
                    if (sacrifices != null)
                    {
                        foreach (var s in sacrifices)
                        {
                            if (hoveredCell.GetPositionTuple().Equals(s.GetPositionTuple()))
                            {
                                if (endangeredMoves != null)
                                {
                                    possibleMoves = kingManager.CalculateProtectionMoves(piece.GetPositionClass(), piece.GetMoveset(), piece.GetIsBlack(), endangeredMoves); ;
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
            }
            else
            {
                PossibleMovesCalculationHandler(piece, hoveredCell);
            }
        }
    }

    public void PossibleMovesCalculationHandler(Piece piece, GridCell hoveredCell, bool bodyguard = false)
    {
        if (bodyguard)
        {
            possibleMoves = new()
            {
                CellWhichHoldsAttacker.objectInThisGridSpace.GetComponent<Piece>().GetPositionTuple()
            };
        }
        else if (piece.isKing)
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
            possibleMoves = boardManager.CalculatePossibleMoves(piece.GetPositionClass(), piece.GetMoveset(), piece.GetIsBlack());
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

    public void HandlePieceMove(GridCell hoveredCell, bool registerMove = true)
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

        if (registerMove)
        {
            Tuple<Tuple<int, int>, Tuple<int, int>> sourceDestination
                = new(piece.GetPositionTuple(), hoveredCell.GetPositionTuple());
            boardManager.RegisterMove(sourceDestination, piece.GetIsDrop());
        }

        if (piece.GetIsDrop())
        {
            if (piece.GetIsBlack())
            {
                gameGrid.eCamp.capturedPieces.Remove(piece);
            }
            else
            {
                gameGrid.pCamp.capturedPieces.Remove(piece);
            }
            piece.ResetIsDrop();
        }

        //check for promotions
        else if (!piece.GetIsPromoted() && CheckForPromotion(hoveredCell, piece.GetIsBlack()))
        {
            boardManager.ApplyPromotion(piece);
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

        HandleKingEndangerement(piece);
    }

    public void HandleKingEndangerement(Piece piece)
    {
        Piece king = piece.GetIsBlack() ? gameGrid.GetPlayerKing() : gameGrid.GetBotKing();
        var piecesList = piece.GetIsBlack() ? gameGrid.GetPlayerPieces() : gameGrid.GetBotPieces();

        var closeRes = kingManager.CloseScanForKing(king, piece.GetPositionTuple());
        var farRes = kingManager.FarScanForKing(king.GetPositionTuple(), king.GetIsBlack(), ref attackerPos);
        if (closeRes || farRes)
        {
            if (closeRes)
            {
                attackerPos = piece.GetPositionTuple();
            }
            CellWhichHoldsAttacker = gameGrid.GetGridCell(attackerPos.Item1, attackerPos.Item2);
            var attacker = gameGrid.GetPieceInGrid(attackerPos.Item1, attackerPos.Item2).GetComponent<Piece>();
            kingInDanger = true;
            kingPos = king.GetPositionTuple();
            king.GetComponentInChildren<MeshRenderer>().material.color = Color.red;
            //extendedDangerMoves = kingManager.CalculateEndangeredMoves(attacker);
            endangeredMoves = kingManager.CalculateEndangeredMoves(attacker, king.GetPositionTuple());
            bodyguards = kingManager.FindGuards(attackerPos, piecesList);
            sacrifices = kingManager.FindSacrifices(endangeredMoves, piecesList);
        }
    }

    public void HandlePieceKill(GridCell hoveredCell)
    {
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
        if (possibleMoves != null)
        {
            foreach (var r in possibleMoves)
            {
                var cell = gameGrid.gameGrid[r.Item1, r.Item2].GetComponent<GridCell>();
                cell.ResetIsPossibleMove();
                cell.GetComponentInChildren<SpriteRenderer>().material.color = Color.black;
            }
            possibleMoves = null;
        }
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
