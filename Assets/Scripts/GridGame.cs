using System;
using System.Collections.Generic;
using UnityEngine;

public class GridGame : MonoBehaviour
{
    [SerializeField] public int width, height;

    [SerializeField] private GameObject gridCell;

    [SerializeField] private Transform cameraPosition;

    [SerializeField] private float gridCellSize;

    public float xRot = 40;

    public float yRot = -90;

    public GameObject[,] gameGrid;

    public GameObject[,] playerCamp;

    public int playerCampPos = 0;

    public int playerCampPiecesNumber = 0;

    public int playerCampPosY;

    public int enemyCampPos = 0;

    public int enemyCampPiecesNumber = 0;

    public int enemyCampPosY;

    public GameObject[,] enemyCamp;

    private FileManager fileManager;

    public Camp pCamp;

    public Camp eCamp;

    public List<Piece> playerPieces = new();

    public Piece playerKing;

    public List<Piece> botPieces = new();

    public Piece botKing;

    public void Start()
    {
        gridCellSize *= 2;
        fileManager = FindFirstObjectByType<FileManager>();
        var camps = FindObjectsByType<Camp>(FindObjectsSortMode.InstanceID);
        pCamp = camps[0];
        eCamp = camps[1];
        pCamp.InitializePosY(2);
        eCamp.InitializePosY(0);
        GenerateField();
        InitializePieces();
        playerCampPosY = 2;
        enemyCampPosY = 0;
    }

    public void GenerateField()
    {

        pCamp.GenerateCamp();

        float campSpacing = 1.0F + gridCellSize * 3;
        gameGrid = new GameObject[width, height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                gameGrid[x, y] = Instantiate(gridCell, new Vector4(x * gridCellSize, 0, y * gridCellSize + campSpacing), Quaternion.identity);
                GridCell cell = gameGrid[x, y].GetComponent<GridCell>();
                cell.InitializeGridCell(x, y, gridCellSize);
                cell.SetPosition(x, y);
                gameGrid[x, y].transform.parent = transform;
                gameGrid[x, y].transform.rotation = Quaternion.Euler(90, 0, 0);
            }
        }
        campSpacing += 9 * gridCellSize + 1.0F;

        eCamp.GenerateCamp(campSpacing);
    }

    public void InitializePieces()
    {
        var piecesPositions = fileManager.PiecesPositions.boardPositions;

        foreach (var p in piecesPositions)
        {
            var name = p.piece;
            var resource = Resources.Load(name + "Piece") as GameObject;
            if (resource != null)
            {
                var cell = gameGrid[p.posX, p.posY].GetComponent<GridCell>();
                var moveset = fileManager.GetMovesetByPieceName(p.piece);
                bool isSpecialPiece = SpecialPieceCheck(p.piece);
                cell.SetPiece(resource);
                var pieceScript = cell.objectInThisGridSpace.GetComponent<Piece>();
                Position piecePos = cell.GetPosition();
                pieceScript.InitializePiece(p.piece, moveset, piecePos.x, piecePos.y, isSpecialPiece);

                if (pieceScript.GetIsBlack())
                {
                    if (pieceScript.isKing) { botKing = pieceScript; }
                    else { botPieces.Add(pieceScript); }
                    cell.objectInThisGridSpace.GetComponentInChildren<MeshRenderer>().material.color = Color.black;
                }
                else
                {
                    if (pieceScript.isKing) { playerKing = pieceScript; }
                    else { playerPieces.Add(pieceScript); }
                    cell.objectInThisGridSpace.GetComponentInChildren<MeshRenderer>().material.color = Color.white;
                    cell.objectInThisGridSpace.GetComponentInChildren<Transform>().rotation = Quaternion.Euler(0, 180, 0);
                }
            }
        }
    }

    private bool SpecialPieceCheck(string name)
    {
        if (name == "Rook" || name == "Bishop")
        {
            return true;
        }
        return false;
    }

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

    public GridCell GetGridCell(int x, int y)
        => gameGrid[x, y].GetComponent<GridCell>();


    public GridCell GetGridCell(Position p)
        => gameGrid[p.x, p.y].GetComponent<GridCell>();


    public GridCell GetGridCell(Tuple<int, int> pos)
        => gameGrid[pos.Item1, pos.Item2].GetComponent<GridCell>();


    public void AddToCamp(GameObject piece)
    {
        Piece p = piece.GetComponent<Piece>();
        if (p.GetIsBlack())
        {
            p.GetComponentInChildren<Transform>().rotation = Quaternion.Euler(0, 180, 0);
            p.MovePiece(new(100, 100));
            playerPieces.Add(p);
            botPieces.Remove(p);
            pCamp.AddToCamp(piece);
        }
        else
        {
            p.GetComponentInChildren<Transform>().rotation = Quaternion.Euler(0, 0, 0);
            p.MovePiece(new(200, 200));
            botPieces.Add(p);
            playerPieces.Remove(p);
            eCamp.AddToCamp(piece);
        }
    }

    public List<Piece> GetPlayerPieces()
        => playerPieces;

    public List<Piece> GetBotPieces()
        => botPieces;

    public Piece GetPlayerKing()
        => playerKing;

    public Piece GetBotKing()
        => botKing;

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
            for (int x = 0; x < width; x++)
            {
                pCamp.campGrid[x, y].GetComponentInChildren<SpriteRenderer>().material.color = defaultColor;
                eCamp.campGrid[x, y].GetComponentInChildren<SpriteRenderer>().material.color = defaultColor;
            }
        }
    }

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