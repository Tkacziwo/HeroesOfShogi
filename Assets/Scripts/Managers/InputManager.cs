using NUnit.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Manager which handles all input in the game.
/// </summary>
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

    private LogicBoardManager newBestBoardManager = new();

    private LogicKingManager newBestKingManager = new();

    private LogicCell[,] logicCells = new LogicCell[9, 9];

    public static Action RequestLogicCellsUpdate;

    public static Action ResetUnitMoved;

    [SerializeField] private int movesPerPlayer = 3;

    private int doneMoves = 0;

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
                currentTerrain.transform.position = new Vector3(-110, 10, -150);
            }
            else
            {
                currentTerrain = Instantiate(Resources.Load<Terrain>("Terrains/Desert"));
                currentTerrain.transform.position = new Vector3(-110, 10, -150);
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

    private void OnEnable()
    {
        Grid.OnGridFinishRender += HandleGridFinishedRendering;
    }
    private void OnDisable()
    {
        Grid.OnGridFinishRender -= HandleGridFinishedRendering;
    }

    private void HandleGridFinishedRendering(LogicCell[,] logicCells)
    {
        this.logicCells = logicCells;
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        paused = true;
    }

    public async void Restart()
    {
        await SceneManager.UnloadSceneAsync(SceneManager.GetActiveScene().buildIndex);
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ResumeGame()
    {
        paused = false;
        Time.timeScale = 1f;
    }

    /// <summary>
    /// Starts Minimax algorithm.
    /// </summary>
    public void StartBotMinimax()
    {
        Thread botThread = new(() =>
        {
            botResult = bot.ApplyMoveToRealBoard();
            botFinishedCalculating = true;
        });

        botThread.Start();
    }

    /// <summary>
    /// Prepares bot for Minimax algorithm by setting adequate fields.
    /// </summary>
    public void PrepareBotForMinimax()
    {
        duringBotText.gameObject.SetActive(true);
        duringBotMove = true;
        botFinishedCalculating = false;
        Position aPosition = new(attackerPos);

        bot.GetBoardState(grid, kingInDanger, aPosition);

        StartBotMinimax();
    }

    /// <summary>
    /// Applies Minimax algorithm result to real board.
    /// </summary>
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

        doneMoves = 0;
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
                if (hoveredCell == null) return;


                if (possibleMoves == null)
                {
                    hoveredCell.GetComponentInChildren<SpriteRenderer>().material.color = Color.magenta;
                }
                else
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

    /// <summary>
    /// Additional handler to King's ability.
    /// </summary>
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

    /// <summary>
    /// Additional handler to abilities which grant extra move.
    /// </summary>
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

    /// <summary>
    /// Handler activated when player clicks special ability window.
    /// </summary>
    public void HandleSpecialAbilityUsage()
    {
        throw new NotImplementedException();
        //if (chosenPiece)
        //{
        //    var piece = CellWhichHoldsPiece.objectInThisGridSpace.GetComponent<Piece>();
        //    if (piece.abilityCooldown == 0)
        //    {
        //        switch (piece.GetName())
        //        {
        //            case "King":
        //                {
        //                    var pieceList = piece.GetIsBlack() ? grid.GetBotPieces() : grid.GetPlayerPieces();
        //                    bool promoted = false;
        //                    foreach (var p in pieceList)
        //                    {
        //                        if (p.GetIsPromoted())
        //                        {
        //                            promoted = true;
        //                            break;
        //                        }
        //                    }
        //                    if (promoted)
        //                    {
        //                        foreach (var p in pieceList)
        //                        {
        //                            if (p.GetIsPromoted())
        //                            {
        //                                promotedPieces.Add(p);
        //                                p.GetComponentInChildren<MeshRenderer>().material.color = Color.yellow;
        //                            }
        //                            else
        //                            {
        //                                p.GetComponentInChildren<MeshRenderer>().material.color = Color.blue;
        //                                nonPromotedPieces.Add(p);
        //                            }
        //                        }
        //                        abilityImage.material = grayMaterial;
        //                        duringKingAbility = true;
        //                        piece.abilityCooldown = -1;
        //                    }
        //                    break;
        //                }
        //            case "GoldGeneral":
        //                {
        //                    bool turn = playerTurn;
        //                    var positions = abilitiesManager.Onward(piece.GetPosition(), piece.GetIsBlack());
        //                    foreach (var p in positions)
        //                    {
        //                        CellWhichHoldsPiece = grid.GetGridCell(p.Item1.x, p.Item1.y);
        //                        ExecutePieceMove(grid.GetGridCell(p.Item2.x, p.Item2.y));
        //                    }
        //                    int destY;
        //                    var piecePos = piece.GetPosition();
        //                    if (piece.GetIsBlack())
        //                    {
        //                        destY = piecePos.y - 1;
        //                    }
        //                    else
        //                    {
        //                        destY = piecePos.y + 1;
        //                    }
        //                    if (boardManager.IsInBoard(destY, piecePos.x) && boardManager.IsCellFree(piecePos.x, destY))
        //                    {
        //                        CellWhichHoldsPiece = grid.GetGridCell(piece.GetPosition());
        //                        ExecutePieceMove(grid.GetGridCell(piecePos.x, destY));
        //                    }
        //                    abilityImage.material = grayMaterial;
        //                    specialAbilityInUse = false;
        //                    piece.abilityCooldown = 2;
        //                    playerTurn = !turn;
        //                    break;
        //                }
        //            case "SilverGeneral":
        //            case "Rook":
        //                specialAbilityInUse = true;
        //                abilityImage.material = grayMaterial;
        //                break;
        //            default:
        //                break;
        //        }
        //        if (StaticData.tutorial)
        //        {
        //            tutorialGrid.SetAbilityUsageMessage(piece.GetName());
        //        }
        //    }
        //}
    }

    /// <summary>
    /// Handler for clicking cell on the board.
    /// </summary>
    private void HandleBoardClick(GridCell hoveredCell)
    {
        if (hoveredCell.GetIsPossibleMove())
        {
            ExecutePieceMove(hoveredCell);
            //playerTurn = !playerTurn;
        }
        else if (CellWhichHoldsPiece != null && CellWhichHoldsPiece.GetPosition().Equals(hoveredCell.GetPosition()))
        {
            HandleUnclickPiece();
        }
        else if (hoveredCell.unitInGridCell != null)
        {
            HandlePieceClicked(hoveredCell);
        }
    }

    /// <summary>
    /// Handler for clicking piece on the board.
    /// </summary>

    public void HandlePieceClicked(GridCell hoveredCell)
    {
        var unit = hoveredCell.unitInGridCell.Unit;

        if (unit.MovedInTurn) { Debug.Log("Already made move"); return; }


        //if (StaticData.tutorial)
        //{
        //    tutorialGrid.InitializePieces(piece.GetName(), piece.GetIsDrop(), piece.GetMoveset());
        //}

        if ((playerTurn && !unit.GetIsBlack()) || (!playerTurn && unit.GetIsBlack()))
        {
            //HandleAbilityImageColorChange(unit);
            if (chosenPiece) RemovePossibleMoves();

            if (!kingInDanger) PossibleMovesCalculation(unit, hoveredCell);

            //else
            //{
            //    if (piece.isKing)
            //    {
            //        HandleKingClickedWhileInDanger(piece, hoveredCell);
            //    }
            //    else
            //    {
            //        SaveTheKing(piece, hoveredCell);
            //    }
            //}
        }

        //if ((playerTurn && !piece.GetIsBlack()) || (!playerTurn && piece.GetIsBlack()))
        //{
        //    HandleAbilityImageColorChange(piece);
        //    if (chosenPiece)
        //    {
        //        RemovePossibleMoves();
        //    }

        //    if (!kingInDanger)
        //    {
        //        PossibleMovesCalculationHandler(piece, hoveredCell);
        //    }
        //    else
        //    {
        //        if (piece.isKing)
        //        {
        //            HandleKingClickedWhileInDanger(piece, hoveredCell);
        //        }
        //        else
        //        {
        //            SaveTheKing(piece, hoveredCell);
        //        }
        //    }
        //}
    }

    /// <summary>
    /// Handler for activating calculation methods in BoardManager or KingManager.
    /// </summary>
    public void PossibleMovesCalculation(Unit unit, GridCell hoveredCell)
    {
        if (unit.GetIsDrop())
        {
            //possibleMoves = newBestBoardManager.CalculatePossibleDrops(unit);
        }
        else
        {
            possibleMoves = newBestBoardManager.NewCalculatePossibleMoves(unit, logicCells);
            //boardManager.CheckIfMovesAreLegal(ref possibleMoves, piece);
        }

        PossibleMovesDisplayLoop();
        CellWhichHoldsPiece = hoveredCell;
        chosenPiece = true;
    }

    /// <summary>
    /// Handler for King while King is in danger. Ends game if there are no possible moves for King or his pieces left.
    /// </summary>
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

    /// <summary>
    /// Handler for guards, sacrifices, drop sacrifices while king is in danger.
    /// </summary>
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

    /// <summary>
    /// Check for Game End. Returns true when game is lost, false otherwise.
    /// </summary>
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
            //[ToDo] adjust
            //if (piece.Unit.GetIsDrop())
            //{
            //    possibleMoves = boardManager.CalculatePossibleDrops(piece);
            //    possibleMoves = boardManager.CalculateOverlappingMoves(possibleMoves, endangeredMoves, true);
            //    if (possibleMoves != null || possibleMoves.Count != 0)
            //    {
            //        return false;
            //    }
            //}
        }

        return true;
    }

    /// <summary>
    /// Ends game.
    /// </summary>
    public void GameEnd()
    {
        gameOver.gameObject.SetActive(true);
        paused = true;
    }

    /// <summary>
    /// Loop which displays possible moves.
    /// </summary>
    public void PossibleMovesDisplayLoop()
    {
        foreach (var p in possibleMoves)
        {
            var cell = grid.GetGridCell(p);
            cell.SetIsPossibleMove();
            cell.GetComponentInChildren<SpriteRenderer>().material.color = Color.green;
        }
    }

    /// <summary>
    /// Handler for unclicking piece.
    /// </summary>
    public void HandleUnclickPiece()
    {
        abilityImage.sprite = null;
        CellWhichHoldsPiece = null;
        RemovePossibleMoves();
        chosenPiece = false;
        specialAbilityInUse = false;
    }

    /// <summary>
    /// Executes piece move from stored in CellWhichHoldsPiece cell to hoveredCell destination.
    /// </summary>
    public void ExecutePieceMove(GridCell hoveredCell, bool registerMove = true)
    {
        //Piece piece = CellWhichHoldsPiece.objectInThisGridSpace.GetComponent<Piece>();
        Unit unit = CellWhichHoldsPiece.unitInGridCell.Unit;

        //if (kingInDanger)
        //{
        //    grid.GetPieceInGrid(kingPos).GetComponentInChildren<MeshRenderer>().material.color =
        //        piece.GetIsBlack() ? Color.black : Color.white;
        //    kingInDanger = false;

        //    Piece attacker = grid.GetPieceInGrid(attackerPos).GetComponent<Piece>();

        //    if (piece.GetIsBlack())
        //    {
        //        attacker.ResetIsBlack();
        //    }
        //    else
        //    {
        //        attacker.SetIsBlack();
        //    }
        //}

        //check for promotions
        //if (!piece.GetIsDrop() && !piece.GetIsPromoted() && CheckForPromotion(hoveredCell, piece.GetIsBlack()))
        //{
        //    boardManager.ApplyPromotion(piece);
        //    if (StaticData.tutorial)
        //    {
        //        tutorialGrid.SetPromotionTutorialMessage();
        //    }
        //}

        //[ToDo] handle new kill stuff



        if (hoveredCell.unitInGridCell != null)
        {
            var enemyUnit = hoveredCell.unitInGridCell.Unit;

            var isDead = enemyUnit.ReduceHP(unit.AttackPower);

            if (isDead)
            {
                Destroy(hoveredCell.unitInGridCell.gameObject);
                hoveredCell.unitInGridCell = null;
                Instantiate(dieAnimation, hoveredCell.GetWorldPosition(), Quaternion.identity);
                unit.MovePiece(hoveredCell.GetPosition());
                hoveredCell.SetAndMovePiece(CellWhichHoldsPiece.unitInGridCell, hoveredCell.GetWorldPosition());

                CellWhichHoldsPiece.unitInGridCell = null;

                if(enemyUnit.isKing)
                {
                    GameEnd();
                }
            }
            else
            {
                if (unit.UnitName == UnitEnum.Bishop || unit.UnitName == UnitEnum.Rook || unit.UnitName == UnitEnum.Lance)
                {
                    var unitPos = unit.GetPosition();


                    Position destination = null;

                    for (int row = -1; row <= 1; row++)
                    {
                        for (int col = -1; col <= 1; col++)
                        {
                            destination = newBestBoardManager.FindPositionBeforeEnemy(row, col, unitPos, logicCells, enemyUnit.GetPosition());
                        }
                    }

                    if(destination != null)
                    {
                        unit.MovePiece(destination);
                        var beforeEnemyCell = grid.GetGridCell(destination);
                        beforeEnemyCell.SetAndMovePiece(CellWhichHoldsPiece.unitInGridCell, beforeEnemyCell.GetWorldPosition());
                        CellWhichHoldsPiece.unitInGridCell = null;
                    }
                }
            }

            RequestLogicCellsUpdate?.Invoke();
            unit.MovedInTurn = true;
            doneMoves++;
        }
        else
        {
            unit.MovePiece(hoveredCell.GetPosition());
            hoveredCell.SetAndMovePiece(CellWhichHoldsPiece.unitInGridCell, hoveredCell.GetWorldPosition());
            CellWhichHoldsPiece.unitInGridCell = null;
            RequestLogicCellsUpdate?.Invoke();
            unit.MovedInTurn = true;
            doneMoves++;
        }

        //var handlePieceKillResult = HandlePieceKill(hoveredCell, unit);





        //unit.MovePiece(hoveredCell.GetPosition());
        ////if (piece.abilityCooldown > 0)
        ////{
        ////    piece.abilityCooldown--;
        ////}

        ////grid.GetGridCell(CellWhichHoldsPiece.GetPosition()).unitInGridCell = null;
        //hoveredCell.SetAndMovePiece(CellWhichHoldsPiece.unitInGridCell, hoveredCell.GetWorldPosition());
        //CellWhichHoldsPiece.unitInGridCell = null;
        //RequestLogicCellsUpdate?.Invoke();

        //HandleDropCheck(piece);

        RemovePossibleMoves();
        chosenPiece = false;

        if (playerTurn && doneMoves == movesPerPlayer)
        {
            playerTurn = !playerTurn;
            doneMoves = 0;
            ResetUnitMoved?.Invoke();
        }

        //if (specialAbilityInUse)
        //{
        //    switch (piece.GetName())
        //    {
        //        case "SilverGeneral":
        //            var turn = playerTurn;
        //            playerTurn = !turn;
        //            possibleMoves = abilitiesManager.Rush(piece.GetPosition());
        //            if (possibleMoves != null && possibleMoves.Count != 0)
        //            {
        //                CellWhichHoldsPiece = grid.GetGridCell(piece.GetPosition());
        //                cantChangePiece = true;
        //                cantChangePossibleMoves = new(possibleMoves);
        //            }
        //            specialAbilityInUse = false;
        //            piece.abilityCooldown = 4;
        //            break;
        //        case "Rook":
        //            {
        //                if (handlePieceKillResult.Item1)
        //                {
        //                    Position p = new(hoveredCell.GetPosition());
        //                    var infernoResult = abilitiesManager.Inferno(p, handlePieceKillResult.Item2);
        //                    if (infernoResult != null)
        //                    {
        //                        KillPiece(grid.GetGridCell(infernoResult.x, infernoResult.y));
        //                    }
        //                    specialAbilityInUse = false;
        //                    piece.abilityCooldown = -1;
        //                }
        //                break;
        //            }
        //        default:
        //            break;
        //    }
        //}

        if (!unit.isKing)
        {
            HandleKingEndangerement(unit);
        }
    }

    /// <summary>
    /// Handle if clicked piece is drop.
    /// </summary>
    /// <param name="piece"></param>
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

    /// <summary>
    /// Handle if King is in danger after executing move.
    /// </summary>
    /// <param name="unitModel"></param>
    public void HandleKingEndangerement(Unit unit)
    {
        //[ToDo] adjust
        //UnitModel king = unit.GetIsBlack() ? grid.GetPlayerKing() : grid.GetBotKing();
        //var piecesList = !unit.GetIsBlack() ? grid.GetPlayerPieces() : grid.GetBotPieces();

        ////var attackerRes = newBestKingManager.AttackerScanForKing(king, unit);
        //var closeRes = newBestKingManager.CloseScanForKing(king.Unit, logicCells, unit.GetPosition());
        //var farRes = newBestKingManager.FarScanForKing(king.Unit.GetPosition(), king.Unit.GetIsBlack(),logicCells, piecesList.Select(o =>o.Unit).ToList(), ref attackerPos);
        //if (closeRes || farRes)
        //{

        //    CellWhichHoldsAttacker = grid.GetGridCell(attackerPos);
        //    var attacker = grid.GetPieceInGrid(attackerPos).GetComponent<Piece>();
        //    kingInDanger = true;
        //    king.GetComponentInChildren<MeshRenderer>().material.color = Color.red;
        //}
    }

    /// <summary>
    /// Ability icon color change when used.
    /// </summary>
    /// <param name="piece"></param>
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

    /// <summary>
    /// Handles when piece is killed.
    /// </summary>
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

    /// <summary>
    /// Kills piece and adds it to camp.
    /// </summary>
    /// <param name="hoveredCell"></param>
    public void KillPiece(GridCell hoveredCell)
    {
        grid.AddToCamp(hoveredCell.objectInThisGridSpace);
        hoveredCell.objectInThisGridSpace = null;
        Instantiate(dieAnimation, hoveredCell.GetWorldPosition(), Quaternion.identity);
    }

    /// <summary>
    /// Checks if piece after moving got promoted.
    /// </summary>
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

    /// <summary>
    /// Removes possible moves.
    /// </summary>
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

    /// <summary>
    /// Raycast function to return GridCell which mouse hovers above.
    /// </summary>
    /// <returns>GridCell</returns>
    private GridCell MouseOverCell()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        var hit = Physics.Raycast(ray, out RaycastHit info);
        return hit ? info.transform.GetComponent<GridCell>() : null;
    }
}
