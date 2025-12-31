using System;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    public List<Position> endangeredMoves;

    private bool chosenPiece = false;

    private GridCell CellWhichHoldsPiece = null;

    [SerializeField] private bool playerTurn;

    [SerializeField] private bool botEnabled;

    [SerializeField] private ShogiBot bot;

    [SerializeField] private Canvas canvas;

    [SerializeField] private GameObject gameOver;

    [SerializeField] private Canvas mainCanvas;

    private bool duringBotMove = false;

    private bool botFinishedCalculating = false;

    private Tuple<Position, Position> botResult;

    [SerializeField] private TextMeshProUGUI duringBotText;

    private bool paused = false;

    [SerializeField] private GameObject dieAnimation;

    private readonly LogicBoardManager boardController = new();

    private LogicCell[,] logicCells = new LogicCell[9, 9];

    public static Action RequestLogicCellsUpdate;

    public static Action ResetUnitMoved;

    [SerializeField] private int movesPerPlayer = 3;

    private int doneMoves = 0;

    private bool isUnitMoving = false;

    void Start()
    {
        botEnabled = StaticData.botEnabled;
        bot.InitializeBot(StaticData.botDifficulty);

        Scene activeScene = SceneManager.GetSceneByName("Game");
        SceneManager.SetActiveScene(activeScene);

        string mapName = StaticData.map == "GrasslandsImage" ? "Grasslands" : "Desert";
        GameObject terrain = Resources.Load($"Terrains/{mapName}") as GameObject;

        if (terrain != null)
        {
            var instantiated = Instantiate(terrain, activeScene);
            instantiated.GetComponent<Transform>().transform.position = new Vector3(-30, 10, -25);
        }
    }

    private void OnEnable()
    {
        Grid.OnGridFinishRender += OnGridFinishedRendering;
        UnitModel.UnitFinishedMoving += OnUnitFinishedMoving;
        GameOverController.OnBackToMap += HandleBackToMap;
    }

    private void OnDisable()
    {
        Grid.OnGridFinishRender -= OnGridFinishedRendering;
        UnitModel.UnitFinishedMoving -= OnUnitFinishedMoving;
        GameOverController.OnBackToMap -= HandleBackToMap;
    }


    private async void HandleBackToMap()
    {
        Destroy(gameOver);
        SceneManager.SetActiveScene(SceneManager.GetSceneByName("TestMap"));
        await SceneManager.UnloadSceneAsync("Game");
    }

    /// <summary>
    /// Pauses game.
    /// </summary>
    public void PauseGame()
    {
        Time.timeScale = 0f;
        paused = true;
    }

    /// <summary>
    /// Unpauses game.
    /// </summary>
    public void ResumeGame()
    {
        Time.timeScale = 1f;
        paused = false;
    }

    /// <summary>
    /// Starts Minimax algorithm on another thread.
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
    /// Sets parameters to minimax algorithm. Shows text when Minimax is running.
    /// </summary>
    public void PrepareBotForMinimax()
    {
        duringBotText.gameObject.SetActive(true);
        duringBotMove = true;
        botFinishedCalculating = false;
        bot.GetBoardState(grid);
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

    /// <summary>
    /// Checks bot status and executes Minimax. Also checks bot result and ends game when bot doesn't have any moves left.
    /// </summary>
    private void HandleBotOperations()
    {
        if (botFinishedCalculating)
        {
            if (botResult == null)
            {
                //ShowGameOverScreen("YOU WIN", Color.green);
                //botFinishedCalculating = false;
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
    }

    /// <summary>
    /// Visualizes possible / not possible moves on board when user clicks one of his units.
    /// Highlights currently hovered cell while if no unit is chosen.
    /// </summary>
    /// <param name="hoveredCell">currently hovered cell</param>
    private void ColorBoardTiles(GridCell hoveredCell)
    {
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
    }

    void Update()
    {
        if (paused) return;

        if (mainCamera == null) return;

        grid.ClearPossibleMoves(possibleMoves);

        HandleBotOperations();

        var hoveredCell = MouseOverCell();

        if (hoveredCell == null) return;

        ColorBoardTiles(hoveredCell);

        if (Input.GetMouseButtonDown(0) && !duringBotMove && !isUnitMoving)
        {
            HandleBoardClick(hoveredCell);
        }
    }

    /// <summary>
    /// Handles input when user clicks on a board.
    /// </summary>
    /// <param name="hoveredCell">currently hovered cell</param>
    private void HandleBoardClick(GridCell hoveredCell)
    {
        if (hoveredCell.GetIsPossibleMove())
        {
            ExecutePieceMove(hoveredCell);
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
    /// Handles input when user clicks one of his units.
    /// </summary>
    public void HandlePieceClicked(GridCell hoveredCell)
    {
        var unit = hoveredCell.unitInGridCell.Unit;

        if ((playerTurn && !unit.GetIsBlack()) || (!playerTurn && unit.GetIsBlack()))
        {
            if (chosenPiece) RemovePossibleMoves();

            PossibleMovesCalculation(hoveredCell);
        }
    }

    /// <summary>
    /// Handles calculation of possible moves of a unit occupying hovered cell.
    /// </summary>
    /// <param name="hoveredCell">currently hovered cell</param>
    public void PossibleMovesCalculation(GridCell hoveredCell)
    {
        Unit unit = hoveredCell.unitInGridCell.Unit;
        if (unit.GetIsDrop())
        {
            possibleMoves = boardController.CalculatePossibleDrops(unit, logicCells);
        }
        else
        {
            possibleMoves = boardController.NewCalculatePossibleMoves(unit, logicCells);
        }

        PossibleMovesDisplayLoop();
        CellWhichHoldsPiece = hoveredCell;
        chosenPiece = true;
    }

    /// <summary>
    /// Displays possible moves.
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
    /// Handles input when user clicks previously selected unit.
    /// </summary>
    public void HandleUnclickPiece()
    {
        CellWhichHoldsPiece = null;
        RemovePossibleMoves();
        chosenPiece = false;
    }

    /// <summary>
    /// Executes piece move from stored in CellWhichHoldsPiece cell to hoveredCell destination.
    /// </summary>
    public void ExecutePieceMove(GridCell hoveredCell, bool registerMove = true)
    {
        isUnitMoving = true;
        Unit unit = CellWhichHoldsPiece.unitInGridCell.Unit;
        if (CheckForPromotion(hoveredCell, unit.GetIsBlack()))
        {
            var changedMoveset = boardController.GetPromotedUnitMoveset(unit);
            CellWhichHoldsPiece.unitInGridCell.PromoteUnit(changedMoveset);
        }

        if (hoveredCell.unitInGridCell != null)
        {
            var enemyUnit = hoveredCell.unitInGridCell.Unit;
            var isDead = enemyUnit.ReduceHP(unit.AttackPower);
            if (isDead)
            {
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
                KillPiece(hoveredCell);
                hoveredCell.unitInGridCell = null;
                Instantiate(dieAnimation, hoveredCell.GetWorldPosition(), Quaternion.identity);
                unit.MovePiece(hoveredCell.GetPosition());
                hoveredCell.SetAndMovePiece(CellWhichHoldsPiece.unitInGridCell, hoveredCell.GetWorldPosition());

                CellWhichHoldsPiece.unitInGridCell = null;
            }
            else
            {
                var enemyHealthBar = hoveredCell.unitInGridCell.healthBar;
                enemyHealthBar.GetComponent<HealthBarController>().UpdateHealthBar(enemyUnit.HealthPoints);
                if (unit.UnitName == UnitEnum.Bishop || unit.UnitName == UnitEnum.Rook || unit.UnitName == UnitEnum.Lance)
                {
                    var unitPos = unit.GetPosition();

                    Position destination = null;

                    for (int row = -1; row <= 1; row++)
                    {
                        for (int col = -1; col <= 1; col++)
                        {
                            destination = boardController.FindPositionBeforeEnemy(row, col, unitPos, logicCells, enemyUnit.GetPosition());
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
                    var attackPath = TransformationCalculator.LinearAttackTransformation(CellWhichHoldsPiece.GetWorldPosition(), hoveredCell.GetWorldPosition());
                    CellWhichHoldsPiece.unitInGridCell.SetPath(attackPath);
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

        HandleDropCheck(unit);

        RemovePossibleMoves();
        chosenPiece = false;

        if (doneMoves == movesPerPlayer)
        {
            playerTurn = !playerTurn;
            doneMoves = 0;
            ResetUnitMoved?.Invoke();
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
    /// Handles when unit drops from captured units.
    /// </summary>
    /// <param name="unit">Checked unit</param>
    public void HandleDropCheck(Unit unit)
    {
        if (unit.GetIsDrop())
        {
            if (unit.GetIsBlack())
            {
                grid.eCamp.capturedPieceObjects.Remove(CellWhichHoldsPiece.unitInGridCell);
                grid.eCamp.Reshuffle();
            }
            else
            {
                grid.pCamp.capturedPieceObjects.Remove(CellWhichHoldsPiece.unitInGridCell);
                grid.pCamp.Reshuffle();
            }
            unit.ResetIsDrop();
        }
        else
        {
            CellWhichHoldsPiece.objectInThisGridSpace = null;
        }

        if (playerTurn)
        {
            unit.ResetIsBlack();
        }
        else
        {
            unit.SetIsBlack();
        }
    }

    /// <summary>
    /// Kills piece and adds it to camp.
    /// </summary>
    /// <param name="hoveredCell">currently hovered cell</param>
    public void KillPiece(GridCell hoveredCell)
    {
        grid.AddToCamp(hoveredCell.unitInGridCell);
        hoveredCell.unitInGridCell = null;
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

    /// <summary>
    /// Assigns InputManager's logicCells from Grid
    /// </summary>
    /// <param name="logicCells">Sent logic cells from grid</param>
    private void OnGridFinishedRendering(LogicCell[,] logicCells)
        => this.logicCells = logicCells;

    /// <summary>
    /// Triggers when unit finishes moving.
    /// </summary>
    private void OnUnitFinishedMoving()
        => isUnitMoving = false;
}