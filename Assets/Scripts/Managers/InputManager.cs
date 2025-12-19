using NUnit.Framework.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
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

    [SerializeField] private GameObject gameOver;

    [SerializeField] private AbilitiesManager abilitiesManager;

    [SerializeField] private Image abilityImage;

    [SerializeField] private Material grayMaterial;

    [SerializeField] private Canvas tutorialCanvas;

    [SerializeField] private Canvas mainCanvas;

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

    private bool isUnitMoving = false;

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

        Scene active = SceneManager.GetSceneByName("Game");
        SceneManager.SetActiveScene(active);

        if (StaticData.tutorial)
        {
            tutorialGrid.gameObject.SetActive(true);
            tutorialCanvas.gameObject.SetActive(true);
            currentTerrain = Instantiate(Resources.Load<Terrain>("Terrains/TutorialPlayground"));
        }
        else
        {
            tutorialGrid.gameObject.SetActive(false);
            tutorialCanvas.gameObject.SetActive(false);

            string mapName = StaticData.map == "GrasslandsImage" ? "Grasslands" : "Desert";

            GameObject terrain = Resources.Load($"Terrains/{mapName}") as GameObject;

            if (terrain != null)
            {
                var instantiated = Instantiate(terrain, active);
                instantiated.GetComponent<Transform>().transform.position = new Vector3(-30, 10, -25);
            }
        }

        attackerPos = new();
    }

    private void OnEnable()
    {
        Grid.OnGridFinishRender += HandleGridFinishedRendering;
        UnitModel.UnitFinishedMoving += HandleUnitFinishedMoving;
        GameOverController.OnBackToMap += HandleBackToMap;
    }
    private void OnDisable()
    {
        Grid.OnGridFinishRender -= HandleGridFinishedRendering;
        UnitModel.UnitFinishedMoving -= HandleUnitFinishedMoving;
        GameOverController.OnBackToMap -= HandleBackToMap;
    }

    private void HandleUnitFinishedMoving()
    {
        isUnitMoving = false;
    }

    private async void HandleBackToMap()
    {
        Destroy(gameOver);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("TestMap"));
        await SceneManager.UnloadSceneAsync("Game");
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


    private int botMoves = 3;

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
        //playerTurn = true;
        botFinishedCalculating = false;
        duringBotMove = false;
        duringBotText.gameObject.SetActive(false);
        botMoves--;
        //doneMoves = 0; 
        ExecutePieceMove(cell);


        botResult = null;
    }

    int maxPossibleMovesForBot = 0;

    // Update is called once per frame
    void Update()
    {
        if (paused) return;

        if (mainCamera == null) return;

        grid.ClearPossibleMoves(possibleMoves);
        if (StaticData.tutorial)
        {
            tutorialGrid.ClearPossibleMoves();
        }

        if (botFinishedCalculating)
        {
            if (botResult == null)
            {
                ShowGameOverScreen("YOU WIN", Color.green);
                botFinishedCalculating = false;
                EndTurn();
            }
            else
            {
                ApplyBotMinimaxResult();


            }
        }
        else if (!playerTurn && botEnabled && !duringBotMove && !isUnitMoving)
        {
            var botUnits = grid.GetBotPieces();

            //incude king thats why is + 1
            maxPossibleMovesForBot = Math.Min(botUnits.Count, movesPerPlayer) + 1;

            if (doneMoves == maxPossibleMovesForBot)
            {
                maxPossibleMovesForBot = 0;
                EndTurn();
            }
            else
            {
                PrepareBotForMinimax();
            }
        }


        var hoveredCell = MouseOverCell();

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

        if (Input.GetMouseButtonDown(0) && !duringBotMove && !isUnitMoving)
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
        isUnitMoving = true;
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

        if (CheckForPromotion(hoveredCell, unit.GetIsBlack()))
        {
            var changedMoveset = newBestBoardManager.GetPromotedUnitMoveset(unit);
            CellWhichHoldsPiece.unitInGridCell.PromoteUnit(changedMoveset);
        }

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

                if (enemyUnit.UnitName == UnitEnum.King)
                {
                    grid.SetWinner(unit.GetIsBlack());
                    if (enemyUnit.GetIsBlack())
                    {
                        ShowGameOverScreen("YOU WIN", Color.green);
                    }
                    else
                    {
                        ShowGameOverScreen("YOU LOSE", Color.red);
                    }
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
                            if (destination != null)
                            {
                                break;
                            }
                        }

                        if (destination != null)
                        {
                            break;
                        }
                    }


                    if (destination != null && !destination.Equals(enemyUnit.GetPosition()))
                    {
                        unit.MovePiece(destination);
                        var beforeEnemyCell = grid.GetGridCell(destination);
                        beforeEnemyCell.SetAndMovePiece(CellWhichHoldsPiece.unitInGridCell, beforeEnemyCell.GetWorldPosition());
                        CellWhichHoldsPiece.unitInGridCell = null;
                    }
                    else
                    {
                        var cell = grid.GetGridCell(CellWhichHoldsPiece.GetPosition());
                        unit.MovePiece(cell.GetPosition());
                        cell.SetAndMovePiece(CellWhichHoldsPiece.unitInGridCell, cell.GetWorldPosition());
                    }
                }
                else
                {
                    var cell = grid.GetGridCell(CellWhichHoldsPiece.GetPosition());
                    unit.MovePiece(cell.GetPosition());
                    cell.SetAndMovePiece(CellWhichHoldsPiece.unitInGridCell, cell.GetWorldPosition());
                }
            }
        }
        else
        {
            unit.MovePiece(hoveredCell.GetPosition());
            hoveredCell.SetAndMovePiece(CellWhichHoldsPiece.unitInGridCell, hoveredCell.GetWorldPosition());
            CellWhichHoldsPiece.unitInGridCell = null;
        }

        RequestLogicCellsUpdate?.Invoke();
        unit.MovedInTurn = true;
        doneMoves++;

        //HandleDropCheck(piece);

        RemovePossibleMoves();
        chosenPiece = false;

        if (doneMoves == movesPerPlayer)
        {
            playerTurn = !playerTurn;
            doneMoves = 0;
            ResetUnitMoved?.Invoke();
        }

        if (!unit.isKing)
        {
            HandleKingEndangerement(unit);
        }
    }

    private void ShowGameOverScreen(string text, Color color)
    {
        gameOver = Instantiate(gameOver);
        gameOver.transform.SetParent(mainCanvas.transform);
        this.gameOver.GetComponent<RectTransform>().anchoredPosition = new(0, 0);
        gameOver.GetComponent<GameOverController>().SetText(text, color);
    }

    public void EndTurn()
    {
        chosenPiece = false;

        playerTurn = !playerTurn;
        doneMoves = 0;
        ResetUnitMoved?.Invoke();
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
