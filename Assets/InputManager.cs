using System;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputManager : MonoBehaviour
{
    GridGame gameGrid;
    [SerializeField] private LayerMask whatIsAGridLayer;

    [SerializeField] private GameObject shogiPiece;

    private BoardManager boardManager;

    private KingManager kingManager;

    private List<Position> possibleMoves;

    private List<Position> cantChangePossibleMoves;

    public List<Piece> bodyguards;

    public List<Piece> sacrifices;

    public List<Position> endangeredMoves;

    private bool chosenPiece;

    private bool kingInDanger;

    private Position kingPos;

    private GridCell CellWhichHoldsPiece;

    private GridCell CellWhichHoldsAttacker;

    private Position attackerPos;

    [SerializeField] private bool playerTurn;

    [SerializeField] private bool botEnabled;

    [SerializeField] private ShogiBot bot;

    [SerializeField] private Canvas canvas;

    [SerializeField] private AbilitiesManager abilitiesManager;

    [SerializeField] private Image abilityImage;

    [SerializeField] private Material grayMaterial;

    private bool specialAbilityInUse;

    private bool cantChangePiece;

    private List<Piece> promotedPieces = new();

    private List<Piece> nonPromotedPieces = new();

    private Position srcKingAbilityPiecePosition;

    private Position dstKingAbilityPiecePosition;

    private bool duringKingAbility;

    private bool duringBotMove;

    private bool botFinishedCalculating;

    private Tuple<Position, Position> botResult;

    [SerializeField] private TextMeshProUGUI duringBotText;

    private bool paused = false;

    [SerializeField] private GameObject dieAnimation;

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
        cantChangePiece = false;
        duringBotMove = false;
        botFinishedCalculating = false;
        botEnabled = StaticData.botEnabled;
        bot.InitializeBot(StaticData.botDifficulty);
        attackerPos = new();
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        paused = true;
    }

    public void ResumeGame()
    {
        paused = false;
        Time.timeScale = 1f;
    }

    public void StartBotMinimax()
    {
        Thread botThread = new(() =>
        {
            botResult = bot.ApplyMoveToRealBoard();
            botFinishedCalculating = true;
        });

        botThread.Start();
    }

    public void PrepareBotForMinimax()
    {
        duringBotText.gameObject.SetActive(true);
        duringBotMove = true;
        botFinishedCalculating = false;
        Position aPosition = new(attackerPos);

        bot.GetBoardState(gameGrid, kingInDanger, aPosition);

        StartBotMinimax();
    }

    public void ApplyBotMinimaxResult()
    {
        if (botResult.Item1.x > 9 || botResult.Item1.y > 9)
        {
            CellWhichHoldsPiece = gameGrid.eCamp.campGrid[botResult.Item1.x - 200, botResult.Item1.y - 200].GetComponent<GridCell>();
        }
        else
        {
            CellWhichHoldsPiece = gameGrid.GetGridCell(botResult.Item1.x, botResult.Item1.y);
        }
        var cell = gameGrid.GetGridCell(botResult.Item2.x, botResult.Item2.y);
        ExecutePieceMove(cell);
        playerTurn = true;
        botFinishedCalculating = false;
        duringBotMove = false;
        duringBotText.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (!paused)
        {
            gameGrid.ClearPossibleMoves(possibleMoves);
            var hoveredCell = MouseOverCell();

            if (botFinishedCalculating)
            {
                if (botResult == null)
                {
                    //Handle finish game
                }
                else
                {
                    ApplyBotMinimaxResult();
                }
            }
            else if (!playerTurn && botEnabled && !duringBotMove)
            {
                PrepareBotForMinimax();
            }
            else if (hoveredCell != null)
            {

                if (possibleMoves != null)
                {
                    if (possibleMoves.Contains(hoveredCell.GetPosition()))
                    {
                        hoveredCell.GetComponentInChildren<SpriteRenderer>().material.color = Color.green;
                    }
                    else
                    {
                        hoveredCell.GetComponentInChildren<SpriteRenderer>().material.color = new(1.0f, 86 / 255, 83 / 255);
                    }
                }
                else
                {
                    hoveredCell.GetComponentInChildren<SpriteRenderer>().material.color = Color.magenta;
                }

                if (Input.GetMouseButtonDown(0) && !duringBotMove)
                {
                    if (duringKingAbility)
                    {
                        HandleKingAbility(hoveredCell);
                    }
                    else if (cantChangePiece)
                    {
                        HandleExtraMove(hoveredCell);
                    }
                    else
                    {
                        HandleBoardClick(hoveredCell);
                    }
                }
                else if (Input.GetKeyDown(KeyCode.Backspace))
                {
                    //undo move
                    var undo = boardManager.UndoMove();
                    var source = undo.src;
                    var dest = undo.dst;
                    CellWhichHoldsPiece = gameGrid.GetGridCell(dest.Item1, dest.Item2);
                    var cell = gameGrid.GetGridCell(source.Item1, source.Item2);
                    ExecutePieceMove(cell);
                }
            }
        }
    }

    private void HandleKingAbility(GridCell hoveredCell)
    {
        if (srcKingAbilityPiecePosition == null)
        {
            foreach (var p in promotedPieces)
            {
                if (hoveredCell.GetPosition().Equals(p.GetPosition()))
                {
                    srcKingAbilityPiecePosition = p.GetPosition();
                    break;
                }
            }
        }
        else if (dstKingAbilityPiecePosition == null)
        {
            foreach (var p in nonPromotedPieces)
            {
                if (hoveredCell.GetPosition().Equals(p.GetPosition()) && srcKingAbilityPiecePosition != null)
                {
                    dstKingAbilityPiecePosition = p.GetPosition();
                    abilitiesManager.KingPromote(srcKingAbilityPiecePosition, dstKingAbilityPiecePosition);
                    srcKingAbilityPiecePosition = dstKingAbilityPiecePosition = null;
                    duringKingAbility = false;
                    break;
                }
            }
        }
    }

    private void HandleExtraMove(GridCell hoveredCell)
    {
        foreach (var p in cantChangePossibleMoves)
        {
            hoveredCell.SetIsPossibleMove();
            if (hoveredCell.GetPosition().Equals(p))
            {
                HandleBoardClick(hoveredCell);
                cantChangePiece = false;
                break;
            }
        }
    }

    public void HandleSpecialAbilityUsage()
    {
        if (chosenPiece)
        {
            var piece = CellWhichHoldsPiece.objectInThisGridSpace.GetComponent<Piece>();
            abilityImage.material = grayMaterial;
            if (piece.abilityCooldown == 0)
            {
                switch (piece.GetName())
                {
                    case "King":
                        {
                            var pieceList = piece.GetIsBlack() ? gameGrid.GetBotPieces() : gameGrid.GetPlayerPieces();
                            foreach (var p in pieceList)
                            {
                                if (p.GetIsPromoted())
                                {
                                    promotedPieces.Add(p);
                                    p.GetComponentInChildren<MeshRenderer>().material.color = Color.yellow;
                                }
                                else
                                {
                                    p.GetComponentInChildren<MeshRenderer>().material.color = Color.blue;
                                    nonPromotedPieces.Add(p);
                                }
                            }
                            duringKingAbility = true;
                            piece.abilityCooldown = -1;
                            break;
                        }
                    case "GoldGeneral":
                        {
                            bool turn = playerTurn;
                            var positions = abilitiesManager.Onward(piece.GetPosition(), piece.GetIsBlack());
                            foreach (var p in positions)
                            {
                                CellWhichHoldsPiece = gameGrid.GetGridCell(p.Item1.x, p.Item1.y);
                                ExecutePieceMove(gameGrid.GetGridCell(p.Item2.x, p.Item2.y));
                            }
                            int destY;
                            var piecePos = piece.GetPosition();
                            if (piece.GetIsBlack())
                            {
                                destY = piecePos.y - 1;
                            }
                            else
                            {
                                destY = piecePos.y + 1;
                            }
                            if (boardManager.IsInBoard(destY, piecePos.x) && boardManager.IsCellFree(piecePos.x, destY))
                            {
                                CellWhichHoldsPiece = gameGrid.GetGridCell(piece.GetPosition());
                                ExecutePieceMove(gameGrid.GetGridCell(piecePos.x, destY));
                            }

                            specialAbilityInUse = false;
                            piece.abilityCooldown = 2;
                            playerTurn = !turn;
                            break;
                        }
                    case "Bishop":
                        {
                            bool turn = playerTurn;
                            var positions = abilitiesManager.Regroup(piece.GetPosition(), piece.GetIsBlack());
                            foreach (var p in positions)
                            {

                                CellWhichHoldsPiece = gameGrid.GetGridCell(p.Item1.x, p.Item1.y);
                                ExecutePieceMove(gameGrid.GetGridCell(p.Item2.x, p.Item2.y));
                            }
                            int destY;
                            var piecePos = piece.GetPosition();
                            if (piece.GetIsBlack())
                            {
                                destY = piecePos.y + 1;
                            }
                            else
                            {
                                destY = piecePos.y - 1;
                            }
                            if (boardManager.IsInBoard(destY, piecePos.x) && boardManager.IsCellFree(piecePos.x, destY))
                            {
                                CellWhichHoldsPiece = gameGrid.GetGridCell(piece.GetPosition());
                                ExecutePieceMove(gameGrid.GetGridCell(piecePos.x, destY));
                            }

                            specialAbilityInUse = false;
                            piece.abilityCooldown = 3;
                            playerTurn = !turn;
                            break;
                        }
                    case "SilverGeneral":
                    case "Rook":
                        specialAbilityInUse = true;
                        break;
                    default:
                        break;
                }
            }
        }
    }

    private void HandleBoardClick(GridCell hoveredCell)
    {
        if (hoveredCell.GetIsPossibleMove())
        {
            ExecutePieceMove(hoveredCell);
            playerTurn = !playerTurn;
        }
        else if (CellWhichHoldsPiece != null && CellWhichHoldsPiece.GetPosition().Equals(hoveredCell.GetPosition()))
        {
            HandleUnclickPiece();
        }
        else if ((hoveredCell.objectInThisGridSpace != null))
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
            HandleAbilityImageColorChange(piece);
            if (chosenPiece)
            {
                RemovePossibleMoves();
            }
            //handle king safety
            if (kingInDanger)
            {
                if (piece.isKing)
                {
                    possibleMoves = kingManager.CloseScan(piece.GetPosition());
                    var attacker = CellWhichHoldsAttacker.objectInThisGridSpace.GetComponent<Piece>();
                    var attackerProtected = kingManager.FarScan(attacker.GetPosition(), attacker.GetIsBlack());

                    var attackerPossibleMovesUnrestricted = boardManager.CalculatePossibleMoves(attacker, true);
                    if (attackerPossibleMovesUnrestricted != null)
                    {
                        possibleMoves = boardManager.CalculateOverlappingMoves(possibleMoves, attackerPossibleMovesUnrestricted, false);
                    }

                    var additionalDangerMoves = kingManager.KingDangerMovesScan(possibleMoves, piece.GetIsBlack());
                    List<Position> attackerPos = new()
                    {
                    attacker.GetPosition()
                    };

                    if (additionalDangerMoves != null)
                    {
                        possibleMoves = boardManager.CalculateOverlappingMoves(possibleMoves, additionalDangerMoves, false);
                    }

                    if (attackerProtected)
                    {
                        possibleMoves = boardManager.CalculateOverlappingMoves(possibleMoves, attackerPos, false);
                    }

                    PossibleMovesDisplayLoop();

                    CellWhichHoldsPiece = hoveredCell;
                    chosenPiece = true;
                }
                else
                {
                    bool bodyguardClicked = false;
                    //bodyguard checking
                    foreach (var b in bodyguards)
                    {
                        if (hoveredCell.GetPosition().Equals(b.GetPosition()))
                        {
                            PossibleMovesCalculationHandler(piece, hoveredCell, true);
                            bodyguardClicked = true;
                            break;
                        }
                    }
                    //drop checking
                    if (!bodyguardClicked)
                    {
                        if (piece.GetIsDrop())
                        {
                            possibleMoves = boardManager.CalculatePossibleDrops(piece);
                            possibleMoves = boardManager.CalculateOverlappingMoves(possibleMoves, endangeredMoves, true);
                        }
                        //sacrifice checking
                        else if (sacrifices != null)
                        {
                            foreach (var s in sacrifices)
                            {
                                if (hoveredCell.GetPosition().Equals(s.GetPosition()))
                                {
                                    if (endangeredMoves != null)
                                    {
                                        possibleMoves = kingManager.CalculateProtectionMoves(piece, endangeredMoves); ;
                                    }

                                    PossibleMovesDisplayLoop();

                                    CellWhichHoldsPiece = hoveredCell;
                                    chosenPiece = true;
                                    break;
                                }
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
                CellWhichHoldsAttacker.objectInThisGridSpace.GetComponent<Piece>().GetPosition()
            };
        }
        else if (piece.isKing)
        {
            possibleMoves = kingManager.CloseScan(piece.GetPosition());
            //also check FarScan
            var copy = new List<Position>();
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
            possibleMoves = boardManager.CalculatePossibleDrops(piece);
        }
        else
        {
            possibleMoves = boardManager.CalculatePossibleMoves(piece);
            boardManager.CheckIfMovesAreLegal(ref possibleMoves, piece);
        }


        PossibleMovesDisplayLoop();

        CellWhichHoldsPiece = hoveredCell;
        chosenPiece = true;
    }

    public void PossibleMovesDisplayLoop()
    {
        foreach (var p in possibleMoves)
        {
            var cell = gameGrid.GetGridCell(p);
            cell.SetIsPossibleMove();
            cell.GetComponentInChildren<SpriteRenderer>().material.color = Color.green;
        }
    }

    public void HandleUnclickPiece()
    {
        abilityImage.sprite = null;
        CellWhichHoldsPiece = null;
        RemovePossibleMoves();
        chosenPiece = false;
        specialAbilityInUse = false;
    }

    public void ExecutePieceMove(GridCell hoveredCell, bool registerMove = true)
    {
        Piece piece = CellWhichHoldsPiece.objectInThisGridSpace.GetComponent<Piece>();

        if (kingInDanger)
        {
            gameGrid.GetPieceInGrid(kingPos).GetComponentInChildren<MeshRenderer>().material.color =
                piece.GetIsBlack() ? Color.black : Color.white;
            kingInDanger = false;

            Piece attacker = gameGrid.GetPieceInGrid(attackerPos).GetComponent<Piece>();

            if (piece.GetIsBlack())
            {
                attacker.ResetIsBlack();
            }
            else
            {
                attacker.SetIsBlack();
            }
        }

        //if (registerMove)
        //{
        //    Tuple<Tuple<int, int>, Tuple<int, int>> sourceDestination
        //        = new(piece.GetPositionTuple(), hoveredCell.GetPositionTuple());
        //    boardManager.RegisterMove(sourceDestination, piece.GetIsDrop());
        //}

        //check for promotions
        if (!piece.GetIsDrop() && !piece.GetIsPromoted() && CheckForPromotion(hoveredCell, piece.GetIsBlack()))
        {
            boardManager.ApplyPromotion(piece);
        }

        var handlePieceKillResult = HandlePieceKill(hoveredCell, piece);

        piece.MovePiece(hoveredCell.GetPosition());
        if (piece.abilityCooldown > 0)
        {
            piece.abilityCooldown--;
        }

        hoveredCell.SetAndMovePiece(CellWhichHoldsPiece.objectInThisGridSpace, hoveredCell.GetWorldPosition());

        HandleDropCheck(piece);

        RemovePossibleMoves();
        chosenPiece = false;

        if (specialAbilityInUse)
        {
            switch (piece.GetName())
            {
                case "SilverGeneral":
                    var turn = playerTurn;
                    playerTurn = !turn;
                    possibleMoves = abilitiesManager.Rush(piece.GetPosition());
                    CellWhichHoldsPiece = gameGrid.GetGridCell(piece.GetPosition());
                    cantChangePiece = true;
                    cantChangePossibleMoves = new(possibleMoves);
                    specialAbilityInUse = false;
                    piece.abilityCooldown = 4;
                    break;
                case "Rook":
                    {
                        if (handlePieceKillResult.Item1)
                        {
                            Position p = new(hoveredCell.GetPosition());
                            var infernoResult = abilitiesManager.Inferno(p, handlePieceKillResult.Item2);
                            if (infernoResult != null)
                            {
                                KillPiece(gameGrid.GetGridCell(infernoResult.x, infernoResult.y));
                            }
                            specialAbilityInUse = false;
                            piece.abilityCooldown = -1;
                        }
                        break;
                    }
                default:
                    break;
            }
        }

        if (!piece.isKing)
        {
            HandleKingEndangerement(piece);
        }
    }

    public void HandleDropCheck(Piece piece)
    {
        if (piece.GetIsDrop())
        {
            if (piece.GetIsBlack())
            {
                gameGrid.eCamp.capturedPieceObjects.Remove(CellWhichHoldsPiece.objectInThisGridSpace);
                gameGrid.eCamp.Reshuffle();
            }
            else
            {
                gameGrid.pCamp.capturedPieceObjects.Remove(CellWhichHoldsPiece.objectInThisGridSpace);
                gameGrid.pCamp.Reshuffle();
            }
            piece.ResetIsDrop();
        }
        else
        {
            CellWhichHoldsPiece.objectInThisGridSpace = null;
        }
    }

    public void HandleKingEndangerement(Piece piece)
    {
        Piece king = piece.GetIsBlack() ? gameGrid.GetPlayerKing() : gameGrid.GetBotKing();
        var piecesList = piece.GetIsBlack() ? gameGrid.GetPlayerPieces() : gameGrid.GetBotPieces();

        var attackerRes = kingManager.AttackerScanForKing(king, piece);
        var closeRes = kingManager.CloseScanForKing(king, piece.GetPosition());
        var farRes = kingManager.FarScanForKing(king.GetPosition(), king.GetIsBlack(), ref attackerPos);
        if (closeRes || farRes || attackerRes)
        {
            if (closeRes || attackerRes)
            {
                attackerPos = piece.GetPosition();
            }
            CellWhichHoldsAttacker = gameGrid.GetGridCell(attackerPos);
            var attacker = gameGrid.GetPieceInGrid(attackerPos).GetComponent<Piece>();
            kingInDanger = true;
            kingPos = king.GetPosition();
            king.GetComponentInChildren<MeshRenderer>().material.color = Color.red;
            endangeredMoves = kingManager.CalculateEndangeredMoves(attacker, king.GetPosition());
            bodyguards = kingManager.FindGuards(attackerPos, piecesList);
            sacrifices = kingManager.FindSacrifices(endangeredMoves, piecesList);
        }
    }

    public void HandleAbilityImageColorChange(Piece piece)
    {
        abilityImage.sprite = Resources.Load<Sprite>("Sprites/" + piece.GetName() + "Ability");
        if (piece.abilityCooldown > 0 || piece.abilityCooldown < 0)
        {
            abilityImage.material = grayMaterial;
        }
        else
        {
            abilityImage.material = null;
        }
    }

    public Tuple<bool, bool> HandlePieceKill(GridCell hoveredCell, Piece piece)
    {
        bool killedPiece = false;
        bool killedPieceColor;

        if (hoveredCell.objectInThisGridSpace != null &&
            hoveredCell.objectInThisGridSpace.GetComponent<Piece>().GetIsBlack() != piece.GetIsBlack())
        {
            killedPieceColor = hoveredCell.objectInThisGridSpace.GetComponent<Piece>().GetIsBlack();
            KillPiece(hoveredCell);
            killedPiece = true;
        }
        else
        {
            killedPieceColor = false;
        }

        return new(killedPiece, killedPieceColor);
    }

    public void KillPiece(GridCell hoveredCell)
    {
        gameGrid.AddToCamp(hoveredCell.objectInThisGridSpace);
        hoveredCell.objectInThisGridSpace = null;
        Instantiate(dieAnimation, hoveredCell.GetWorldPosition(), Quaternion.identity);
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
                var cell = gameGrid.GetGridCell(r);
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
