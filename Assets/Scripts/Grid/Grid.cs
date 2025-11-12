using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// Symbolizes playing board. Consists of GridCells.
/// </summary>
[Serializable]
public class Grid : MonoBehaviour
{
    [SerializeField] private GameObject gridCell;

    [SerializeField] private Transform cameraPosition;

    [SerializeField] private FileManager fileManager;

    public Camp pCamp;

    public Camp eCamp;

    private UnitModel playerKing;

    private UnitModel botKing;

    private GameObject[,] gameGrid;

    private readonly float gridCellSize = 2;

    private readonly List<UnitModel> playerPieces = new();

    private readonly List<UnitModel> botPieces = new();

    private LogicCell[,] logicCells = new LogicCell[9, 9];

    public static Action<LogicCell[,]> OnGridFinishRender;

    [SerializeField] private float movementSpeed = 20f;

    private readonly int width = StaticData.battleMapWidth;

    private readonly int height = StaticData.battleMapHeight;

    private void OnEnable()
    {
        InputManager.RequestLogicCellsUpdate += UpdateLogicCells;
        InputManager.ResetUnitMoved += ResetMovedInTurn;
    }

    private void OnDisable()
    {
        InputManager.RequestLogicCellsUpdate -= UpdateLogicCells;
        InputManager.ResetUnitMoved -= ResetMovedInTurn;

    }

    private void ResetMovedInTurn()
    {
        foreach (var unit in playerPieces)
        {
            unit.Unit.MovedInTurn = false;
        }

        foreach (var unit in botPieces)
        {
            unit.Unit.MovedInTurn = false;
        }
    }

    private void UpdateLogicCells()
    {
        logicCells = new LogicCell[width, height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                var cell = this.GetGridCell(x, y);
                logicCells[x, y] = new LogicCell(cell);
                if (cell.unitInGridCell != null)
                {
                    Unit p = new(cell.unitInGridCell.Unit);
                    logicCells[x, y].unit = p;
                }
            }
        }

