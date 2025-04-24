using System;
using System.Collections.Generic;
using UnityEngine;

public class GridGame : MonoBehaviour
{
    [SerializeField] public int width, height;

    [SerializeField] private GameObject gridCell;

    [SerializeField] private Transform cameraPosition;

    [SerializeField] private float gridCellSize;

    [SerializeField] private GameObject piecePrefab;

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

    public void Start()
    {

        gridCellSize *= 2;
        fileManager = FindFirstObjectByType<FileManager>();
        //pCamp = FindFirstObjectByType<Camp>();
        var camps = FindObjectsByType<Camp>(FindObjectsSortMode.InstanceID);
        pCamp = camps[0];
        eCamp = camps[1];
        pCamp.InitializePosY(2);
        eCamp.InitializePosY(0);
        //pCamp.InitializeGrid(2, gridCellSize, gridCell);
        //eCamp.InitializeGrid(0, gridCellSize, gridCell);
        GenerateField();
        InitializePieces();
        playerCampPosY = 2;
        enemyCampPosY = 0;
    }

    public void GenerateField()
    {

        pCamp.GenerateCamp();
        //playerCamp = new GameObject[9, 3];
        //for (int x = 0; x < 9; x++)
        //{
        //    for (int y = 0; y < 3; y++)
        //    {
        //        playerCamp[x, y] = Instantiate(gridCell, new Vector4(x * gridCellSize, 0, y * gridCellSize), Quaternion.identity);
        //        GridCell cell = playerCamp[x, y].GetComponent<GridCell>();
        //        cell.InitializeGridCell(x, y, gridCellSize);
        //        cell.SetPosition(x, y);
        //        playerCamp[x, y].transform.parent = transform;
        //        playerCamp[x, y].transform.rotation = Quaternion.Euler(90, 0, 0);
        //    }
        //}
        float campSpacing = 5.0F + gridCellSize * 3;
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
        campSpacing += 9 * gridCellSize + 5.0F;

        eCamp.GenerateCamp(campSpacing);
        //enemyCamp = new GameObject[9, 3];
        //for (int x = 0; x < 9; x++)
        //{
        //    for (int y = 0; y < 3; y++)
        //    {
        //        enemyCamp[x, y] = Instantiate(gridCell, new Vector4(x * gridCellSize, 0, y * gridCellSize + campSpacing), Quaternion.identity);
        //        GridCell cell = enemyCamp[x, y].GetComponent<GridCell>();
        //        cell.InitializeGridCell(x, y, gridCellSize);
        //        cell.SetPosition(x, y);
        //        enemyCamp[x, y].transform.parent = transform;
        //        enemyCamp[x, y].transform.rotation = Quaternion.Euler(90, 0, 0);
        //    }
        //}


        float xRot = 63.0F;
        float yRot = -90;

        cameraPosition.position = new Vector4((float)100, 80, ((height + 6) * gridCellSize + 10.0F) / 2 - gridCellSize / 2);
        cameraPosition.rotation = Quaternion.Euler(xRot, yRot, 0);
    }

    public void InitializePieces()
    {
        var piecesPositions = fileManager.PiecesPositions.boardPositions;

        foreach (var p in piecesPositions)
        {
            var cell = gameGrid[p.posX, p.posY].GetComponent<GridCell>();
            var resource = Resources.Load("ShogiPiece") as GameObject;
            var moveset = fileManager.GetMovesetByPieceName(p.piece);
            bool isSpecialPiece = SpecialPieceCheck(p.piece);
            cell.SetPiece(resource);
            var pieceScript = cell.objectInThisGridSpace.GetComponent<Piece>();
            pieceScript.InitializePiece(p.piece, moveset, cell.GetPosition().x, cell.GetPosition().y, isSpecialPiece);

            if (pieceScript.GetIsBlack())
            {
                cell.objectInThisGridSpace.GetComponentInChildren<MeshRenderer>().material.color = Color.black;
            }
            else
            {
                cell.objectInThisGridSpace.GetComponentInChildren<MeshRenderer>().material.color = Color.white;
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

    public GameObject GetPieceInGrid(Tuple<int, int> pos)
    {
        if (gameGrid[pos.Item1, pos.Item2].GetComponent<GridCell>().objectInThisGridSpace != null)
        {
            return gameGrid[pos.Item1, pos.Item2].GetComponentInChildren<GridCell>().objectInThisGridSpace;
        }
        else
        {
            return null;
        }
    }

    public GridCell GetGridCell(int x, int y)
    {
        return gameGrid[x, y].GetComponent<GridCell>();
    }

    public GridCell GetGridCell(Tuple<int, int> pos)
    {
        return gameGrid[pos.Item1, pos.Item2].GetComponent<GridCell>();
    }

    public void AddToCamp(GameObject piece)
    {
        if (piece.GetComponent<Piece>().GetIsBlack())
        {
            pCamp.AddToCamp(piece);
        }
        else
        {
            eCamp.AddToCamp(piece);
        }
    }

    public void ReshuffleCamp(GameObject[,] camp)
    {

    }

    public void OnHoverExitRestoreDefaultColor()
    {
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                gameGrid[x, y].GetComponentInChildren<SpriteRenderer>().material.color = Color.white;
            }
        }
        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < width; x++)
            {
                pCamp.campGrid[x, y].GetComponentInChildren<SpriteRenderer>().material.color = Color.white;
                eCamp.campGrid[x, y].GetComponentInChildren<SpriteRenderer>().material.color = Color.white;
                //playerCamp[x, y].GetComponentInChildren<SpriteRenderer>().material.color = Color.white;
                //enemyCamp[x, y].GetComponentInChildren<SpriteRenderer>().material.color = Color.white;
            }
        }
    }

    public void ClearPossibleMoves(IList<Tuple<int, int>> blacklist = null)
    {
        if (blacklist == null)
        {
            OnHoverExitRestoreDefaultColor();
        }
        else
        {
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    gameGrid[x, y].GetComponentInChildren<SpriteRenderer>().material.color = Color.white;
                }
            }
            foreach (var item in blacklist)
            {
                gameGrid[item.Item1, item.Item2].GetComponentInChildren<SpriteRenderer>().material.color = Color.black;
            }
        }
    }

    public void DisplayBoardState()
    {
        string whole = "";
        for (int y = 8; y >= 0; y--)
        {
            string rowStr = "|";
            for (var x = 0; x < 9; x++)
            {
                var cell = GetGridCell(x, y);
                if (cell.objectInThisGridSpace != null)
                {
                    rowStr += $"[{cell.objectInThisGridSpace.GetComponent<Piece>().GetName().Substring(0, 1)}]";
                }
                else
                {
                    rowStr += "[ ]";
                }
            }
            rowStr += "|";
            whole += rowStr + "\n";
        }

        Debug.Log(whole);
    }
}