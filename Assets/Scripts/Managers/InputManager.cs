using NUnit.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class InputManager : MonoBehaviour
{
    [SerializeField] private Grid grid;

    [SerializeField] private BoardManager boardManager;

    [SerializeField] private KingManager kingManager;

    [SerializeField] private Camera mainCamera;

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

    [SerializeField] private Canvas gameOver;

    [SerializeField] private TextMeshProUGUI gameOverText;

    [SerializeField] private AbilitiesManager abilitiesManager;

    [SerializeField] private Image abilityImage;

    [SerializeField] private Material grayMaterial;

    [SerializeField] private Canvas tutorialCanvas;

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

    [SerializeField] private TutorialGrid tutorialGrid;

    private Terrain currentTerrain;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        chosenPiece = false;
        kingInDanger = false;
        CellWhichHoldsPiece = null;
        CellWhichHoldsAttacker = null;
        cantChangePiece = false;
        duringBotMove = false;
        botFinishedCalculating = false;
        botEnabled = StaticData.botEnabled;
        bot.InitializeBot(StaticData.botDifficulty);
        if (!StaticData.tutorial)
        {
            tutorialGrid.gameObject.SetActive(false);
            tutorialCanvas.gameObject.SetActive(false);
            if (StaticData.map == "GrasslandsImage")
            {
                currentTerrain = Instantiate(Resources.Load<Terrain>("Terrains/Grasslands"));
                currentTerrain.transform.position = new Vector3(-110, -1, -150);
            }
            else
            {
                currentTerrain = Instantiate(Resources.Load<Terrain>("Terrains/Desert"));
                currentTerrain.transform.position = new Vector3(-110, -1, -150);
            }
        }
        else
        {
            tutorialGrid.gameObject.SetActive(true);
            tutorialCanvas.gameObject.SetActive(true);
            currentTerrain = Instantiate(Resources.Load<Terrain>("Terrains/TutorialPlayground"));
        }

        attackerPos = new();

        gameOverText.text = "GAME OVER";
        gameOverText.fontMaterial.color = Color.red;
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        paused = true;
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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

        bot.GetBoardState(grid, kingInDanger, aPosition);

        StartBotMinimax();
    }

    public void ApplyBotMinimaxResult()
    {
        if (botResult.Item1.x > 9 || botResult.Item1.y > 9)
        {
            CellWhichHoldsPiece = grid.eCamp.campGrid[botResult.Item1.x - 200, botResult.Item1.y - 200].GetComponent<GridCell>();
        }
        else
        {
            CellWhichHoldsPiece = grid.GetGridCell(botResult.Item1.x, botResult.Item1.y);
        }
        var cell = grid.GetGridCell(botResult.Item2.x, botResult.Item2.y);
        ExecutePieceMove(cell);
        playerTurn = true;
        botFinishedCalculating = false;
        duringBotMove = false;
        duringBotText.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (mainCamera != null)
        {
            if (!paused)
            {
                grid.ClearPossibleMoves(possibleMoves);
                if (StaticData.tutorial)
                {
                    tutorialGrid.ClearPossibleMoves();
                }
                var hoveredCell = MouseOverCell();

                if (botFinishedCalculating)
                {
                    if (botResult == null)
                    {
                        var text = gameOverText;
                        text.text = "YOU WIN";
                        gameOver.gameObject.SetActive(true);
                        GameEnd();
                    }
                    else
                    {
                        ApplyBotMinimaxResult();
                    }
                }
                else if (!playerTurn && botEnabled && !duringBotMove && grid.PiecesFinishedMoving())
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

                    if (Input.GetMouseButtonDown(0) && !duringBotMove && grid.PiecesFinishedMoving())
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
            if (piece.abilityCooldown == 0)
            {
                switch (piece.GetName())
                {
                    case "King":
                        {
                            var pieceList = piece.GetIsBlack() ? grid.GetBotPieces() : grid.GetPlayerPieces();
                            bool promoted = false;
                            foreach (var p in pieceList)
                            {
                                if (p.GetIsPromoted())
                                {
                                    promoted = true;
                                    break;
                                }
                            }
                            if (promoted)
                            {
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
                                abilityImage.material = grayMaterial;
                                duringKingAbility = true;
                                piece.abilityCooldown = -1;
                            }
                            break;
                        }
                    case "GoldGeneral":
                        {
                            bool turn = playerTurn;
                            var positions = abilitiesManager.Onward(piece.GetPosition(), piece.GetIsBlack());
                            foreach (var p in positions)
                            {
                                CellWhichHoldsPiece = grid.GetGridCell(p.Item1.x, p.Item1.y);
                                ExecutePieceMove(grid.GetGridCell(p.Item2.x, p.Item2.y));
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
                                CellWhichHoldsPiece = grid.GetGridCell(piece.GetPosition());
                                ExecutePieceMove(grid.GetGridCell(piecePos.x, destY));
                            }
                            abilityImage.material = grayMaterial;
                            specialAbilityInUse = false;
                            piece.abilityCooldown = 2;
                            playerTurn = !turn;
                            break;
                        }
                    case "SilverGeneral":
                    case "Rook":
                        specialAbilityInUse = true;
                        abilityImage.material = grayMaterial;
                        break;
                    default:
                        break;
                }
                if (StaticData.tutorial)
                {
                    tutorialGrid.SetAbilityUsageMessage(piece.GetName());
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
        var piece = hoveredCell.objectInThisGridSpace.GetComponent<Piece>();
        if (StaticData.tutorial)
        {
            tutorialGrid.InitializePieces(piece.GetName(), piece.GetIsDrop(), piece.GetMoveset());
        }

        if ((playerTurn && !piece.GetIsBlack()) || (!playerTurn && piece.GetIsBlack()))
        {
            HandleAbilityImageColorChange(piece);
            if (chosenPiece)
            {
                RemovePossibleMoves();
            }

            if (!kingInDanger)
            {
                PossibleMovesCalculationHandler(piece, hoveredCell);
            }
            else
            {
                if (piece.isKing)
                {
                    HandleKingClickedWhileInDanger(piece, hoveredCell);
                }
                else
                {
                    SaveTheKing(piece, hoveredCell);
                }
            }
        }
    }

    public void HandleKingClickedWhileInDanger(Piece piece, GridCell hoveredCell)
    {
        // Find valid moves for king
        possibleMoves = kingManager.ValidMovesScan(piece);
        var attacker = CellWhichHoldsAttacker.objectInThisGridSpace.GetComponent<Piece>();

        // Attacker protected -> remove attacker pos from possible moves
        bool attackerProtected = kingManager.IsAttackerProtected(attacker);
        if (attackerProtected && possibleMoves.Contains(attacker.GetPosition()))
        {
            possibleMoves.Remove(attacker.GetPosition());
        }

        var attackerPossibleMovesUnrestricted = boardManager.CalculatePossibleMoves(attacker, true);
        if (attackerPossibleMovesUnrestricted != null && attackerPossibleMovesUnrestricted.Count != 0)
        {
            possibleMoves = boardManager.CalculateOverlappingMoves(possibleMoves, attackerPossibleMovesUnrestricted, false);
        }

        var additionalDangerMoves = kingManager.KingDangerMovesScan(possibleMoves, piece.GetIsBlack());
        if (additionalDangerMoves != null && additionalDangerMoves.Count != 0)
        {
            possibleMoves = boardManager.CalculateOverlappingMoves(possibleMoves, additionalDangerMoves, false);
        }

        PossibleMovesDisplayLoop();
        CellWhichHoldsPiece = hoveredCell;
        chosenPiece = true;

        if (possibleMoves == null || possibleMoves.Count == 0)
        {
            if (GameEndCheck(piece.GetIsBlack()))
            {
                GameEnd();
            }
        }
    }

    public void SaveTheKing(Piece piece, GridCell hoveredCell)
    {
        foreach (var b in bodyguards)
        {
            if (hoveredCell.GetPosition().Equals(b.GetPosition()))
            {
                possibleMoves = new() { CellWhichHoldsAttacker.objectInThisGridSpace.GetComponent<Piece>().GetPosition() };
                PossibleMovesDisplayLoop();

                CellWhichHoldsPiece = hoveredCell;
                chosenPiece = true;
                break;
            }
        }
        if (piece.GetIsDrop())
        {
            possibleMoves = boardManager.CalculatePossibleDrops(piece);
            possibleMoves = boardManager.CalculateOverlappingMoves(possibleMoves, endangeredMoves, true);
            PossibleMovesDisplayLoop();
            CellWhichHoldsPiece = hoveredCell;
            chosenPiece = true;
        }
        else if (sacrifices != null && endangeredMoves != null)
        {
            foreach (var s in sacrifices)
            {
                if (hoveredCell.GetPosition().Equals(s.GetPosition()))
                {
                    possibleMoves = kingManager.CalculateProtectionMoves(piece, endangeredMoves); ;
                    PossibleMovesDisplayLoop();
                    CellWhichHoldsPiece = hoveredCell;
                    chosenPiece = true;
                    break;
                }
            }
        }

        if (possibleMoves == null || possibleMoves.Count == 0)
        {
            GameEnd();
        }
    }

    public bool GameEndCheck(bool isBlack)
    {
        var pieces = isBlack ? grid.GetBotPieces() : grid.GetPlayerPieces();
        if ((sacrifices != null && sacrifices.Count != 0) ||
            (bodyguards != null && bodyguards.Count != 0))
        {
            return false;
        }

        foreach (var piece in pieces)
        {
            if (piece.GetIsDrop())
            {
                possibleMoves = boardManager.CalculatePossibleDrops(piece);
                possibleMoves = boardManager.CalculateOverlappingMoves(possibleMoves, endangeredMoves, true);
                if (possibleMoves != null || possibleMoves.Count != 0)
                {
                    return false;
                }
            }
        }

        return true;
    }

    public void GameEnd()
    {
        gameOver.gameObject.SetActive(true);
        paused = true;
    }

    public void PossibleMovesCalculationHandler(Piece piece, GridCell hoveredCell)
    {
        if (piece.isKing)
        {
            possibleMoves = kingManager.ValidMovesScan(piece);
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
            var cell = grid.GetGridCell(p);
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
            grid.GetPieceInGrid(kingPos).GetComponentInChildren<MeshRenderer>().material.color =
                piece.GetIsBlack() ? Color.black : Color.white;
            kingInDanger = false;

            Piece attacker = grid.GetPieceInGrid(attackerPos).GetComponent<Piece>();

            if (piece.GetIsBlack())
            {
                attacker.ResetIsBlack();
            }
            else
            {
                attacker.SetIsBlack();
            }
        }

        //check for promotions
        if (!piece.GetIsDrop() && !piece.GetIsPromoted() && CheckForPromotion(hoveredCell, piece.GetIsBlack()))
        {
            boardManager.ApplyPromotion(piece);
            if (StaticData.tutorial)
            {
                tutorialGrid.SetPromotionTutorialMessage();
            }
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
                    if (possibleMoves != null && possibleMoves.Count != 0)
                    {
                        CellWhichHoldsPiece = grid.GetGridCell(piece.GetPosition());
                        cantChangePiece = true;
                        cantChangePossibleMoves = new(possibleMoves);
                    }
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
                                KillPiece(grid.GetGridCell(infernoResult.x, infernoResult.y));
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
                grid.eCamp.capturedPieceObjects.Remove(CellWhichHoldsPiece.objectInThisGridSpace);
                grid.eCamp.Reshuffle();
            }
            else
            {
                grid.pCamp.capturedPieceObjects.Remove(CellWhichHoldsPiece.objectInThisGridSpace);
                grid.pCamp.Reshuffle();
            }
            piece.ResetIsDrop();
        }
        else
        {
            CellWhichHoldsPiece.objectInThisGridSpace = null;
        }

        if (playerTurn)
        {
            piece.ResetIsBlack();
        }
        else
        {
            piece.SetIsBlack();
        }
    }

    public void HandleKingEndangerement(Piece piece)
    {
        Piece king = piece.GetIsBlack() ? grid.GetPlayerKing() : grid.GetBotKing();
        var piecesList = piece.GetIsBlack() ? grid.GetPlayerPieces() : grid.GetBotPieces();

        var attackerRes = kingManager.AttackerScanForKing(king, piece);
        var closeRes = kingManager.CloseScanForKing(king, piece.GetPosition());
        var farRes = kingManager.FarScanForKing(king.GetPosition(), king.GetIsBlack(), ref attackerPos);
        if (closeRes || farRes || attackerRes)
        {
            if (closeRes || attackerRes)
            {
                attackerPos = piece.GetPosition();
            }
            CellWhichHoldsAttacker = grid.GetGridCell(attackerPos);
            var attacker = grid.GetPieceInGrid(attackerPos).GetComponent<Piece>();
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
        grid.AddToCamp(hoveredCell.objectInThisGridSpace);
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
                var cell = grid.GetGridCell(r);
                cell.ResetIsPossibleMove();
                cell.GetComponentInChildren<SpriteRenderer>().material.color = Color.black;
            }
            possibleMoves = null;
        }
    }

    private GridCell MouseOverCell()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        var hit = Physics.Raycast(ray, out RaycastHit info);
        return hit ? info.transform.GetComponent<GridCell>() : null;
    }
}