        OnGridFinishRender?.Invoke(logicCells);
    }

    public void Start()
    {
        pCamp.InitializePosY(2);
        eCamp.InitializePosY(0);
        GenerateField();

        var units = BattleDeploymentStaticData.playerFormation;


        InitializePieces(units);

        UpdateLogicCells();
    }

    /// <summary>
    /// Returns true when all pieces finish their move. False otherwise.
    /// </summary>
    /// <returns>bool</returns>
    public bool PiecesFinishedMoving()
    {
        //foreach (var piece in playerPieces)
        //{
        //    if (!piece.finishedMoving)
        //    {
        //        return false;
        //    }
        //}
        //foreach (var piece in botPieces)
        //{
        //    if (!piece.finishedMoving)
        //    {
        //        return false;
        //    }
        //}

        //if (!playerKing.finishedMoving || !botKing.finishedMoving)
        //{
        //    return false;
        //}

        return true;
    }

    /// <summary>
    /// Generates 9 x 9 field of GridCells.
    /// </summary>
    public void GenerateField()
    {
        //pCamp.GenerateCamp();

        float campSpacing = 1.0F + gridCellSize * 3;
        gameGrid = new GameObject[width, height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                gameGrid[x, y] = Instantiate(gridCell, new Vector4(x * gridCellSize, 11.2f, y * gridCellSize + campSpacing), Quaternion.identity);
                GridCell cell = gameGrid[x, y].GetComponent<GridCell>();
                cell.InitializeGridCell(x, y, gridCellSize);
                cell.SetPosition(x, y);
                gameGrid[x, y].transform.parent = transform;
                gameGrid[x, y].transform.rotation = Quaternion.Euler(90, 0, 0);
            }
        }
        campSpacing += 9 * gridCellSize + 1.0F;

        //eCamp.GenerateCamp(campSpacing);
    }

    /// <summary>
    /// Loads pieces from resource files and initializes them as Piece objects.
    /// </summary>
    public void InitializePieces(Unit[,] units = null)
    {
        var unitTemplates = StaticData.unitTemplates;
        if (units != null)
        {
            for (int y = 0; y < 3; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var unit = units[y, x];

                    if (unit != null)
                    {
                        UnitModel resource = Resources.Load<UnitModel>($"Prefabs/Units/{unit.UnitName.ToString()}Unit");

                        var cell = gameGrid[x, y].GetComponent<GridCell>();


                        var moveset = fileManager.GetMovesetByPieceName(unit.UnitName.ToString());

                        cell.SetUnit(resource);

                        Position unitPos = cell.GetPosition();
                        var template = unitTemplates.Single(o => o.UnitName == unit.UnitName);
                        cell.unitInGridCell.InitUnit(unit.UnitName.ToString(), moveset, unitPos.x, unitPos.y, false, movementSpeed, template);

                        var unitModel = cell.unitInGridCell;

                        //if (unit.GetIsBlack())
                        //{
                        //    if (unit.isKing) { botKing = unitModel; }
                        //    else { botPieces.Add(unitModel); }
                        //    //cell.objectInThisGridSpace.GetComponentInChildren<MeshRenderer>().material.color = Color.black;
                        //}
                        //else
                        //{
                        //    if (unit.isKing) { playerKing = unitModel; }
                        //    else { playerPieces.Add(unitModel); }
                        //    //cell.objectInThisGridSpace.GetComponentInChildren<MeshRenderer>().material.color = Color.white;
                        //    //cell.objectInThisGridSpace.GetComponentInChildren<Transform>().rotation = Quaternion.Euler(0, 180, 0);
                        //}
                    }
                }
            }
        }
        else
        {
            var piecesPositions = fileManager.PiecesPositions.boardPositions;



            foreach (var p in piecesPositions)
            {
                var name = p.piece;

                UnitModel resource = Resources.Load<UnitModel>($"Prefabs/Units/{name}Unit");

                if (resource != null)
                {


                    var cell = gameGrid[p.posX, p.posY].GetComponent<GridCell>();

                    var moveset = fileManager.GetMovesetByPieceName(p.piece);

                    cell.SetUnit(resource);

                    Position unitPos = cell.GetPosition();
                    Unit template = null;

                    for (int i = 0; i < unitTemplates.Count; i++)
                    {
                        var unitname = unitTemplates[i].UnitName.ToString();

                        if (unitname == p.piece)
                        {
                            template = unitTemplates[i];
                            break;
                        }
                    }

                    cell.unitInGridCell.InitUnit(p.piece, moveset, unitPos.x, unitPos.y, false, movementSpeed, template);



                    var unitModel = cell.unitInGridCell;

                    if (unitModel.Unit.GetIsBlack())
                    {
                        if (unitModel.Unit.isKing) { botKing = unitModel; }
                        else { botPieces.Add(unitModel); }

                        unitModel.Model.GetComponentInChildren<MeshRenderer>().material.color = Color.black;
                    }
                    else
                    {
                        if (unitModel.Unit.isKing) { playerKing = unitModel; }
                        else { playerPieces.Add(unitModel); }
                        unitModel.Model.GetComponentInChildren<MeshRenderer>().material.color = Color.white;
                        unitModel.Model.GetComponentInChildren<Transform>().rotation = Quaternion.Euler(-90, 180, 0);
                    }
                }
            }

        }

        //foreach (var p in piecesPositions)
        //{
        //    var name = p.piece;
        //    var resource = Resources.Load("Prefabs/Piece/" + name + "Piece") as GameObject;
        //    if (resource != null)
        //    {
        //        var cell = gameGrid[p.posX, p.posY].GetComponent<GridCell>();
        //        var moveset = fileManager.GetMovesetByPieceName(p.piece);
        //        bool isSpecialPiece = SpecialPieceCheck(p.piece);





        //        cell.SetPiece(resource);
        //        var pieceScript = cell.objectInThisGridSpace.GetComponent<Piece>();
        //        Position piecePos = cell.GetPosition();
        //        pieceScript.InitializePiece(p.piece, moveset, piecePos.x, piecePos.y, isSpecialPiece);




        //        if (pieceScript.GetIsBlack())
        //        {
        //            if (pieceScript.isKing) { botKing = pieceScript; }
        //            else { botPieces.Add(pieceScript); }
        //            cell.objectInThisGridSpace.GetComponentInChildren<MeshRenderer>().material.color = Color.black;
        //        }
        //        else
        //        {
        //            if (pieceScript.isKing) { playerKing = pieceScript; }
        //            else { playerPieces.Add(pieceScript); }
        //            cell.objectInThisGridSpace.GetComponentInChildren<MeshRenderer>().material.color = Color.white;
        //            cell.objectInThisGridSpace.GetComponentInChildren<Transform>().rotation = Quaternion.Euler(0, 180, 0);
        //        }
        //    }
        //}
    }

    /// <summary>
    /// Check if checked piece is special.
    /// </summary>
    /// <returns>bool</returns>
    private bool SpecialPieceCheck(string name)
    {
        if (name == "Rook" || name == "Bishop")
        {
            return true;
        }
        return false;
    }

    /// <summary>
    /// Returns piece GameObject in grid.
    /// </summary>
    /// <param name="x">x position</param>
    /// <param name="y">y position</param>
    /// <returns>GameObject</returns>
    public GameObject GetPieceInGrid(int x, int y)
    {
        if (gameGrid[x, y].GetComponent<GridCell>().objectInThisGridSpace != null)
        {
            return gameGrid[x, y].GetComponentInChildren<GridCell>().objectInThisGridSpace;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Returns piece GameObject in grid.
    /// </summary>
    /// <param name="pos">Position class</param>
    /// <returns>GameObject</returns>
    public GameObject GetPieceInGrid(Position pos)
    {
        if (gameGrid[pos.x, pos.y].GetComponent<GridCell>().objectInThisGridSpace != null)
        {
            return gameGrid[pos.x, pos.y].GetComponentInChildren<GridCell>().objectInThisGridSpace;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Gets grid cell in grid.
    /// </summary>
    /// <param name="x">x position</param>
    /// <param name="y">y position</param>
    /// <returns>GridCell</returns>
    public GridCell GetGridCell(int x, int y)
        => gameGrid[x, y].GetComponent<GridCell>();

    /// <summary>
    /// Gets grid cell in grid.
    /// </summary>
    /// <param name="p">Position class</param>
    /// <returns>GridCell</returns>
    public GridCell GetGridCell(Position p)
        => gameGrid[p.x, p.y].GetComponent<GridCell>();

    /// <summary>
    /// Adds killed piece to camp.
    /// </summary>
    /// <param name="piece">killed piece GameObject</param>
    public void AddToCamp(GameObject piece)
    {
        throw new NotImplementedException();
        //Piece p = piece.GetComponent<Piece>();
        //if (p.GetIsBlack())
        //{
        //    p.GetComponentInChildren<Transform>().rotation = Quaternion.Euler(0, 180, 0);
        //    p.MovePiece(new(100, 100));
        //    playerPieces.Add(p);
        //    botPieces.Remove(p);
        //    pCamp.AddToCamp(piece);
        //}
        //else
        //{
        //    p.GetComponentInChildren<Transform>().rotation = Quaternion.Euler(0, 0, 0);
        //    p.MovePiece(new(200, 200));
        //    botPieces.Add(p);
        //    playerPieces.Remove(p);
        //    eCamp.AddToCamp(piece);
        //}
    }

    /// <summary>
    /// Gets player (white) pieces.
    /// </summary>
    public List<UnitModel> GetPlayerPieces()
        => playerPieces;

    /// <summary>
    /// Gets bot (black) pieces.
    /// </summary>
    public List<UnitModel> GetBotPieces()
        => botPieces;

    /// <summary>
    /// Gets player (white) king.
    /// </summary>
    public UnitModel GetPlayerKing()
        => playerKing;

    /// <summary>
    /// Gets bot (black) king.
    /// </summary>
    public UnitModel GetBotKing()
        => botKing;

    /// <summary>
    /// Restores default color of GridCell on mouse hover exit.
    /// </summary>
    public void OnHoverExitRestoreDefaultColor()
    {
        Color defaultColor = new(0.04f, 0.43f, 0.96f);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                gameGrid[x, y].GetComponentInChildren<SpriteRenderer>().material.color = defaultColor;
            }
        }
        for (int y = 0; y < 3; y++)
        {
            //for (int x = 0; x < width; x++)
            //{
            //    pCamp.campGrid[x, y].GetComponentInChildren<SpriteRenderer>().material.color = defaultColor;
            //    eCamp.campGrid[x, y].GetComponentInChildren<SpriteRenderer>().material.color = defaultColor;
            //}
        }
    }

    /// <summary>
    /// Clears possible moves from board, ignores those moves on blacklist.
    /// </summary>
    public void ClearPossibleMoves(List<Position> blacklist = null)
    {
        OnHoverExitRestoreDefaultColor();
        if (blacklist != null)
        {
            foreach (var item in blacklist)
            {
                gameGrid[item.x, item.y].GetComponentInChildren<SpriteRenderer>().material.color = Color.green;
            }
        }
    }
}